using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesSecure;
using Spix.UnitOfWork.InterfacesSecure;

namespace Spix.UnitOfWork.ImplementSecure;

public class UsuarioRoleUnitOfWork : IUsuarioRoleUnitOfWork
{
    private readonly IUsuarioRoleService _usuarioRoleService;

    public UsuarioRoleUnitOfWork(IUsuarioRoleService usuarioRoleService)
    {
        _usuarioRoleService = usuarioRoleService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync() => await _usuarioRoleService.ComboAsync();

    public async Task<ActionResponse<IEnumerable<UsuarioRole>>> GetAsync(PaginationDTO pagination) => await _usuarioRoleService.GetAsync(pagination);

    public async Task<ActionResponse<UsuarioRole>> GetAsync(int id) => await _usuarioRoleService.GetAsync(id);

    public async Task<ActionResponse<UsuarioRole>> AddAsync(UsuarioRole modelo, string Email) => await _usuarioRoleService.AddAsync(modelo, Email);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _usuarioRoleService.DeleteAsync(id);
}