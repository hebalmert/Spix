using Spix.Domain.Entities;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfaceEntities;
using Spix.UnitOfWork.InterfaceEntities;

namespace Spix.UnitOfWork.ImplementEntities;

public class SoftPlanUnitOfWork : ISoftPlanUnitOfWork
{
    private readonly ISoftPlanService _softPlanService;

    public SoftPlanUnitOfWork(ISoftPlanService softPlanService)
    {
        _softPlanService = softPlanService;
    }

    public async Task<ActionResponse<IEnumerable<SoftPlan>>> ComboAsync() => await _softPlanService.ComboAsync();

    public async Task<ActionResponse<IEnumerable<SoftPlan>>> GetAsync(PaginationDTO pagination) => await _softPlanService.GetAsync(pagination);

    public async Task<ActionResponse<SoftPlan>> GetAsync(int id) => await _softPlanService.GetAsync(id);

    public async Task<ActionResponse<SoftPlan>> UpdateAsync(SoftPlan modelo) => await _softPlanService.UpdateAsync(modelo);

    public async Task<ActionResponse<SoftPlan>> AddAsync(SoftPlan modelo) => await _softPlanService.AddAsync(modelo);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _softPlanService.DeleteAsync(id);
}