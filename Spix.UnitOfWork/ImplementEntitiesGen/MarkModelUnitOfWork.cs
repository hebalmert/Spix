using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesGen;
using Spix.UnitOfWork.InterfacesEntitiesGen;

namespace Spix.UnitOfWork.ImplementEntitiesGen;

public class MarkModelUnitOfWork : IMarkModelUnitOfWork
{
    private readonly IMarkModelService _markModelService;

    public MarkModelUnitOfWork(IMarkModelService markModelService)
    {
        _markModelService = markModelService;
    }

    public async Task<ActionResponse<IEnumerable<MarkModel>>> ComboAsync(string username, Guid id) => await _markModelService.ComboAsync(username, id);

    public async Task<ActionResponse<IEnumerable<MarkModel>>> GetAsync(PaginationDTO pagination, string username) => await _markModelService.GetAsync(pagination, username);

    public async Task<ActionResponse<MarkModel>> GetAsync(Guid id) => await _markModelService.GetAsync(id);

    public async Task<ActionResponse<MarkModel>> UpdateAsync(MarkModel modelo) => await _markModelService.UpdateAsync(modelo);

    public async Task<ActionResponse<MarkModel>> AddAsync(MarkModel modelo, string username) => await _markModelService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id) => await _markModelService.DeleteAsync(id);
}