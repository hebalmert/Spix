using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesGen;

public class MarkModelServiceX : IMarkModelServiceX
{
    private readonly IMarkModelService _markModelService;

    public MarkModelServiceX(IMarkModelService markModelService)
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