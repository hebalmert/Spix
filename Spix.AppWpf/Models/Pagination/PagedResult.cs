namespace Spix.AppWpf.Models.Pagination;

// Representa una pagina de datos y la cantidad total de paginas enviada por el Backend.
public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();

    public int TotalPages { get; init; }
}
