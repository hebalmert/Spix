using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesPayment.PreExoneratedPage;

public partial class EditPreExonerated
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private PreExonerated? Model;
    private BillingContractDto? SelectedContract;
    private List<BillingContractDto> Contracts = new();
    private List<IntItemModel>? Months;
    private bool isLoading;
    private bool IsSaving;
    private const string BaseUrl = "api/v1/preexonerateds";

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        await LoadMonthsAsync();
        await LoadModelAsync();
        isLoading = false;
    }

    private async Task LoadMonthsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Months = responseHttp.Response ?? new();
    }

    private async Task LoadModelAsync()
    {
        var responseHttp = await _repository.GetAsync<PreExonerated>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Model = responseHttp.Response;
        SelectedContract = BuildContractDto(Model);
    }

    private async Task SearchContracts(string filter)
    {
        if (filter.Length < 2)
        {
            Contracts.Clear();
            return;
        }

        var responseHttp = await _repository.GetAsync<List<BillingContractDto>>($"{BaseUrl}/searchcontracts?filter={Uri.EscapeDataString(filter)}");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Contracts = responseHttp.Response ?? new();
    }

    private void SelectContract(BillingContractDto contract)
    {
        SelectedContract = contract;
        Contracts.Clear();
    }

    private async Task Edit()
    {
        IsSaving = true;
        var responseHttp = await _repository.PutAsync(BaseUrl, Model);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }

    private static BillingContractDto? BuildContractDto(PreExonerated? model)
    {
        var contract = model?.ContractClient;
        if (contract is null)
            return null;

        var plan = contract.ContractPlans?.Select(x => x.Plan).FirstOrDefault() ?? model!.Plan;

        return new BillingContractDto
        {
            ContractClientId = contract.ContractClientId,
            ClientId = contract.ClientId,
            ControlContrato = contract.ControlContrato,
            ClientFullName = $"{model!.Client?.FirstName} {model.Client?.LastName}",
            PhoneNumber = contract.PhoneNumber,
            Address = contract.Address,
            CityName = contract.Zone?.City?.Name,
            ZoneName = contract.Zone?.ZoneName,
            PlanId = plan?.PlanId ?? model.PlanId,
            PlanName = plan?.PlanName ?? model.Plan?.PlanName,
            PlanPrice = model.UnitPrice,
            TaxRate = model.TaxRate,
            PlanPriceWithTax = model.PriceWithTax
        };
    }
}
