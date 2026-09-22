namespace Spix.DomainLogic.EntitiesInvenDTO;

//Un proveedor en el listado web, con sus compras ya contadas por la base
public class SupplierListItemDto
{
    public Guid SupplierId { get; set; }

    public string Name { get; set; } = null!;

    public string? Document { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public bool Active { get; set; }

    public string? Photo { get; set; }

    public string? ImageFullPath { get; set; }

    public int Purchases { get; set; }

    public DateTime? LastPurchase { get; set; }
}
