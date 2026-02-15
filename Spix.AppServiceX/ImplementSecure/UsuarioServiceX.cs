using Spix.AppService.InterfacesSecure;
using Spix.AppServiceX.InterfacesSecure;
using Spix.Domain.EntitesSoftSec;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.ServiceX.ImplementSecure;

public class UsuarioServiceX : IUsuarioServiceX
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioServiceX(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    public async Task<ActionResponse<IEnumerable<Usuario>>> GetAsync(PaginationDTO pagination, string username) => await _usuarioService.GetAsync(pagination, username);

    public async Task<ActionResponse<Usuario>> GetAsync(Guid id) => await _usuarioService.GetAsync(id);

    public async Task<ActionResponse<Usuario>> UpdateAsync(Usuario modelo, string urlFront) => await _usuarioService.UpdateAsync(modelo, urlFront);

    public async Task<ActionResponse<Usuario>> AddAsync(Usuario modelo, string urlFront, string username) => await _usuarioService.AddAsync(modelo, urlFront, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _usuarioService.DeleteAsync(id);
}