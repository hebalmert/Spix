using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesOper;

public interface IClientService
{
    Task<ActionResponse<IEnumerable<Client>>> ComboAsync(string email);

    Task<ActionResponse<IEnumerable<Client>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<Client>> GetAsync(Guid id);

    Task<ActionResponse<Client>> UpdateAsync(Client modelo, string frontUrl);

    Task<ActionResponse<Client>> AddAsync(Client modelo, string email, string frontUrl);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}