using Spix.DomainLogic.EnumTypes;

namespace Spix.AppService.ImplementContratos;

//Quien puede cambiar el estado de un contrato.
//Una sola regla para todos: el modal de Control de Contratos y cualquier otro punto que lo necesite.
//
//Estas son las reglas de ESTE modulo. Suspender y reactivar tienen las suyas y viven en
//ContractSuspendedService: cada modulo maneja sus propias condiciones.
//
//Lo que el MikroTik exige para cada destino ya NO vive aca: se mudo a
//ContractActivationIntegrityService.GetBlockingReasonAsync, porque esa condicion depende de
//como trabaje el equipo (IpBinding en HotSpot, credencial en PPPoE) y esta clase no lo sabe.
//Mientras estuvo aca exigia IpBinding para activar CUALQUIER contrato.
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
}
