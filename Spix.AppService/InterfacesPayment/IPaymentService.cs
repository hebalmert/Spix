using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesPayment;

public interface IPaymentService
{
    Task<ActionResponse<IEnumerable<CxCBill>>> GetCxCBillsAsync(PaginationDTO pagination, string username);
}
