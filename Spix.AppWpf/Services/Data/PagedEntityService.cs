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

        var items = responseHttp.Response ?? new List<T>();

        //El Backend manda las paginas en la cabecera. Si por lo que sea no llega, se
        //calculan con el total de registros, y si tampoco esta, se deducen de lo que vino:
        //asi la barra de paginas nunca queda en "Pagina 1 de 0" con datos en pantalla.
        var totalPages = LeerEntero(responseHttp, "Totalpages");
        var totalRecords = LeerEntero(responseHttp, "Counting");

        if (totalPages <= 0 && totalRecords > 0)
        {
            totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        }

        if (totalPages <= 0 && items.Count > 0)
        {
            totalPages = items.Count < pageSize ? page : page + 1;
        }

        return new PagedResult<T>
        {
            Items = items,
            TotalPages = Math.Max(0, totalPages),
            TotalRecords = Math.Max(0, totalRecords)
        };
    }

    //Lee una cabecera numerica de la respuesta; devuelve 0 si no vino o no es un numero
    private static int LeerEntero(HttpResponseWrapper<List<T>> responseHttp, string header)
    {
        responseHttp.HttpResponseMessage.Headers.TryGetValues(header, out var valores);

        return int.TryParse(valores?.FirstOrDefault(), out var numero) ? numero : 0;
    }
}
