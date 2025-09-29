using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesGen;
using Spix.UnitOfWork.InterfacesEntitiesGen;

namespace Spix.UnitOfWork.ImplementEntitiesGen;

public class MarkUnitOfWork : IMarkUnitOfWork
{
    private readonly IMarkService _markService;

    public MarkUnitOfWork(IMarkService markService)
    {
        _markService = markService;
    }

    public async Task<ActionResponse<IEnumerable<Mark>>> ComboAsync(string username) => await _markService.ComboAsync(username);

    public async Task<ActionResponse<IEnumerable<Mark>>> GetAsync(PaginationDTO pagination, string username) => await _markService.GetAsync(pagination, username);

    public async Task<ActionResponse<Mark>> GetAsync(Guid id) => await _markService.GetAsync(id);

    public async Task<ActionResponse<Mark>> UpdateAsync(Mark modelo) => await _markService.UpdateAsync(modelo);

    public async Task<ActionResponse<Mark>> AddAsync(Mark modelo, string username) => await _markService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _markService.DeleteAsync(id);
}