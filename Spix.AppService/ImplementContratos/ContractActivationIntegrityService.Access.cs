using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos;

//LOS METODOS DE ENTRADA. Aca vive la unica pregunta "de que tipo es este equipo".
//
//Antes esa pregunta estaba repartida en 16 sitios como UsesHotSpotControlAsync(corporationId),
//y como devolvia un bool, PPPoE y Ninguno caian en la misma rama: el contrato pasaba a
//Suspendido y el cliente seguia navegando.
//
//Ahora se resuelve por el SERVIDOR del contrato, y el que llama solo llama.
public partial class ContractActivationIntegrityService
{
    //Como trabaja el equipo por el que sale este contrato.
    //
    //Sale de ContractServer, que existe para los dos mundos. Un contrato sin servidor
    //asignado es Ninguno: no hay equipo a quien pedirle nada.
    public async Task<MikrotikControlType> ResolveControlAsync(Guid contractClientId)
    {
        //El OrderBy no es decoracion: sin el, si un contrato llegara a tener dos servidores,
        //el tipo de control que sale depende del orden que decida la base y puede cambiar
        //entre dos llamadas. Lo que impide que haya dos es ContractServerService.AddAsync;
        //esto es el cinturon por si alguna fila vieja ya venia duplicada.
        var tipo = await _context.ContractServers
            .AsNoTracking()
            .Where(x => x.ContractClientId == contractClientId)
            .OrderBy(x => x.ContractServerId)
            .Select(x => (MikrotikControlType?)x.Server!.ControlMk)
            .FirstOrDefaultAsync();

        return tipo ?? MikrotikControlType.Ninguno;
    }

    public async Task<MikrotikControlType> ResolveControlByServerAsync(Guid serverId)
    {
        var tipo = await _context.Servers
            .AsNoTracking()
            .Where(x => x.ServerId == serverId)
            .Select(x => (MikrotikControlType?)x.ControlMk)
            .FirstOrDefaultAsync();

        return tipo ?? MikrotikControlType.Ninguno;
    }

    // ---------- Validar que el contrato este armado ----------

    public async Task<ActionResponse<bool>> ValidateAsync(Guid contractClientId)
    {
        var control = await ResolveControlAsync(contractClientId);

        return control switch
        {
            MikrotikControlType.HotSpot => await ValidateHotSpotAsync(contractClientId),
            MikrotikControlType.PPPoE => await ValidatePppoeAsync(contractClientId),
            _ => Success()
        };
    }

    // ---------- Que impide pasar a un estado ----------

    //Devuelve el NOMBRE de la clave de Resource, no el texto: el que llama lo localiza.
    //
    //Esta regla vivia en ContractStateRules y era la unica del flujo que no preguntaba el
    //tipo de control: exigia IpBinding para activar CUALQUIER contrato. Bajo PPPoE eso
    //bloquearia todas las activaciones con un mensaje que habla de HotSpot.
    public async Task<string?> GetBlockingReasonAsync(Guid contractClientId, ContractState target)
    {
        var control = await ResolveControlAsync(contractClientId);

        if (control == MikrotikControlType.Ninguno)
        {
            return null;
        }

        var hasQueue = await _context.ContractQues
            .AsNoTracking()
            .AnyAsync(x => x.ContractClientId == contractClientId);

        var hasAccess = control == MikrotikControlType.HotSpot
            ? await _context.ContractBinds.AsNoTracking().AnyAsync(x => x.ContractClientId == contractClientId)
            : await _context.ContractPppoes.AsNoTracking().AnyAsync(x => x.ContractClientId == contractClientId);

        var faltaClave = control == MikrotikControlType.HotSpot
            ? nameof(Resource.ContractState_NeedsQueueAndBind)
            : nameof(Resource.ContractState_NeedsQueueAndPppoe);

        return target switch
        {
            //Cerrar el contrato: no puede quedar nada suyo registrado en el MikroTik
            ContractState.Cancelled or ContractState.Terminated =>
                hasAccess || hasQueue ? nameof(Resource.ContractState_NeedsCleanMikrotik) : null,

            //Activar: tiene que tener su Queue y su acceso armados
            ContractState.Active =>
                !hasQueue || !hasAccess ? faltaClave : null,

            _ => null
        };
    }

