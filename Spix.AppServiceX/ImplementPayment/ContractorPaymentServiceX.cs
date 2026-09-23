using Spix.AppService.InterfacesPayment;
using Spix.AppServiceX.InterfacesPayment;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementPayment;

public class ContractorPaymentServiceX : IContractorPaymentServiceX
{
    private readonly IContractorPaymentService _contractorPaymentService;

    public ContractorPaymentServiceX(IContractorPaymentService contractorPaymentService)
    {
        _contractorPaymentService = contractorPaymentService;
    }

    public async Task<ActionResponse<CxCContractorSummaryDto>> GetCxCSummaryAsync(string username) =>
        await _contractorPaymentService.GetCxCSummaryAsync(username);

    public async Task<ActionResponse<IEnumerable<ContractorPendingDto>>> GetPendingAsync(Guid contractorId, string username) =>
        await _contractorPaymentService.GetPendingAsync(contractorId, username);

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboContractorsAsync(string username) =>
        await _contractorPaymentService.ComboContractorsAsync(username);

    public async Task<ActionResponse<CxCContractor>> CreateCxCContractorAsync(CxCContractorCreateDto model, string username) =>
        await _contractorPaymentService.CreateCxCContractorAsync(model, username);

    public async Task<ActionResponse<IEnumerable<CxCContractor>>> GetCxCContractorsAsync(PaginationDTO pagination, string username) =>
        await _contractorPaymentService.GetCxCContractorsAsync(pagination, username);

    public async Task<ActionResponse<IEnumerable<ContractorPendingDto>>> GetCommissionsAsync(Guid id, PaginationDTO pagination, string username) =>
        await _contractorPaymentService.GetCommissionsAsync(id, pagination, username);

    public async Task<ActionResponse<IEnumerable<CxCContractorPaymentItemDto>>> GetPaymentsAsync(Guid id, PaginationDTO pagination, string username) =>
        await _contractorPaymentService.GetPaymentsAsync(id, pagination, username);

    public async Task<ActionResponse<CxCContractor>> GetCxCContractorAsync(Guid id, string username) =>
        await _contractorPaymentService.GetCxCContractorAsync(id, username);

    public async Task<ActionResponse<CxCContractor>> PayCxCContractorAsync(CxCContractorPaymentDto model, string username) =>
        await _contractorPaymentService.PayCxCContractorAsync(model, username);

    public async Task<ActionResponse<CxCContractor>> CancelCxCContractorAsync(Guid id, string motivo, string username) =>
        await _contractorPaymentService.CancelCxCContractorAsync(id, motivo, username);

    public async Task<ActionResponse<IEnumerable<ContractorAccountPayable>>> GetAccountPayablesAsync(PaginationDTO pagination, string username)
    {
        return await _contractorPaymentService.GetAccountPayablesAsync(pagination, username);
    }

    public async Task<ActionResponse<ContractorPayment>> PayAsync(ContractorPaymentCreateDto model, string username)
    {
        return await _contractorPaymentService.PayAsync(model, username);
    }
}
