using Spix.AppService.InterfacesPayment;
using Spix.AppServiceX.InterfacesPayment;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementPayment;

public class PaymentServiceX : IPaymentServiceX
{
    private readonly IPaymentService _paymentService;

    public PaymentServiceX(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<ActionResponse<IEnumerable<CxCBill>>> GetCxCBillsAsync(PaginationDTO pagination, string username) =>
        await _paymentService.GetCxCBillsAsync(pagination, username);

    public async Task<ActionResponse<IEnumerable<PrePayment>>> GetPrePaymentsAsync(PaginationDTO pagination, string username) =>
        await _paymentService.GetPrePaymentsAsync(pagination, username);

    public async Task<ActionResponse<PrePayment>> GetPrePaymentAsync(Guid id, string username) =>
        await _paymentService.GetPrePaymentAsync(id, username);

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username) =>
        await _paymentService.ComboMonthsAsync(username);

    public async Task<ActionResponse<IEnumerable<BillingContractDto>>> SearchContractsAsync(string filter, string username) =>
        await _paymentService.SearchContractsAsync(filter, username);

    public async Task<ActionResponse<PrePayment>> AddPrePaymentAsync(PrePayment model, string username) =>
        await _paymentService.AddPrePaymentAsync(model, username);

    public async Task<ActionResponse<PrePayment>> UpdatePrePaymentAsync(PrePayment model, string username) =>
        await _paymentService.UpdatePrePaymentAsync(model, username);

    public async Task<ActionResponse<bool>> DeletePrePaymentAsync(Guid id, string username) =>
        await _paymentService.DeletePrePaymentAsync(id, username);

    public async Task<ActionResponse<IEnumerable<PreExonerated>>> GetPreExoneratedsAsync(PaginationDTO pagination, string username) =>
        await _paymentService.GetPreExoneratedsAsync(pagination, username);

    public async Task<ActionResponse<PreExonerated>> GetPreExoneratedAsync(Guid id, string username) =>
        await _paymentService.GetPreExoneratedAsync(id, username);

    public async Task<ActionResponse<PreExonerated>> AddPreExoneratedAsync(PreExonerated model, string username) =>
        await _paymentService.AddPreExoneratedAsync(model, username);

    public async Task<ActionResponse<PreExonerated>> UpdatePreExoneratedAsync(PreExonerated model, string username) =>
        await _paymentService.UpdatePreExoneratedAsync(model, username);

    public async Task<ActionResponse<bool>> DeletePreExoneratedAsync(Guid id, string username) =>
        await _paymentService.DeletePreExoneratedAsync(id, username);
}
