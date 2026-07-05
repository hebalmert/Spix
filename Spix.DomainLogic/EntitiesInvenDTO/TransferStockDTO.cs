namespace Spix.DomainLogic.EntitiesInvenDTO;

public class TransferStockDTO
{
    public Guid TransferId { get; set; }

    public Guid ProductId { get; set; }

    public decimal DiponibleOrigen { get; set; }
}
