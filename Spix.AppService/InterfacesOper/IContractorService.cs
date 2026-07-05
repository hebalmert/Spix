using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesOper;

public interface IContractorService
{
    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username);

    Task<ActionResponse<IEnumerable<Contractor>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<Contractor>> GetAsync(Guid id);

    Task<ActionResponse<Contractor>> UpdateAsync(Contractor modelo, string frontUrl);

    Task<ActionResponse<Contractor>> AddAsync(Contractor modelo, string username, string frontUrl);

    Task<ActionResponse<bool>> ResendActivationEmailAsync(Guid id, string frontUrl);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}
