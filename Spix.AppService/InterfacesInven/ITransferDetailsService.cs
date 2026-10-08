using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesInven;

public interface ITransferDetailsService
{
    Task<ActionResponse<IEnumerable<TransferDetails>>> GetAsync(PaginationDTO pagination, string email);

    Task<ActionResponse<TransferDetails>> GetAsync(Guid id, string username);

    Task<ActionResponse<TransferDetails>> UpdateAsync(TransferDetails modelo, string username);

    Task<ActionResponse<TransferDetails>> AddAsync(TransferDetails modelo, string email);

    Task<ActionResponse<IEnumerable<GuidItemModel>>> GetAvailableSerialsAsync(Guid transferId, Guid productId, Guid? transferDetailsId, string username);

    Task<ActionResponse<IEnumerable<GuidItemModel>>> GetMovedSerialsAsync(Guid transferDetailsId, string username);

    Task<ActionResponse<IEnumerable<GuidItemModel>>> GetLineSerialsAsync(Guid transferDetailsId, string username);

    Task<ActionResponse<bool>> SaveSerialsAsync(Guid transferDetailsId, List<Guid> serialIds, string username);

    Task<ActionResponse<Transfer>> CerrarTransAsync(Transfer modelo, string email);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);
}