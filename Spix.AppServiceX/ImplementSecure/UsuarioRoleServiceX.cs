using Spix.AppService.InterfacesSecure;
using Spix.AppServiceX.InterfacesSecure;
using Spix.Domain.EntitesSoftSec;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.ServiceX.ImplementSecure;

public class UsuarioRoleServiceX : IUsuarioRoleServiceX
{
    private readonly IUsuarioRoleService _usuarioRoleService;

    public UsuarioRoleServiceX(IUsuarioRoleService usuarioRoleService)
    {
        _usuarioRoleService = usuarioRoleService;
    }

    public async Task<ActionResponse<IEnumerable<IntNameModel>>> ComboAsync() => await _usuarioRoleService.ComboAsync();

    public async Task<ActionResponse<IEnumerable<UsuarioRole>>> GetAsync(PaginationDTO pagination) => await _usuarioRoleService.GetAsync(pagination);

    public async Task<ActionResponse<UsuarioRole>> GetAsync(Guid id) => await _usuarioRoleService.GetAsync(id);

    public async Task<ActionResponse<UsuarioRole>> AddAsync(UsuarioRole modelo, string Email) => await _usuarioRoleService.AddAsync(modelo, Email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _usuarioRoleService.DeleteAsync(id);
}