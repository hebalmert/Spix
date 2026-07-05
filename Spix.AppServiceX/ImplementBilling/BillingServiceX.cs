using Spix.AppService.InterfacesBilling;
using Spix.AppServiceX.InterfacesBilling;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementBilling;

public class BillingServiceX : IBillingServiceX
{
    private readonly IBillingService _billingService;

    public BillingServiceX(IBillingService billingService)
    {
        _billingService = billingService;
    }

    public async Task<ActionResponse<IEnumerable<BillingNote>>> GetBillingNotesAsync(PaginationDTO pagination, string username) =>
        await _billingService.GetBillingNotesAsync(pagination, username);

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username) =>
        await _billingService.ComboMonthsAsync(username);

    public async Task<ActionResponse<BillingNote>> GetBillingNoteAsync(Guid id, string username) =>
        await _billingService.GetBillingNoteAsync(id, username);

    public async Task<ActionResponse<BillingNote>> AddBillingNoteAsync(BillingNote model, string username) =>
        await _billingService.AddBillingNoteAsync(model, username);

    public async Task<ActionResponse<BillingNote>> UpdateBillingNoteAsync(BillingNote model, string username) =>
        await _billingService.UpdateBillingNoteAsync(model, username);

    public async Task<ActionResponse<bool>> DeleteBillingNoteAsync(Guid id, string username) =>
        await _billingService.DeleteBillingNoteAsync(id, username);

    public async Task<ActionResponse<BillingNote>> LaunchBillingNoteAsync(Guid id, string username) =>
        await _billingService.LaunchBillingNoteAsync(id, username);

    public async Task<ActionResponse<IEnumerable<BillingNoteOne>>> GetBillingNoteOnesAsync(PaginationDTO pagination, string username) =>
        await _billingService.GetBillingNoteOnesAsync(pagination, username);

    public async Task<ActionResponse<BillingNoteOne>> GetBillingNoteOneAsync(Guid id, string username) =>
        await _billingService.GetBillingNoteOneAsync(id, username);

    public async Task<ActionResponse<BillingNoteOne>> AddBillingNoteOneAsync(BillingNoteOne model, string username) =>
        await _billingService.AddBillingNoteOneAsync(model, username);

    public async Task<ActionResponse<BillingNoteOne>> UpdateBillingNoteOneAsync(BillingNoteOne model, string username) =>
        await _billingService.UpdateBillingNoteOneAsync(model, username);

    public async Task<ActionResponse<bool>> DeleteBillingNoteOneAsync(Guid id, string username) =>
        await _billingService.DeleteBillingNoteOneAsync(id, username);

    public async Task<ActionResponse<BillingNoteOne>> LaunchBillingNoteOneAsync(Guid id, string username) =>
        await _billingService.LaunchBillingNoteOneAsync(id, username);

    public async Task<ActionResponse<IEnumerable<BillingContractDto>>> SearchContractsAsync(string filter, string username) =>
        await _billingService.SearchContractsAsync(filter, username);

    public async Task<ActionResponse<IEnumerable<Sell>>> GetSellsAsync(PaginationDTO pagination, string username) =>
        await _billingService.GetSellsAsync(pagination, username);
}