    // ---------- El servicio esta prendido en el equipo ----------

    //Reemplaza la comprobacion de "IpBinding en bypassed" que estaba copiada en
    //ContractSuspendedService y en ContractExemptService.
    public async Task<bool> IsServiceOnAsync(Guid contractClientId)
    {
        var control = await ResolveControlAsync(contractClientId);

        if (control == MikrotikControlType.HotSpot)
        {
            var bind = await _context.ContractBinds
                .AsNoTracking()
                .Where(x => x.ContractClientId == contractClientId)
                .Select(x => new { x.HotSpotTypeId })
                .FirstOrDefaultAsync();

            var bypassed = await _context.HotSpotTypes
                .AsNoTracking()
                .Where(x => x.Active && x.TypeName == "bypassed")
                .Select(x => x.HotSpotTypeId)
                .FirstOrDefaultAsync();

            return bind != null && bind.HotSpotTypeId == bypassed;
        }

        if (control == MikrotikControlType.PPPoE)
        {
            return await _context.ContractPppoes
                .AsNoTracking()
                .AnyAsync(x => x.ContractClientId == contractClientId &&
                               x.PppoeAccessState == PppoeAccessState.Activo);
        }

        //Sin control en el equipo no hay nada que comprobar: el estado lo manda la base
        return true;
    }

    // ---------- Devolver y quitar el acceso ----------

    public async Task<ActionResponse<bool>> ActivateAsync(ContractClient contract)
    {
        var control = await ResolveControlAsync(contract.ContractClientId);

        return control switch
        {
            MikrotikControlType.HotSpot => await ActivateHotSpotBindingsAsync(contract),
            MikrotikControlType.PPPoE => await ActivatePppoeAsync(contract),
            _ => Success()
        };
    }

    public async Task<ActionResponse<bool>> ActivateAsync(IEnumerable<ContractClient> contracts)
    {
        //Cada contrato puede vivir en un equipo distinto, asi que se agrupa por tipo
        var porTipo = await AgruparPorControlAsync(contracts);

        foreach (var grupo in porTipo)
        {
            ActionResponse<bool> respuesta = grupo.Key switch
            {
                MikrotikControlType.HotSpot => await ActivateHotSpotManyAsync(grupo.Value),
                MikrotikControlType.PPPoE => await ActivatePppoeAsync(grupo.Value),
                _ => Success()
            };

            if (!respuesta.WasSuccess) return respuesta;
        }

        return Success();
    }

    public async Task<ActionResponse<bool>> SuspendAsync(ContractClient contract)
    {
        var control = await ResolveControlAsync(contract.ContractClientId);

        return control switch
        {
            MikrotikControlType.HotSpot => await SuspendHotSpotBindingsAsync(contract),
            MikrotikControlType.PPPoE => await SuspendPppoeAsync(contract),
            _ => Success()
        };
    }

    public async Task<ActionResponse<bool>> SuspendAsync(IEnumerable<ContractClient> contracts)
    {
        var porTipo = await AgruparPorControlAsync(contracts);

        foreach (var grupo in porTipo)
        {
            ActionResponse<bool> respuesta = grupo.Key switch
            {
                MikrotikControlType.HotSpot => await SuspendHotSpotBindingsAsync(grupo.Value),
                MikrotikControlType.PPPoE => await SuspendPppoeAsync(grupo.Value),
                _ => Success()
            };

            if (!respuesta.WasSuccess) return respuesta;
        }

        return Success();
    }

    // ---------- Se puede hablar con los equipos ----------

    public async Task<ActionResponse<bool>> VerifyConnectionAsync(ContractClient contract)
    {
        var control = await ResolveControlAsync(contract.ContractClientId);

        return control switch
        {
            MikrotikControlType.HotSpot => await VerifyHotSpotBindingsConnectionAsync(contract),
            MikrotikControlType.PPPoE => await VerifyPppoeConnectionAsync(contract),
            _ => Success()
        };
    }

