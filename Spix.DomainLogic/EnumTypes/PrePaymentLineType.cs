namespace Spix.DomainLogic.EnumTypes;

//De donde sale una linea del pago adelantado
public enum PrePaymentLineType
{
    //El plan mensual del contrato
    Plan = 1,

    //Un servicio de una solicitud tecnica completada y no facturada
    Service = 2
}
