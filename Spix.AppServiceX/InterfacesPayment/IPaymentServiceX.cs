using Spix.Domain.EntitiesPayment;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesPayment;

public interface IPaymentServiceX
{
    Task<ActionResponse<CxCBillSummaryDto>> GetCxCBillSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<CxCBill>>> GetCxCBillsAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<CxCBill>> GetCxCBillAsync(Guid id, string username);

    Task<ActionResponse<CxCBill>> PayCxCBillAsync(CxCBillPaymentDto model, string username);

    Task<ActionResponse<CxCBill>> CancelCxCBillAsync(CxCBillCancelDto model, string username);

    Task<ActionResponse<PrePaymentSummaryDto>> GetPrePaymentSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<PrePayment>>> GetPrePaymentsAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<PrePayment>> GetPrePaymentAsync(Guid id, string username);

    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username);

    Task<ActionResponse<IEnumerable<BillingContractDto>>> SearchCxCContractsAsync(string filter, string username);

    Task<ActionResponse<IEnumerable<BillingContractDto>>> SearchContractsAsync(string filter, string username);

    Task<ActionResponse<IEnumerable<PrePaymentServiceDto>>> GetPrePaymentServicesAsync(Guid contractClientId, Guid? prePaymentId, string username);

    Task<ActionResponse<PrePayment>> AddPrePaymentAsync(PrePayment model, string username);

    Task<ActionResponse<PrePayment>> UpdatePrePaymentAsync(PrePayment model, string username);

    Task<ActionResponse<bool>> DeletePrePaymentAsync(Guid id, string username);

    Task<ActionResponse<ExoneratedSummaryDto>> GetExoneratedSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<ContractExonerated>>> GetContractExoneratedsAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ContractExonerated>> GetContractExoneratedAsync(Guid id, string username);

    Task<ActionResponse<ContractExonerated>> AddContractExoneratedAsync(ContractExonerated model, string username);

    Task<ActionResponse<ContractExonerated>> UpdateContractExoneratedAsync(ContractExonerated model, string username);

    Task<ActionResponse<bool>> DeleteContractExoneratedAsync(Guid id, string username);
}
