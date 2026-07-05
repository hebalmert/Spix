using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesBilling;

public interface IBillingServiceX
{
    Task<ActionResponse<IEnumerable<BillingNote>>> GetBillingNotesAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username);

    Task<ActionResponse<BillingNote>> GetBillingNoteAsync(Guid id, string username);

    Task<ActionResponse<BillingNote>> AddBillingNoteAsync(BillingNote model, string username);

    Task<ActionResponse<BillingNote>> UpdateBillingNoteAsync(BillingNote model, string username);

    Task<ActionResponse<bool>> DeleteBillingNoteAsync(Guid id, string username);

    Task<ActionResponse<IEnumerable<BillingNoteOne>>> GetBillingNoteOnesAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<BillingNoteOne>> GetBillingNoteOneAsync(Guid id, string username);

    Task<ActionResponse<BillingNoteOne>> AddBillingNoteOneAsync(BillingNoteOne model, string username);

    Task<ActionResponse<BillingNoteOne>> UpdateBillingNoteOneAsync(BillingNoteOne model, string username);

    Task<ActionResponse<bool>> DeleteBillingNoteOneAsync(Guid id, string username);

    Task<ActionResponse<IEnumerable<BillingContractDto>>> SearchContractsAsync(string filter, string username);

    Task<ActionResponse<IEnumerable<Sell>>> GetSellsAsync(PaginationDTO pagination, string username);
}
