namespace Spix.DomainLogic.EntitiesInvenDTO;

//Una bodega en el listado web, con sus existencias ya contadas por la base
public class StorageListItemDto
{
    public Guid ProductStorageId { get; set; }

    public string StorageName { get; set; } = null!;

    public string? StateName { get; set; }

    public string? CityName { get; set; }

    public bool Active { get; set; }

    //Productos distintos con existencia en la bodega
    public int Products { get; set; }

    //Unidades en existencia, sumando todos los productos
    public decimal Units { get; set; }
}
