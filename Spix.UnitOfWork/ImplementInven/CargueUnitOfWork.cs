using Spix.Domain.EntitiesInven;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesInven;
using Spix.UnitOfWork.InterfacesInven;

namespace Spix.UnitOfWork.ImplementInven;

public class CargueUnitOfWork : ICargueUnitOfWork
{
    private readonly ICargueService _cargueService;

    public CargueUnitOfWork(ICargueService cargueService)
    {
        _cargueService = cargueService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus() => await _cargueService.GetComboStatus();

    public async Task<ActionResponse<IEnumerable<Cargue>>> GetAsync(PaginationDTO pagination, string email) => await _cargueService.GetAsync(pagination, email);

    public async Task<ActionResponse<Cargue>> GetAsync(Guid id) => await _cargueService.GetAsync(id);

    public async Task<ActionResponse<Cargue>> UpdateAsync(Cargue modelo) => await _cargueService.UpdateAsync(modelo);

    public async Task<ActionResponse<Cargue>> AddAsync(Cargue modelo, string email) => await _cargueService.AddAsync(modelo, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _cargueService.DeleteAsync(id);
}