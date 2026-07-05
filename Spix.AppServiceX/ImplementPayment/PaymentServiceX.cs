using Spix.AppService.InterfacesPayment;
using Spix.AppServiceX.InterfacesPayment;
using Spix.Domain.EntitiesPayment;
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
}
