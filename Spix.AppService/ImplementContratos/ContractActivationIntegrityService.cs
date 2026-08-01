using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;
using Spix.xNetwork.MkHelper;

namespace Spix.AppService.ImplementContratos;

public class ContractActivationIntegrityService : IContractActivationIntegrityService
{
    private const string HotSpotRequirementsMessage = "No se puede cambiar el contrato a Active o Suspended porque la corporacion usa MikroTik HotSpot y el contrato no tiene Contract Queue e IpBinding configurados.";
    private const string ContractBindRequiredMessage = "No se puede cambiar el contrato a Active o Suspended porque la corporacion usa MikroTik HotSpot y el contrato no tiene IpBinding configurado.";
    private const string ContractQueueRequiredMessage = "No se puede cambiar el contrato a Active o Suspended porque la corporacion usa MikroTik HotSpot y el contrato no tiene Contract Queue configurado.";

    private readonly DataContext _context;
    private readonly IStringLocalizer _localizer;

    public ContractActivationIntegrityService(DataContext context, IStringLocalizer localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<ActionResponse<bool>> ValidateAsync(Guid contractClientId, int corporationId)
    {
        var usesHotSpotControl = await UsesHotSpotControlAsync(corporationId);

        if (!usesHotSpotControl)
        {
            return Success();
        }

        var hasContractBind = await _context.ContractBinds
            .AsNoTracking()
            .AnyAsync(x => x.ContractClientId == contractClientId);

        var hasContractQueue = await _context.ContractQues
            .AsNoTracking()
            .AnyAsync(x => x.ContractClientId == contractClientId);

        if (!hasContractBind && !hasContractQueue)
        {
            return Fail(HotSpotRequirementsMessage);
        }

        if (!hasContractBind)
        {
            return Fail(ContractBindRequiredMessage);
        }

        if (!hasContractQueue)
        {
            return Fail(ContractQueueRequiredMessage);
        }

        return Success();
    }

    public async Task<bool> UsesHotSpotControlAsync(int corporationId)
    {
        return await _context.ConnectionMikrotikControls
            .AsNoTracking()
            .AnyAsync(x => x.CorporationId == corporationId &&
                           x.MikrotikControlType == MikrotikControlType.HotSpot);
    }

    public async Task<ActionResponse<bool>> ActivateHotSpotBindingsAsync(ContractClient contract)
    {
        return await UpdateHotSpotBindingsAsync(contract, "bypassed", "activar");
    }

    public async Task<ActionResponse<bool>> VerifyHotSpotBindingsConnectionAsync(ContractClient contract)
    {
        var bindings = await _context.ContractBinds
            .Include(x => x.Server)
                .ThenInclude(x => x!.IpNetwork)
            .Include(x => x.IpNet)
            .Include(x => x.CargueDetail)
            .Where(x => x.ContractClientId == contract.ContractClientId)
            .ToListAsync();

        if (bindings.Count == 0)
        {
            return Fail("No se puede comunicar con MikroTik porque el contrato no tiene IpBinding configurado.");
        }

        foreach (var binding in bindings)
        {
            if (binding.Server?.IpNetwork?.Ip == null ||
                binding.IpNet?.Ip == null ||
                binding.CargueDetail?.MacWlan == null ||
                string.IsNullOrWhiteSpace(binding.MikrotikId))
            {
                return Fail("El IpBinding del contrato no tiene la informacion necesaria para comunicarse con MikroTik.");
            }

            MK? mikrotik = null;
            try
            {
                mikrotik = new MK(binding.Server.IpNetwork.Ip, binding.Server.ApiPort);
                if (!mikrotik.Login(binding.Server.Usuario, binding.Server.Clave))
                {
                    return Fail(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

            }
            catch
            {
                return Fail(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
            }
            finally
            {
                if (mikrotik != null)
                {
                    try
                    {
                        mikrotik.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        return Success();
    }

    public async Task<ActionResponse<bool>> VerifyHotSpotServersConnectionAsync(
        IEnumerable<Guid> contractClientIds)
    {
        var contractIds = contractClientIds.Distinct().ToList();
        if (contractIds.Count == 0)
        {
            return Success();
        }

        var servers = await _context.ContractBinds
            .Where(x => contractIds.Contains(x.ContractClientId))
            .Include(x => x.Server)
                .ThenInclude(x => x!.IpNetwork)
            .Select(x => new
            {
                x.ServerId,
                ServerName = x.Server!.ServerName,
                ServerIp = x.Server.IpNetwork!.Ip,
                x.Server.Usuario,
                x.Server.Clave,
                x.Server.ApiPort
            })
            .ToListAsync();

        var unavailableServers = new List<string>();
        var uniqueServers = servers
            .GroupBy(x => x.ServerId)
            .Select(x => x.First())
            .ToList();

        foreach (var server in uniqueServers)
        {
            var serverDescription = $"{server.ServerName} ({server.ServerIp})";
            if (string.IsNullOrWhiteSpace(server.ServerIp))
            {
                unavailableServers.Add(serverDescription);
                continue;
            }

            MK? mikrotik = null;
            try
            {
                mikrotik = new MK(server.ServerIp, server.ApiPort);
                if (!mikrotik.Login(server.Usuario, server.Clave))
                {
                    unavailableServers.Add(serverDescription);
                }
            }
            catch
            {
                unavailableServers.Add(serverDescription);
            }
            finally
            {
                if (mikrotik != null)
                {
                    try
                    {
                        mikrotik.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        if (unavailableServers.Count > 0)
        {
            var serversText = string.Join(", ", unavailableServers.Distinct());
            return Fail($"No se puede continuar con el corte general. Los siguientes servidores no responden o estan fuera de linea: {serversText}.");
        }

        return Success();
    }

    public async Task<ActionResponse<bool>> SuspendHotSpotBindingsAsync(ContractClient contract)
    {
        return await SuspendHotSpotBindingsAsync(new List<ContractClient> { contract });
    }

    public async Task<ActionResponse<bool>> SuspendHotSpotBindingsAsync(IEnumerable<ContractClient> contracts)
    {
        var contractList = contracts
            .GroupBy(x => x.ContractClientId)
            .Select(x => x.First())
            .ToList();
        if (contractList.Count == 0)
        {
            return Success();
        }

        var contractIds = contractList.Select(x => x.ContractClientId).ToList();
        var contractsById = contractList.ToDictionary(x => x.ContractClientId);
        var bindings = await _context.ContractBinds
            .Include(x => x.Server)
                .ThenInclude(x => x!.IpNetwork)
            .Include(x => x.IpNet)
            .Include(x => x.CargueDetail)
            .Where(x => contractIds.Contains(x.ContractClientId))
            .ToListAsync();

        var regularType = await _context.HotSpotTypes
            .FirstOrDefaultAsync(x => x.Active && x.TypeName == "regular");
        if (regularType == null)
        {
            return Fail("No existe un tipo de HotSpot activo con el valor regular.");
        }

        foreach (var binding in bindings)
        {
            if (binding.Server?.IpNetwork?.Ip == null ||
                binding.IpNet?.Ip == null ||
                binding.CargueDetail?.MacWlan == null ||
                string.IsNullOrWhiteSpace(binding.MikrotikId) ||
                !contractsById.ContainsKey(binding.ContractClientId))
            {
                return Fail("Uno de los IpBinding no tiene la informacion necesaria para suspender el acceso.");
            }
        }

        var bindingsByServer = bindings
            .GroupBy(x => x.ServerId)
            .ToList();

        foreach (var serverBindings in bindingsByServer)
        {
            var firstBinding = serverBindings.First();
            var server = firstBinding.Server!;
            MK? mikrotik = null;

            try
            {
                mikrotik = new MK(server.IpNetwork!.Ip!, server.ApiPort);
                if (!mikrotik.Login(server.Usuario, server.Clave))
                {
                    return Fail(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                foreach (var binding in serverBindings)
                {
                    var contract = contractsById[binding.ContractClientId];
                    var clientName = $"{contract.Client?.FirstName} {contract.Client?.LastName} - ({contract.ControlContrato})";

                    mikrotik.Send("/ip/hotspot/ip-binding/set");
                    mikrotik.Send("=.id=" + binding.MikrotikId);
                    mikrotik.Send("=address=" + binding.IpNet!.Ip);
                    mikrotik.Send("=to-address=" + binding.IpNet.Ip);
                    mikrotik.Send("=comment=" + clientName);
                    mikrotik.Send("=mac-address=" + binding.CargueDetail!.MacWlan);
                    mikrotik.Send("=server=all");
                    mikrotik.Send("=type=" + regularType.TypeName, true);
                    mikrotik.Read();

                    binding.HotSpotTypeId = regularType.HotSpotTypeId;
                }
            }
            catch
            {
                return Fail(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
            }
            finally
            {
                if (mikrotik != null)
                {
                    try
                    {
                        mikrotik.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        return Success();
    }

    private async Task<ActionResponse<bool>> UpdateHotSpotBindingsAsync(
        ContractClient contract,
        string hotSpotTypeName,
        string operationName)
    {
        var bindings = await _context.ContractBinds
            .Include(x => x.Server)
                .ThenInclude(x => x!.IpNetwork)
            .Include(x => x.IpNet)
            .Include(x => x.CargueDetail)
            .Where(x => x.ContractClientId == contract.ContractClientId)
            .ToListAsync();

        if (bindings.Count == 0)
        {
            return Fail($"No se puede {operationName} el contrato porque no tiene IpBinding configurado.");
        }

        var hotSpotType = await _context.HotSpotTypes
            .FirstOrDefaultAsync(x => x.Active && x.TypeName == hotSpotTypeName);
        if (hotSpotType == null)
        {
            return Fail($"No existe un tipo de HotSpot activo con el valor {hotSpotTypeName}.");
        }

        foreach (var binding in bindings)
        {
            if (binding.Server?.IpNetwork?.Ip == null ||
                binding.IpNet?.Ip == null ||
                binding.CargueDetail?.MacWlan == null ||
                string.IsNullOrWhiteSpace(binding.MikrotikId))
            {
                return Fail($"El IpBinding del contrato no tiene la informacion necesaria para {operationName} el acceso.");
            }

            MK? mikrotik = null;
            try
            {
                mikrotik = new MK(binding.Server.IpNetwork.Ip, binding.Server.ApiPort);
                if (!mikrotik.Login(binding.Server.Usuario, binding.Server.Clave))
                {
                    return Fail(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
                }

                var clientName = $"{contract.Client?.FirstName} {contract.Client?.LastName} - ({contract.ControlContrato})";

                mikrotik.Send("/ip/hotspot/ip-binding/set");
                mikrotik.Send("=.id=" + binding.MikrotikId);
                mikrotik.Send("=address=" + binding.IpNet.Ip);
                mikrotik.Send("=to-address=" + binding.IpNet.Ip);
                mikrotik.Send("=comment=" + clientName);
                mikrotik.Send("=mac-address=" + binding.CargueDetail.MacWlan);
                mikrotik.Send("=server=all");
                mikrotik.Send("=type=" + hotSpotType.TypeName, true);
                mikrotik.Read();

                binding.HotSpotTypeId = hotSpotType.HotSpotTypeId;
            }
            catch
            {
                return Fail(_localizer[nameof(Resource.Mikrotik_Connection_Error)]);
            }
            finally
            {
                if (mikrotik != null)
                {
                    try
                    {
                        mikrotik.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        return Success();
    }

    private static ActionResponse<bool> Success()
    {
        return new ActionResponse<bool>
        {
            WasSuccess = true,
            Result = true
        };
    }

    private static ActionResponse<bool> Fail(string message)
    {
        return new ActionResponse<bool>
        {
            WasSuccess = false,
            Message = message
        };
    }
}
