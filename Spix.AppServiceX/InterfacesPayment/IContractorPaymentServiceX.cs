using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.InterfacesPayment;

public interface IContractorPaymentServiceX
{
    Task<ActionResponse<IEnumerable<ContractorAccountPayable>>> GetAccountPayablesAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<ContractorPayment>> PayAsync(ContractorPaymentCreateDto model, string username);
}
