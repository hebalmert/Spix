namespace Spix.Domain.EntitiesBilling;

//Lo que el CLIENTE ve de su factura. Son DTOs recortados a proposito: aqui solo viaja lo
//que su pantalla muestra, nada de la oficina.

//Una factura en el listado del cliente
public class MyBillItemDto
{
    public Guid SellId { get; set; }

    public string? InvoiceNumber { get; set; }

    public DateTime DateSell { get; set; }

    public long ControlContrato { get; set; }

    public decimal Total { get; set; }

    //Lo que todavia debe de esa factura
    public decimal Balance { get; set; }

    public bool Paid { get; set; }

    public DateTime? DatePaid { get; set; }
}

//El detalle de una factura: de que se compone
public class MyBillDetailDto
{
    public Guid SellId { get; set; }

    public string? InvoiceNumber { get; set; }

    public DateTime DateSell { get; set; }

    public long ControlContrato { get; set; }

    public string? Address { get; set; }

    public string? ZoneName { get; set; }

    public decimal SubTotal { get; set; }

    public decimal TotalTax { get; set; }

    public decimal Total { get; set; }

    public decimal Balance { get; set; }

    public bool Paid { get; set; }

    public DateTime? DatePaid { get; set; }

    public List<MyBillLineDto> Lines { get; set; } = new();
}

//Un renglon de la factura
public class MyBillLineDto
{
    public string? Concept { get; set; }

    //De donde sale el renglon: el plan del mes o un servicio que se le hizo
    public string? Origin { get; set; }

    public decimal Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Total { get; set; }
}

//Lo que el cliente debe hoy: es el numerito de la tarjeta del portal
public class MyBillSummaryDto
{
    public int Pending { get; set; }

    public decimal Balance { get; set; }
}
