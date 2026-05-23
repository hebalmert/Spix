namespace Spix.DomainLogic.EnumTypes;

public enum ContractState
{
    Draft = 1,            // Creado pero incompleto
    PendingApproval = 2,  // Procesando / Validando datos
    Active = 3,           // Servicio activo
    Exempt = 4,           // Exonerado temporalmente
    Suspended = 5,        // Suspendido por deuda o solicitud
    Cancelled = 6,        // Anulado antes de activarse
    Terminated = 7        // Retirado / Finalizado
}