    public async Task<ActionResponse<bool>> VerifyConnectionAsync(IEnumerable<Guid> contractClientIds)
    {
        var ids = contractClientIds.Distinct().ToList();
        if (ids.Count == 0) return Success();

        //Los equipos de esos contratos, cada uno con su tipo, en una sola consulta
        var servidores = await _context.ContractServers
            .AsNoTracking()
            .Where(x => ids.Contains(x.ContractClientId))
            .Select(x => x.Server!.ControlMk)
            .Distinct()
            .ToListAsync();

        if (servidores.Contains(MikrotikControlType.HotSpot))
        {
            var respuesta = await VerifyHotSpotServersConnectionAsync(ids);
            if (!respuesta.WasSuccess) return respuesta;
        }

        if (servidores.Contains(MikrotikControlType.PPPoE))
        {
            var respuesta = await VerifyPppoeServersConnectionAsync(ids);
            if (!respuesta.WasSuccess) return respuesta;
        }

        return Success();
    }

    // ---------- Lo que la pantalla y los lotes necesitan saber ----------

    //Las piezas que le faltan al contrato. La cuenta vive aca y viaja al front, para que
    //no haya que repetirla en Blazor y en el escritorio.
    public async Task<List<string>> MissingItemsAsync(Guid contractClientId)
    {
        var faltan = new List<string>();
        var control = await ResolveControlAsync(contractClientId);

        if (control == MikrotikControlType.Ninguno) return faltan;

        if (!await _context.ContractQues.AsNoTracking().AnyAsync(x => x.ContractClientId == contractClientId))
        {
            faltan.Add(_localizer[nameof(Resource.ContractQue)]);
        }

        if (control == MikrotikControlType.HotSpot)
        {
            if (!await _context.ContractBinds.AsNoTracking().AnyAsync(x => x.ContractClientId == contractClientId))
            {
                faltan.Add(_localizer[nameof(Resource.ContractBind)]);
            }
        }
        else
        {
            if (!await _context.ContractPppoes.AsNoTracking().AnyAsync(x => x.ContractClientId == contractClientId))
            {
                faltan.Add(_localizer[nameof(Resource.ContractPppoe)]);
            }
        }

        return faltan;
    }

    // ---------- Interno ----------

    //Un lote puede traer contratos de equipos de distinto tipo. Se agrupan para abrir una
    //sola pasada por tipo, y cada implementacion sigue agrupando por servidor adentro.
    private async Task<Dictionary<MikrotikControlType, List<ContractClient>>> AgruparPorControlAsync(
        IEnumerable<ContractClient> contracts)
    {
        var lista = contracts
            .GroupBy(x => x.ContractClientId)
            .Select(x => x.First())
            .ToList();

        var resultado = new Dictionary<MikrotikControlType, List<ContractClient>>();
        if (lista.Count == 0) return resultado;

        var ids = lista.Select(x => x.ContractClientId).ToList();

        var tipos = await _context.ContractServers
            .AsNoTracking()
            .Where(x => ids.Contains(x.ContractClientId))
            .Select(x => new { x.ContractClientId, x.Server!.ControlMk })
            .ToListAsync();

        var tipoPorContrato = tipos
            .GroupBy(x => x.ContractClientId)
            .ToDictionary(x => x.Key, x => x.First().ControlMk);

        foreach (var contrato in lista)
        {
            var tipo = tipoPorContrato.TryGetValue(contrato.ContractClientId, out var t)
                ? t
                : MikrotikControlType.Ninguno;

            if (!resultado.TryGetValue(tipo, out var grupo))
            {
                grupo = new List<ContractClient>();
                resultado[tipo] = grupo;
            }

            grupo.Add(contrato);
        }

        return resultado;
    }

    //El lote de HotSpot para activar: no existia porque la activacion masiva lo hacia
    //contrato por contrato desde ActivationService. Aca se conserva ese mismo
    //comportamiento, uno por uno, para no cambiar como aguanta los fallos.
    private async Task<ActionResponse<bool>> ActivateHotSpotManyAsync(IEnumerable<ContractClient> contracts)
    {
        foreach (var contrato in contracts)
        {
            var respuesta = await ActivateHotSpotBindingsAsync(contrato);
            if (!respuesta.WasSuccess) return respuesta;
        }

        return Success();
    }
}
