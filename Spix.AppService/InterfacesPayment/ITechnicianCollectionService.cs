using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesPayment;

public interface ITechnicianCollectionService
{
    //Quienes han recibido pagos, para el combo
    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboCollectorsAsync(string username);

    //Lo que recogio una persona entre dos fechas
    Task<ActionResponse<TechnicianCollectionSummaryDto>> GetSummaryAsync(Guid userId, PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<TechnicianCollectionDto>>> GetCollectionsAsync(Guid userId, PaginationDTO pagination, string username);
}
