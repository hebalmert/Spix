using Spix.Domain.EntitesSoftSec;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesSecure;

public interface IUsuarioServiceX
{
    Task<ActionResponse<IEnumerable<Usuario>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Usuario>> GetAsync(Guid id);

    Task<ActionResponse<Usuario>> UpdateAsync(Usuario modelo, string UrlFront);

    Task<ActionResponse<Usuario>> AddAsync(Usuario modelo, string urlFront, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}