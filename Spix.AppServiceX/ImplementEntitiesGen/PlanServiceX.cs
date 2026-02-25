using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesGen;
using Spix.Domain.Enum;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesGen;

public class PlanServiceX : IPlanServiceX
{
    private readonly IPlanService _planService;

    public PlanServiceX(IPlanService planService)
    {
        _planService = planService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboUpAsync() => await _planService.GetComboUpAsync();

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboDownAsync() => await _planService.GetComboDownAsync();

    public async Task<ActionResponse<IEnumerable<Plan>>> GetAsync(PaginationDTO pagination, string username) => await _planService.GetAsync(pagination, username);

    public async Task<ActionResponse<Plan>> GetAsync(Guid id) => await _planService.GetAsync(id);

    public async Task<ActionResponse<Plan>> UpdateAsync(Plan modelo) => await _planService.UpdateAsync(modelo);

    public async Task<ActionResponse<Plan>> AddAsync(Plan modelo, string username) => await _planService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _planService.DeleteAsync(id);
}