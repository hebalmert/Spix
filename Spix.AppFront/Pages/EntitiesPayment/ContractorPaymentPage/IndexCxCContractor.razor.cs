using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesPayment.ContractorPaymentPage;

//Las cuentas por pagar a los contratistas: cada una agrupa sus comisiones y contra ella
//se le hacen los pagos, completos o por partes.
public partial class IndexCxCContractor
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    private const string BaseUrl = "api/v1/contractor-payments";
    private const int PageSize = 15;

    private int CurrentPage = 1;
    private int TotalPages;
    private string Filter { get; set; } = string.Empty;
    private List<CxCContractor>? Notes { get; set; }

    //Los numeros del tablero: se piden una sola vez al abrir
    private CxCContractorSummaryDto? Summary { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await LoadSummaryAsync();
        await LoadAsync();
    }

    private async Task LoadSummaryAsync()
    {
        var responseHttp = await _repository.GetAsync<CxCContractorSummaryDto>($"{BaseUrl}/summary");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Summary = responseHttp.Response;
    }

    private async Task LoadAsync(int page = 1)
    {
        var url = $"{BaseUrl}/cxc?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
            url += $"&filter={Uri.EscapeDataString(Filter)}";

        var responseHttp = await _repository.GetAsync<List<CxCContractor>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        Notes = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        CurrentPage = 1;
        await LoadAsync();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadAsync(page);
    }

    private async Task ShowCreateAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Title", Localizer["CxCContractor_New"].Value }
        };

        await _modalService.ShowAsync(typeof(CreateCxCContractor), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await ReloadAsync(Localizer["CxCContractor_Created"]);
            }
        });
    }

    private async Task ShowDetailsAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", Localizer["CxCContractor_CommissionsTitle"].Value }
        };

        await _modalService.ShowAsync(typeof(DetailsCxCContractor), parameters);
    }

    private async Task ShowPaymentsAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", Localizer["CxCContractor_PaymentsTitle"].Value }
        };

        await _modalService.ShowAsync(typeof(PaymentsCxCContractor), parameters);
    }

    private async Task ShowPayAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", Localizer["CxCContractor_Pay"].Value }
        };

        await _modalService.ShowAsync(typeof(PayCxCContractor), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await ReloadAsync(Localizer["CxCContractor_PaidOk"]);
            }
        });
    }

    private async Task ShowCancelAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", Localizer["CxCContractor_CancelTitle"].Value }
        };

        await _modalService.ShowAsync(typeof(CancelCxCContractor), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await ReloadAsync(Localizer[nameof(Resource.msg_DeleteConfirmationText)]);
            }
        });
    }

    //Al cerrar el modal la pantalla queda limpia y ahi si se avisa y se recarga
    private async Task ReloadAsync(string mensaje)
    {
        await LoadSummaryAsync();
        await LoadAsync(CurrentPage);
        await _sweetAlert.FireAsync(Localizer["CxCContractor_Title"], mensaje, SweetAlertIcon.Success);
    }
}
