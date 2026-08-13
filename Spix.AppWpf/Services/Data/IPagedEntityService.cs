using Spix.AppWpf.Models.Pagination;

namespace Spix.AppWpf.Services.Data;

// Expone una consulta comun para los indices que el Backend entrega por pagina.
public interface IPagedEntityService<T>
{
    Task<PagedResult<T>> GetPageAsync(
        string endpoint,
        int page,
        int pageSize,
        string filter);
}
