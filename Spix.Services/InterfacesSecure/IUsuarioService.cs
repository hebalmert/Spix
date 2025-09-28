using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesSecure;

public interface IUsuarioService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync(string email);

    Task<ActionResponse<IEnumerable<Usuario>>> GetAsync(PaginationDTO pagination, string Email);

    Task<ActionResponse<Usuario>> GetAsync(int id);

    Task<ActionResponse<Usuario>> UpdateAsync(Usuario modelo, string UrlFront);

    Task<ActionResponse<Usuario>> AddAsync(Usuario modelo, string urlFront, string Email);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}