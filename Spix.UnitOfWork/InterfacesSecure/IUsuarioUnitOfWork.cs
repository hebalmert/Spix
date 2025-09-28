using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.UnitOfWork.InterfacesSecure;

public interface IUsuarioUnitOfWork
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<Usuario>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Usuario>> GetAsync(int id);

    Task<ActionResponse<Usuario>> UpdateAsync(Usuario modelo, string UrlFront);

    Task<ActionResponse<Usuario>> AddAsync(Usuario modelo, string urlFront, string username);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}