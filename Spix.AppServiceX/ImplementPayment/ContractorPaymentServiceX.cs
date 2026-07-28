using Spix.AppService.InterfacesPayment;
using Spix.AppServiceX.InterfacesPayment;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementPayment;

public class ContractorPaymentServiceX : IContractorPaymentServiceX
{
    private readonly IContractorPaymentService _contractorPaymentService;

    public ContractorPaymentServiceX(IContractorPaymentService contractorPaymentService)
    {
        _contractorPaymentService = contractorPaymentService;
    }

    public async Task<ActionResponse<IEnumerable<ContractorAccountPayable>>> GetAccountPayablesAsync(PaginationDTO pagination, string username)
    {
        return await _contractorPaymentService.GetAccountPayablesAsync(pagination, username);
    }

    public async Task<ActionResponse<ContractorPayment>> PayAsync(ContractorPaymentCreateDto model, string username)
    {
        return await _contractorPaymentService.PayAsync(model, username);
    }
}
