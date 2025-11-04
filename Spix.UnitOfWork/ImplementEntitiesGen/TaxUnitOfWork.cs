using Spix.Domain.EntitiesGen;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesGen;
using Spix.UnitOfWork.InterfacesEntitiesGen;

namespace Spix.UnitOfWork.ImplementEntitiesGen;

public class TaxUnitOfWork : ITaxUnitOfWork
{
    private readonly ITaxService _taxService;

    public TaxUnitOfWork(ITaxService taxService)
    {
        _taxService = taxService;
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username) => await _taxService.ComboAsync(username);

    public async Task<ActionResponse<IEnumerable<Tax>>> GetAsync(PaginationDTO pagination, string username) => await _taxService.GetAsync(pagination, username);

    public async Task<ActionResponse<Tax>> GetAsync(Guid id) => await _taxService.GetAsync(id);

    public async Task<ActionResponse<Tax>> UpdateAsync(Tax modelo) => await _taxService.UpdateAsync(modelo);

    public async Task<ActionResponse<Tax>> AddAsync(Tax modelo, string username) => await _taxService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _taxService.DeleteAsync(id);
}