using Microsoft.EntityFrameworkCore;
using Spix.AppInfra;
using Spix.DomainLogic.EnumTypes;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos;

//Quien puede cambiar el estado de un contrato y con que condiciones.
//Una sola regla para todos: el modal de Control de Contratos y cualquier otro punto que lo necesite.
//
//Estas son las reglas de ESTE modulo. Suspender y reactivar tienen las suyas y viven en
//ContractSuspendedService: cada modulo maneja sus propias condiciones.
public static class ContractStateRules
{
    //A donde puede moverse un contrato segun donde esta hoy.
    //Borrador y Por aprobacion se manejan en el registro del contrato.
    //Anulado y Terminado son finales. Exento lo pone otro modulo, no este.
    //Una vez Activo, el contrato no vuelve a En progreso.
    public static List<ContractState> GetAllowedStates(ContractState current) => current switch
    {
        ContractState.InProgress => new() { ContractState.Cancelled },
        ContractState.Active => new() { ContractState.Terminated, ContractState.Cancelled },
        ContractState.Exempt => new() { ContractState.Active, ContractState.Terminated, ContractState.Cancelled },
        ContractState.Suspended => new() { ContractState.Active, ContractState.Terminated, ContractState.Cancelled },
        _ => new()
    };

    //Devuelve la clave del mensaje que impide el cambio, o null si se puede hacer.
    //Cada destino pide una cosa distinta, y se mira como esta el MikroTik ANTES del cambio:
    //  - Un contrato activo tiene su IpBinding en bypassed (pasa trafico).
    //  - Un contrato suspendido lo tiene en regular (lo manda al portal).
    public static async Task<string?> GetBlockingReasonAsync(DataContext context, Guid contractClientId, ContractState target)
    {
        var bind = await context.ContractBinds
            .AsNoTracking()
            .Where(x => x.ContractClientId == contractClientId)
            .Select(x => new { x.HotSpotTypeId })
            .FirstOrDefaultAsync();

        var hasQueue = await context.ContractQues
            .AsNoTracking()
            .AnyAsync(x => x.ContractClientId == contractClientId);

        return target switch
        {
            //Cerrar el contrato: no puede quedar nada suyo registrado en el MikroTik
            ContractState.Cancelled or ContractState.Terminated =>
                bind != null || hasQueue ? nameof(Resource.ContractState_NeedsCleanMikrotik) : null,

            //Activar: tiene que tener su Queue y su IpBinding armados; el proceso los deja
            //en bypassed para devolverle el servicio.
            ContractState.Active =>
                !hasQueue || bind == null
                    ? nameof(Resource.ContractState_NeedsQueueAndBind)
                    : null,

            _ => null
        };
    }
}
