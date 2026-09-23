using Spix.AppService.InterfacesPayment;
using Spix.AppServiceX.InterfacesPayment;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementPayment;

public class TechnicianCollectionServiceX : ITechnicianCollectionServiceX
{
    private readonly ITechnicianCollectionService _technicianCollectionService;

    public TechnicianCollectionServiceX(ITechnicianCollectionService technicianCollectionService)
    {
        _technicianCollectionService = technicianCollectionService;
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboCollectorsAsync(string username)
    {
        return await _technicianCollectionService.ComboCollectorsAsync(username);
    }

    public async Task<ActionResponse<TechnicianCollectionSummaryDto>> GetSummaryAsync(Guid userId, PaginationDTO pagination, string username)
    {
        return await _technicianCollectionService.GetSummaryAsync(userId, pagination, username);
    }

    public async Task<ActionResponse<IEnumerable<TechnicianCollectionDto>>> GetCollectionsAsync(Guid userId, PaginationDTO pagination, string username)
    {
        return await _technicianCollectionService.GetCollectionsAsync(userId, pagination, username);
    }
}
