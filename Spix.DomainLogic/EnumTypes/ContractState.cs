namespace Spix.DomainLogic.EnumTypes;

public enum ContractState
{
    Draft = 1,            // Creado pero incompleto        | Color: #0D6EFD (Azul)
    PendingApproval = 2,  // Procesando / Validando datos  | Color: #FD7E14 (Naranja)
    Active = 3,           // Servicio activo               | Color: #198754 (Verde)
    Exempt = 4,           // Exonerado temporalmente       | Color: #20C997 (Verde agua)
    Suspended = 5,        // Suspendido                    | Color: #DC3545 (Rojo)
    Cancelled = 6,        // Anulado antes de activarse    | Color: #B02A37 (Rojo oscuro)
    Terminated = 7        // Retirado / Finalizado         | Color: #6F42C1 (Púrpura)
}