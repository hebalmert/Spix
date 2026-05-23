using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesOper;

public interface IClientService
{
    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<Client>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Client>> GetAsync(Guid id);

    Task<ActionResponse<Client>> UpdateAsync(Client modelo, string frontUrl);

    Task<ActionResponse<Client>> AddAsync(Client modelo, string username, string frontUrl);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}