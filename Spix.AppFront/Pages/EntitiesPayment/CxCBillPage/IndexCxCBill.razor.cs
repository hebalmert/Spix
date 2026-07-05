using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using System.Globalization;

namespace Spix.AppFront.Pages.EntitiesPayment.CxCBillPage;

public partial class IndexCxCBill
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private const string BaseUrl = "api/v1/cxcbills";
    private string Filter { get; set; } = string.Empty;
    private string ContractFilter { get; set; } = string.Empty;
    private Guid? SelectedContractId { get; set; }
    private List<CxCBill>? CxCBills { get; set; }
    private List<BillingContractDto> Contracts { get; set; } = new();
    private string PaidText => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es" ? "Pagado" : "Paid";
    private string CancelledText => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es" ? "Anulado" : "Cancelled";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await LoadAsync();
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await LoadAsync();
    }

    private async Task FilterContractsChanged(ChangeEventArgs e)
    {
        ContractFilter = e.Value?.ToString() ?? string.Empty;
        SelectedContractId = null;

        if (ContractFilter.Length < 2)
        {
            Contracts.Clear();
            await LoadAsync();
            return;
        }

        var responseHttp = await _repository.GetAsync<List<BillingContractDto>>($"{BaseUrl}/searchcontracts?filter={Uri.EscapeDataString(ContractFilter)}");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Contracts = responseHttp.Response ?? new();
    }

    private async Task SelectContract(BillingContractDto contract)
    {
        SelectedContractId = contract.ContractClientId;
        ContractFilter = $"{contract.ClientFullName} - Contrato {contract.ControlContrato}";
        Contracts.Clear();
        CurrentPage = 1;
        await LoadAsync();
    }

    private async Task ClearContractAsync()
    {
        SelectedContractId = null;
        ContractFilter = string.Empty;
        Contracts.Clear();
        CurrentPage = 1;
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
        if (SelectedContractId.HasValue)
            url += $"&guidid={SelectedContractId.Value}";

        var responseHttp = await _repository.GetAsync<List<CxCBill>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        CxCBills = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowPayAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", "Registrar Pago" }
        };

        await _modalService.ShowAsync(typeof(PayCxCBill), parameters, async result =>
        {
            if (result.Succeeded)
                await LoadAsync(CurrentPage);
        });
    }

    private async Task CancelAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", "Anular Cuenta" }
        };

        await _modalService.ShowAsync(typeof(CancelCxCBill), parameters, async result =>
        {
            if (result.Succeeded)
                await LoadAsync(CurrentPage);
        });
    }
}
