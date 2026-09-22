namespace Spix.Domain.EntitiesPayment;

//Un servicio que el cliente puede adelantar: viene de una solicitud completada, no facturada,
//con valor mayor a cero y que no este ya reservada en otro pago adelantado.
public class PrePaymentServiceDto
{
    public Guid ServiceRequestId { get; set; }

    public Guid ServiceRequestDetailId { get; set; }

    public long RequestNumber { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public string? ServiceName { get; set; }

    public string? Detail { get; set; }

    public decimal TaxRate { get; set; }

    public decimal Price { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Total { get; set; }
}
