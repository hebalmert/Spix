namespace Spix.DomainLogic.Pagination;

public class PaginationDTO
{
    public int Id { get; set; }

    public Guid? GuidId { get; set; }

    public int Page { get; set; } = 1;

    public int RecordsNumber { get; set; } = 15; 

    public string? Filter { get; set; }

    public string? DateStart { get; set; }

    public string? DateEnd { get; set; }
}