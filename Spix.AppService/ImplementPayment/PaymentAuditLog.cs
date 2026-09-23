using Spix.AppInfra;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppService.ImplementPayment;

//La unica puerta para anotar en la bitacora del dinero. La usan los pagos adelantados,
//las exoneraciones, las notas de cobro y el recaudo.
//
//No guarda: quien llama hace SaveChanges dentro de SU transaccion. Asi, si el movimiento
//se cae a mitad, tampoco queda el renglon; y si se guarda, su rastro queda con el.
public static class PaymentAuditLog
{
    public static void Add(
        DataContext context,
        int corporationId,
        PaymentEventType eventType,
        Guid? contractClientId = null,
        Guid? clientId = null,
        Guid? referenceId = null,
        string? referenceType = null,
        decimal amount = 0,
        decimal taxAmount = 0,
        decimal total = 0,
        string? detail = null,
        string? userName = null,
        Guid? userId = null,
        string? sourceIp = null,
        string? userAgent = null)
    {
        context.PaymentAudits.Add(new PaymentAudit
        {
            PaymentAuditId = Guid.NewGuid(),
            DateEvent = DateTime.UtcNow,
            EventType = eventType,
            ContractClientId = contractClientId,
            ClientId = clientId,
            ReferenceId = referenceId,
            ReferenceType = Recortar(referenceType, 40),
            Amount = amount,
            TaxAmount = taxAmount,
            Total = total,
            Detail = Recortar(detail, 1000),
            UserId = userId,
            UserByName = Recortar(userName, 150),
            SourceIp = Recortar(sourceIp, 64),
            UserAgent = Recortar(userAgent, 512),
            CorporationId = corporationId
        });
    }

    //Un detalle largo no puede tumbar el movimiento que se esta auditando
    private static string? Recortar(string? valor, int largo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return valor.Length <= largo ? valor : valor.Substring(0, largo);
    }
}
