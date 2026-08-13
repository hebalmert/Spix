using Spix.AppWpf.Models.Pagination;
using Spix.HttpService;

namespace Spix.AppWpf.Services.Data;

// Consume los endpoints paginados sin que cada vista repita URL, header y filtro.
public class PagedEntityService<T> : IPagedEntityService<T>
{
    private readonly IRepository _repository;

    public PagedEntityService(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<T>> GetPageAsync(
        string endpoint,
        int page,
        int pageSize,
        string filter)
    {
        var querySeparator = endpoint.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        var url = $"{endpoint}{querySeparator}page={page}&recordsnumber={pageSize}";

        if (!string.IsNullOrWhiteSpace(filter))
        {
            url += $"&filter={Uri.EscapeDataString(filter.Trim())}";
        }

        var responseHttp = await _repository.GetAsync<List<T>>(url);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(message)
                    ? "No fue posible cargar los registros."
                    : message.Trim().Trim('"'));
        }

        var totalPages = 0;
        responseHttp.HttpResponseMessage.Headers.TryGetValues(
            "Totalpages",
            out var totalPagesHeaders);

        var totalPagesHeader = totalPagesHeaders?.FirstOrDefault();

        _ = int.TryParse(totalPagesHeader, out totalPages);

        return new PagedResult<T>
        {
            Items = responseHttp.Response ?? new List<T>(),
            TotalPages = Math.Max(0, totalPages)
        };
    }
}
