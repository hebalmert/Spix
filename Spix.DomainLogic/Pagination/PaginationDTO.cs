namespace Spix.DomainLogic.Pagination;

public class PaginationDTO
{
    //Tope duro del tamano de pagina: sin esto un cliente puede pedir RecordsNumber=999999
    //y arrastrar toda la tabla en una sola peticion, tumbando la BD para todos los demas.
    public const int MaxRecordsNumber = 100;

    private int _page = 1;
    private int _recordsNumber = 15;

    public int Id { get; set; }

    public Guid? GuidId { get; set; }

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int RecordsNumber
    {
        get => _recordsNumber;
        set => _recordsNumber = value < 1 ? 15 : (value > MaxRecordsNumber ? MaxRecordsNumber : value);
    }

    public string? Filter { get; set; }

    public string? DateStart { get; set; }

    public string? DateEnd { get; set; }
}
