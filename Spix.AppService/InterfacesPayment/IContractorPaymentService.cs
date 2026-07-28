using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesPayment;

public interface IContractorPaymentService
{
    Task CreateAccountPayableAsync(CxCBill cxCBill, CxCBillDetail cxCBillDetail);

    Task<ActionResponse<IEnumerable<ContractorAccountPayable>>> GetAccountPayablesAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ContractorPayment>> PayAsync(ContractorPaymentCreateDto model, string username);
}
