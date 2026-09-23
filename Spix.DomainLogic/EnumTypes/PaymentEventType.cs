namespace Spix.DomainLogic.EnumTypes;

//Los movimientos de dinero que se anotan en la bitacora financiera
public enum PaymentEventType
{
    //Pagos por adelantado
    PrePaymentCreated = 1,
    PrePaymentUpdated = 2,
    PrePaymentDeleted = 3,
    PrePaymentApplied = 4,

    //Exoneraciones del mes
    ExoneratedCreated = 10,
    ExoneratedClosed = 11,
    ExoneratedApplied = 12,

    //Notas de cobro
    BillCreated = 20,
    BillCancelled = 21,

    //Recaudo
    PaymentReceived = 30,
    PaymentReversed = 31,

    //Contratistas: la comision que se causa al recibir el pago y la liquidacion que se le hace
    ContractorAccrued = 40,
    ContractorPaid = 41
}
