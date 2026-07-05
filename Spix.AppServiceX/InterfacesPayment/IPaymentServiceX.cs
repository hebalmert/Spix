using Spix.Domain.EntitiesPayment;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesPayment;

public interface IPaymentServiceX
{
    Task<ActionResponse<IEnumerable<CxCBill>>> GetCxCBillsAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<CxCBill>> GetCxCBillAsync(Guid id, string username);

    Task<ActionResponse<CxCBill>> PayCxCBillAsync(CxCBillPaymentDto model, string username);

    Task<ActionResponse<CxCBill>> CancelCxCBillAsync(CxCBillCancelDto model, string username);

    Task<ActionResponse<IEnumerable<PrePayment>>> GetPrePaymentsAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<PrePayment>> GetPrePaymentAsync(Guid id, string username);

    Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username);

    Task<ActionResponse<IEnumerable<BillingContractDto>>> SearchContractsAsync(string filter, string username);

    Task<ActionResponse<PrePayment>> AddPrePaymentAsync(PrePayment model, string username);

    Task<ActionResponse<PrePayment>> UpdatePrePaymentAsync(PrePayment model, string username);

    Task<ActionResponse<bool>> DeletePrePaymentAsync(Guid id, string username);

    Task<ActionResponse<IEnumerable<PreExonerated>>> GetPreExoneratedsAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<PreExonerated>> GetPreExoneratedAsync(Guid id, string username);

    Task<ActionResponse<PreExonerated>> AddPreExoneratedAsync(PreExonerated model, string username);

    Task<ActionResponse<PreExonerated>> UpdatePreExoneratedAsync(PreExonerated model, string username);

    Task<ActionResponse<bool>> DeletePreExoneratedAsync(Guid id, string username);
}
