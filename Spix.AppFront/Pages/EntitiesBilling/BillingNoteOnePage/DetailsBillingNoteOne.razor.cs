using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesBilling.BillingNoteOnePage;

//La nota de cobro de un solo cliente: primero se ve que se le va a cobrar y que le falta
//al contrato, y solo despues se lanza.
public partial class DetailsBillingNoteOne
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/billingnoteones";

    private BillingNoteOne? Model;
    private BillingOneCheckDto? Check;
    private BillingContractDto? SelectedContract;
    private List<BillingContractDto> Contracts = new();
    private List<IntItemModel>? Months;
    private bool isLoading;
    private bool IsLaunching;

    //Todo lo que impide lanzar la nota. Si esta vacio, el boton se habilita.
    private List<string> Blocks => BuildBlocks();

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        await LoadMonthsAsync();
        await LoadModelAsync();

        if (Model is not null && !Model.Created)
        {
            await LoadCheckAsync();
        }

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

    private async Task LoadCheckAsync()
    {
        var responseHttp = await _repository.GetAsync<BillingOneCheckDto>($"{BaseUrl}/{Id}/check");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Check = responseHttp.Response;
    }

    //Lo que le falta al contrato y lo que ya no deja lanzar
    private List<string> BuildBlocks()
    {
        var blocks = new List<string>();
        if (Check is null)
            return blocks;

        if (!Check.IsActive) blocks.Add(Localizer["BillingOne_NotActive"]);
        if (Check.AlreadyBilled) blocks.Add(Localizer["BillingOne_AlreadyBilled", Check.ControlContrato]);
        if (Check.PrePaymentAndExonerated) blocks.Add(Localizer["BillingOne_Both"]);

        if (!Check.HasPlan) blocks.Add("Plan");
        if (!Check.HasIp) blocks.Add("IP");
        if (!Check.HasMac) blocks.Add("MAC");
        if (!Check.HasServer) blocks.Add("Servidor");
        if (!Check.HasNode) blocks.Add("Nodo");
        if (!Check.HasQueue) blocks.Add("Queue");
        if (!Check.HasBinding) blocks.Add("IpBinding");

        return blocks;
    }

    private Task SearchContracts(string filter) => Task.CompletedTask;

    private async Task LaunchNotes()
    {
        if (Check is null || IsLaunching)
            return;

        var confirm = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["BillingOne_Title"],
            Text = Localizer["BillingOne_Confirm", Check.ClientFullName, Check.Balance.ToString("N2")].Value,
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer["BillingOne_Title"],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirm.IsDismissed || confirm.Value != "true")
            return;

        IsLaunching = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/{Id}/launch", new { });
        IsLaunching = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync(Localizer["BillingOne_Title"], Localizer["BillingOne_Launched"], SweetAlertIcon.Success);
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
