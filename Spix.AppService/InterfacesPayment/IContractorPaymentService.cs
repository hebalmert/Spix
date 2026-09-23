using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesPayment;

public interface IContractorPaymentService
{
    Task CreateAccountPayableAsync(CxCBill cxCBill, CxCBillDetail cxCBillDetail);

    //La cuenta por pagar del contratista: se arma, se consulta y se le abona
    Task<ActionResponse<CxCContractorSummaryDto>> GetCxCSummaryAsync(string username);

    Task<ActionResponse<IEnumerable<ContractorPendingDto>>> GetPendingAsync(Guid contractorId, string username);

    Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboContractorsAsync(string username);

    Task<ActionResponse<CxCContractor>> CreateCxCContractorAsync(CxCContractorCreateDto model, string username);

    Task<ActionResponse<IEnumerable<CxCContractor>>> GetCxCContractorsAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<ContractorPendingDto>>> GetCommissionsAsync(Guid id, PaginationDTO pagination, string username);

    Task<ActionResponse<IEnumerable<CxCContractorPaymentItemDto>>> GetPaymentsAsync(Guid id, PaginationDTO pagination, string username);

    Task<ActionResponse<CxCContractor>> GetCxCContractorAsync(Guid id, string username);

    Task<ActionResponse<CxCContractor>> PayCxCContractorAsync(CxCContractorPaymentDto model, string username);

    Task<ActionResponse<CxCContractor>> CancelCxCContractorAsync(Guid id, string motivo, string username);

    Task<ActionResponse<IEnumerable<ContractorAccountPayable>>> GetAccountPayablesAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ContractorPayment>> PayAsync(ContractorPaymentCreateDto model, string username);
}
