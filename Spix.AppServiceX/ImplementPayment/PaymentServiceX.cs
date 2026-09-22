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

    public async Task<ActionResponse<CxCBill>> GetCxCBillAsync(Guid id, string username) =>
        await _paymentService.GetCxCBillAsync(id, username);

    public async Task<ActionResponse<CxCBill>> PayCxCBillAsync(CxCBillPaymentDto model, string username) =>
        await _paymentService.PayCxCBillAsync(model, username);

    public async Task<ActionResponse<CxCBill>> CancelCxCBillAsync(CxCBillCancelDto model, string username) =>
        await _paymentService.CancelCxCBillAsync(model, username);

    public async Task<ActionResponse<IEnumerable<PrePayment>>> GetPrePaymentsAsync(PaginationDTO pagination, string username) =>
        await _paymentService.GetPrePaymentsAsync(pagination, username);

    public async Task<ActionResponse<PrePayment>> GetPrePaymentAsync(Guid id, string username) =>
        await _paymentService.GetPrePaymentAsync(id, username);

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username) =>
        await _paymentService.ComboMonthsAsync(username);

    public async Task<ActionResponse<IEnumerable<BillingContractDto>>> SearchContractsAsync(string filter, string username) =>
        await _paymentService.SearchContractsAsync(filter, username);

    public async Task<ActionResponse<IEnumerable<PrePaymentServiceDto>>> GetPrePaymentServicesAsync(Guid contractClientId, Guid? prePaymentId, string username) => await _paymentService.GetPrePaymentServicesAsync(contractClientId, prePaymentId, username);

    public async Task<ActionResponse<PrePayment>> AddPrePaymentAsync(PrePayment model, string username) =>
        await _paymentService.AddPrePaymentAsync(model, username);

    public async Task<ActionResponse<PrePayment>> UpdatePrePaymentAsync(PrePayment model, string username) =>
        await _paymentService.UpdatePrePaymentAsync(model, username);

    public async Task<ActionResponse<bool>> DeletePrePaymentAsync(Guid id, string username) =>
        await _paymentService.DeletePrePaymentAsync(id, username);

    public async Task<ActionResponse<IEnumerable<ContractExonerated>>> GetContractExoneratedsAsync(PaginationDTO pagination, string username) =>
        await _paymentService.GetContractExoneratedsAsync(pagination, username);

    public async Task<ActionResponse<ContractExonerated>> GetContractExoneratedAsync(Guid id, string username) =>
        await _paymentService.GetContractExoneratedAsync(id, username);

    public async Task<ActionResponse<ContractExonerated>> AddContractExoneratedAsync(ContractExonerated model, string username) =>
        await _paymentService.AddContractExoneratedAsync(model, username);

    public async Task<ActionResponse<ContractExonerated>> UpdateContractExoneratedAsync(ContractExonerated model, string username) =>
        await _paymentService.UpdateContractExoneratedAsync(model, username);

    public async Task<ActionResponse<bool>> DeleteContractExoneratedAsync(Guid id, string username) =>
        await _paymentService.DeleteContractExoneratedAsync(id, username);
}
