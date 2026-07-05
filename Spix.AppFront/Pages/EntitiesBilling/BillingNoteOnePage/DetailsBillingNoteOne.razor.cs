using Microsoft.AspNetCore.Components;
using CurrieTechnologies.Razor.SweetAlert2;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesBilling.BillingNoteOnePage;

public partial class DetailsBillingNoteOne
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private BillingNoteOne? Model;
    private BillingContractDto? SelectedContract;
    private List<BillingContractDto> Contracts = new();
    private List<IntItemModel>? Months;
    private bool isLoading;
    private const string BaseUrl = "api/v1/billingnoteones";

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
        var responseHttp = await _repository.GetAsync<BillingNoteOne>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Model = responseHttp.Response;
        SelectedContract = BuildContractDto(Model);
    }

    private Task SearchContracts(string filter) => Task.CompletedTask;

    private async Task LaunchNotes()
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Lanzar",
            Text = "Desea lanzar esta nota individual?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Lanzar",
            CancelButtonText = "Cancelar"
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.PostAsync($"{BaseUrl}/{Id}/launch", new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Lanzado", "Nota generada correctamente.", SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }

    private static BillingContractDto? BuildContractDto(BillingNoteOne? model)
    {
        var contract = model?.ContractClient;
        if (contract is null)
            return null;

        var plan = contract.ContractPlans?.Select(x => x.Plan).FirstOrDefault();

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
            PlanName = plan?.PlanName,
            PlanPrice = plan?.Price
        };
    }
}
