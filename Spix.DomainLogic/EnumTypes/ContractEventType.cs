namespace Spix.DomainLogic.EnumTypes;

//Los pasos que se anotan en la bitacora de un contrato (ContractAudit).
//Nunca se reutiliza un numero: si un paso deja de usarse, se deja su hueco.
public enum ContractEventType
{
    //===== Vida del contrato =====
    Created = 1,
    Updated = 2,
    StateChanged = 3,
    Approved = 4,
    Deleted = 5,

    //===== Firma electronica (espejo de ContractSignatureEvent) =====
    SignatureRequested = 10,
    DocumentViewed = 11,
    CodeSent = 12,
    CodeFailed = 13,
    CodeValidated = 14,
    Signed = 15,

    //===== Suspension =====
    Suspended = 20,
    Reactivated = 21,

    //===== Exoneracion fija =====
    Exempted = 30,
    ExemptRemoved = 31,

    //===== Exoneracion por mes =====
    MonthExonerated = 40,
    MonthExoneratedClosed = 41,

    //===== Configuracion tecnica =====
    BindCreated = 50,
    BindChanged = 51,
    QueueCreated = 52,
    //===== Ubicacion =====
    //El tecnico tomo las coordenadas en la visita
    LocationCaptured = 60,

    //Esa coordenada se aplico a la ubicacion del contrato
    LocationUpdated = 61,

    //Vino distinta de la registrada: queda para que alguien la revise
    LocationMismatch = 62,

    //===== Resultado de la visita =====
    //El tecnico fue y no habia nadie
    ClientAbsent = 63,

    //De una visita fallida salio otra nueva
    VisitRescheduled = 64
}
