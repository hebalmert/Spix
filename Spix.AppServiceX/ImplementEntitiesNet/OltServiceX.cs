using Spix.AppService.InterfaceEntitiesNet;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEntitiesNet;

public class OltServiceX : IOltServiceX
{
    private readonly IOltService _oltService;

    public OltServiceX(IOltService oltService)
    {
        _oltService = oltService;
    }

    public async Task<ActionResponse<IEnumerable<Olt>>> ComboAsync(string username, Guid? id = null) => await _oltService.ComboAsync(username, id);

    public async Task<ActionResponse<NetSummaryDto>> GetSummaryAsync(string username) => await _oltService.GetSummaryAsync(username);

    public async Task<ActionResponse<IEnumerable<OltListItemDto>>> GetAsync(PaginationDTO pagination, string username) => await _oltService.GetAsync(pagination, username);

    public async Task<ActionResponse<Olt>> GetAsync(Guid id, string username, bool withCredentials) => await _oltService.GetAsync(id, username, withCredentials);

    public async Task<ActionResponse<Olt>> UpdateAsync(Olt modelo, string username) => await _oltService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<Olt>> AddAsync(Olt modelo, string username) => await _oltService.AddAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username) => await _oltService.DeleteAsync(id, username);
}
