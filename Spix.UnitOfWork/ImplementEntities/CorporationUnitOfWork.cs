using Spix.Domain.Entities;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfaceEntities;
using Spix.UnitOfWork.InterfaceEntities;

namespace Spix.UnitOfWork.ImplementEntities;

public class CorporationUnitOfWork : ICorporationUnitOfWork
{
    private readonly ICorporationService _corporationService;

    public CorporationUnitOfWork(ICorporationService corporationService)
    {
        _corporationService = corporationService;
    }

    public async Task<ActionResponse<IEnumerable<Corporation>>> ComboAsync() => await _corporationService.ComboAsync();

    public async Task<ActionResponse<IEnumerable<Corporation>>> GetAsync(PaginationDTO pagination) => await _corporationService.GetAsync(pagination);

    public async Task<ActionResponse<Corporation>> GetAsync(int id) => await _corporationService.GetAsync(id);

    public async Task<ActionResponse<Corporation>> UpdateAsync(Corporation modelo) => await _corporationService.UpdateAsync(modelo);

    public async Task<ActionResponse<Corporation>> AddAsync(Corporation modelo) => await _corporationService.AddAsync(modelo);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _corporationService.DeleteAsync(id);
}