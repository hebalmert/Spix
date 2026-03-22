using Spix.AppService.InterfacesEntitiesData;
using Spix.AppServiceX.InterfacesEntitiesData;
using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.ImplementEntitiesData;

public class SecurityServiceX : ISecurityServiceX
{
    private readonly ISecurityService _securityService;

    public SecurityServiceX(ISecurityService securityService)
    {
        _securityService = securityService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync() => await _securityService.ComboAsync();

    public async Task<ActionResponse<IEnumerable<Security>>> GetAsync(PaginationDTO pagination) => await _securityService.GetAsync(pagination);

    public async Task<ActionResponse<Security>> GetAsync(int id) => await _securityService.GetAsync(id);

    public async Task<ActionResponse<Security>> UpdateAsync(Security modelo) => await _securityService.UpdateAsync(modelo);

    public async Task<ActionResponse<Security>> AddAsync(Security modelo) => await _securityService.AddAsync(modelo);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _securityService.DeleteAsync(id);
}