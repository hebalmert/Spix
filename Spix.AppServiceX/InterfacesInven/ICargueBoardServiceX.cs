using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesInven;

public interface ICargueBoardServiceX
{
    Task<ActionResponse<IEnumerable<CargueListItemDto>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<CargueSummaryDto>> GetSummaryAsync(string username);

    Task<ActionResponse<CargueProgressDto>> GetProgressAsync(Guid id, string username);

    Task<ActionResponse<IEnumerable<CargueSerialDto>>> GetSerialsAsync(Guid id, PaginationDTO pagination, string username);
}
