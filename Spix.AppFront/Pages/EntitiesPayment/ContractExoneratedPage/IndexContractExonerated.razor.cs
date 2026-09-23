using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesPayment.ContractExoneratedPage;

public partial class IndexContractExonerated
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private const string BaseUrl = "api/v1/contractexonerateds";
    private string Filter { get; set; } = string.Empty;
    private List<ContractExonerated>? ContractExonerateds { get; set; }
    private List<IntItemModel> Months { get; set; } = new();

    //Los numeros del tablero: se piden una sola vez al abrir
    private ExoneratedSummaryDto? Summary { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadMonthsAsync();
            await LoadSummaryAsync();
            await LoadAsync();
        }
    }

    private async Task LoadMonthsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/combomonths");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Months = responseHttp.Response ?? new();
    }

    private async Task LoadSummaryAsync()
    {
        var responseHttp = await _repository.GetAsync<ExoneratedSummaryDto>($"{BaseUrl}/summary");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Summary = responseHttp.Response;
    }

    private string GetMonthName(MonthType monthType) =>
        Months.FirstOrDefault(x => x.Value == (int)monthType)?.Name ?? monthType.ToString();

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await LoadAsync();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadAsync(page);
    }

    private async Task LoadAsync(int page = 1)
    {
        var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
            url += $"&filter={Uri.EscapeDataString(Filter)}";

        var responseHttp = await _repository.GetAsync<List<ContractExonerated>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        ContractExonerateds = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowModalAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Title", "Nueva Exoneracion" }
        };

        await _modalService.ShowAsync(typeof(CreateContractExonerated), parameters, async result =>
        {
            if (result.Succeeded)
                await LoadSummaryAsync();
                await LoadAsync(CurrentPage);
        });
    }

    private async Task ShowEditAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", "Editar Exoneracion" }
        };

        await _modalService.ShowAsync(typeof(EditContractExonerated), parameters, async result =>
        {
            if (result.Succeeded)
                await LoadSummaryAsync();
                await LoadAsync(CurrentPage);
        });
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Eliminar",
            Text = "Desea eliminar esta exoneracion?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Eliminar",
            CancelButtonText = "Cancelar"
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{BaseUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Eliminado", "Registro eliminado correctamente.", SweetAlertIcon.Success);
        await LoadSummaryAsync();
        await LoadAsync(CurrentPage);
    }

    //Rastro de la exoneracion: quien la registro, cuando, que mes cubre y, si ya se
    //cerro, quien la cerro. La misma ventana que usan Suspendidos y Exonerados Fijos.
    private async Task ShowAuditAsync(ContractExonerated item)
    {
        await AuditAlert.ShowAsync(_sweetAlert, Localizer["Audit_Title"],
            (Localizer["Audit_RegisteredBy"], item.UserByName),
            (Localizer["Audit_Date"], item.DateExonerated.ToString("dd/MM/yyyy")),
            (Localizer["Audit_Period"], $"{GetMonthName(item.MonthType)} {item.YearNumber}"),
            (Localizer["Audit_Reason"], item.Motivo),
            (Localizer["Audit_Billed"], item.DateBilled?.ToString("dd/MM/yyyy")),
            (Localizer["Audit_ReactivatedBy"], item.UserByNameEnded),
            (Localizer["Audit_ReactivatedDate"], item.DateEnded?.ToLocalTime().ToString("dd/MM/yyyy HH:mm")));
    }
}
