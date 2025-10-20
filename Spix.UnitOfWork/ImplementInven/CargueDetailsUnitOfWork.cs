using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesInven;
using Spix.UnitOfWork.InterfacesInven;

namespace Spix.UnitOfWork.ImplementInven;

public class CargueDetailsUnitOfWork : ICargueDetailsUnitOfWork
{
    private readonly ICargueDetailsService _cargueDetailsService;

    public CargueDetailsUnitOfWork(ICargueDetailsService cargueDetailsService)
    {
        _cargueDetailsService = cargueDetailsService;
    }

    public async Task<ActionResponse<IEnumerable<CargueDetail>>> GetAsync(PaginationDTO pagination, string email) => await _cargueDetailsService.GetAsync(pagination, email);

    public async Task<ActionResponse<IEnumerable<CargueDetail>>> GetSerialsAsync(PaginationDTO pagination, string email) => await _cargueDetailsService.GetSerialsAsync(pagination, email);

    public async Task<ActionResponse<CargueDetail>> GetAsync(Guid id) => await _cargueDetailsService.GetAsync(id);

    public async Task<ActionResponse<CargueDetail>> UpdateAsync(CargueDetail modelo) => await _cargueDetailsService.UpdateAsync(modelo);

    public async Task<ActionResponse<CargueDetail>> AddAsync(CargueDetail modelo, string email) => await _cargueDetailsService.AddAsync(modelo, email);

    public async Task<ActionResponse<Cargue>> CerrarCargueAsync(Guid id, string email) => await _cargueDetailsService.CerrarCargueAsync(id, email);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _cargueDetailsService.DeleteAsync(id);
}