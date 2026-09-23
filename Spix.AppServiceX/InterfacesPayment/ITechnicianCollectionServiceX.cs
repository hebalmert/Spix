using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesPayment;

public interface ITechnicianCollectionServiceX
{
    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboCollectorsAsync(string username);

    Task<ActionResponse<TechnicianCollectionSummaryDto>> GetSummaryAsync(Guid userId, PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<TechnicianCollectionDto>>> GetCollectionsAsync(Guid userId, PaginationDTO pagination, string username);
}
