namespace Spix.DomainLogic.EntitiesDTO;

public class TransferStockDTO
{
    public Guid TransferId { get; set; }

    public Guid ProductId { get; set; }

    public decimal DiponibleOrigen { get; set; }
}