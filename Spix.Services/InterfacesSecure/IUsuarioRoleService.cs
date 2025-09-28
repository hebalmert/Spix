using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;

namespace Spix.Services.InterfacesSecure;

public interface IUsuarioRoleService
{
    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<UsuarioRole>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<UsuarioRole>> GetAsync(int id);

    Task<ActionResponse<UsuarioRole>> AddAsync(UsuarioRole modelo, string Email);

    Task<ActionResponse<bool>> DeleteAsync(int id);
}