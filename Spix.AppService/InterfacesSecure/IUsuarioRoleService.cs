using Spix.Domain.EntitesSoftSec;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesSecure;

public interface IUsuarioRoleService
{
    Task<ActionResponse<IEnumerable<IntNameModel>>> ComboAsync();

    Task<ActionResponse<IEnumerable<UsuarioRole>>> GetAsync(PaginationDTO pagination);

    Task<ActionResponse<UsuarioRole>> GetAsync(Guid id);

    Task<ActionResponse<UsuarioRole>> AddAsync(UsuarioRole modelo, string Email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}