using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesGen;

public class ServiceCategoryServiceX : IServiceCategoryServiceX
{
    private readonly IServiceCategoryService _serviceCategoryService;

    public ServiceCategoryServiceX(IServiceCategoryService serviceCategoryService)
    {
        _serviceCategoryService = serviceCategoryService;
    }

    public async Task<ActionResponse<IEnumerable<ServiceCategory>>> ComboAsync(string username) => await _serviceCategoryService.ComboAsync(username);

    public async Task<ActionResponse<IEnumerable<ServiceCategory>>> GetAsync(PaginationDTO pagination, string username) => await _serviceCategoryService.GetAsync(pagination, username);

    public async Task<ActionResponse<ServiceCategory>> GetAsync(Guid id) => await _serviceCategoryService.GetAsync(id);

    public async Task<ActionResponse<ServiceCategory>> UpdateAsync(ServiceCategory modelo) => await _serviceCategoryService.UpdateAsync(modelo);

    public async Task<ActionResponse<ServiceCategory>> AddAsync(ServiceCategory modelo, string username) => await _serviceCategoryService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _serviceCategoryService.DeleteAsync(id);
}