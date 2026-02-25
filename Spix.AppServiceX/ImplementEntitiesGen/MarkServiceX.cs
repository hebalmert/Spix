using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesGen;

public class MarkUnitOfWork : IMarkServiceX
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