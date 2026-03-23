using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesOper;

public interface IContractorServiceX
{
    Task<ActionResponse<IEnumerable<Contractor>>> ComboAsync(string email);

    Task<ActionResponse<IEnumerable<Contractor>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<Contractor>> GetAsync(Guid id);

    Task<ActionResponse<Contractor>> UpdateAsync(Contractor modelo, string frontUrl);

    Task<ActionResponse<Contractor>> AddAsync(Contractor modelo, string email, string frontUrl);

    Task<ActionResponse<bool>> DeleteAsync(Guid id);
}