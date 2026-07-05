using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesPayment.PreExoneratedPage;

public partial class CreatePreExonerated
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string? Title { get; set; }

    private PreExonerated Model = new();
    private BillingContractDto? SelectedContract;
    private List<BillingContractDto> Contracts = new();
    private List<IntItemModel>? Months;
    private bool isLoading;
    private bool IsSaving;
    private const string BaseUrl = "api/v1/preexonerateds";

    protected override async Task OnInitializedAsync()
    {
        SetDefaultDates();
        await LoadMonthsAsync();
    }

    private void SetDefaultDates()
    {
        var nextMonth = DateTime.UtcNow.AddMonths(1);
        Model.DateExonerated = DateTime.UtcNow;
        Model.YearNumber = nextMonth.Year;
        Model.MonthType = (MonthType)nextMonth.Month;
    }

    private async Task LoadMonthsAsync()
    {
        isLoading = true;
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        isLoading = false;

        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Months = responseHttp.Response ?? new();
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

    private async Task Create()
    {
        if (Model.ContractClientId == Guid.Empty)
        {
            await _sweetAlert.FireAsync("Validacion", "Debe seleccionar un contrato activo.", SweetAlertIcon.Warning);
            return;
        }

        IsSaving = true;
        var responseHttp = await _repository.PostAsync(BaseUrl, Model);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
