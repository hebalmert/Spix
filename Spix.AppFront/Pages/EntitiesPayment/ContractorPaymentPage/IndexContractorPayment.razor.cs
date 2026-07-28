using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesPayment.ContractorPaymentPage;

public partial class IndexContractorPayment
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    private const string BaseUrl = "/api/v1/contractor-payments";
    private const int PageSize = 10;
    private List<ContractorAccountPayable>? AccountPayables;
    private HashSet<Guid> SelectedAccountPayableIds { get; } = new();
    private Guid? SelectedContractorId;
    private string? SelectedContractorName;
    private string Filter = string.Empty;
    private int CurrentPage = 1;
    private int TotalPages = 1;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task FilterChanged(ChangeEventArgs args)
    {
        Filter = args.Value?.ToString() ?? string.Empty;
        CurrentPage = 1;
        ClearSelection();
        await LoadAsync();
    }

    private async Task ClearFilterAsync()
    {
        Filter = string.Empty;
        CurrentPage = 1;
        ClearSelection();
        await LoadAsync();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        ClearSelection();
        await LoadAsync(page);
    }

    private async Task LoadAsync(int page = 1)
    {
        var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Uri.EscapeDataString(Filter)}";
        }

        var responseHttp = await _repository.GetAsync<List<ContractorAccountPayable>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        AccountPayables = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    private async Task ToggleAccountPayableAsync(ContractorAccountPayable accountPayable, ChangeEventArgs args)
    {
        var isSelected = args.Value is bool value && value;
        if (isSelected)
        {
            if (SelectedContractorId.HasValue && SelectedContractorId.Value != accountPayable.ContractorId)
            {
                await _sweetAlert.FireAsync("Validación", "Solo puede liquidar cuentas de un contratista a la vez.", SweetAlertIcon.Warning);
                return;
            }

            SelectedContractorId = accountPayable.ContractorId;
            SelectedContractorName = $"{accountPayable.Contractor?.FirstName} {accountPayable.Contractor?.LastName}";
            SelectedAccountPayableIds.Add(accountPayable.ContractorAccountPayableId);
            return;
        }

        SelectedAccountPayableIds.Remove(accountPayable.ContractorAccountPayableId);
        if (SelectedAccountPayableIds.Count == 0)
        {
            SelectedContractorId = null;
            SelectedContractorName = null;
        }
    }

    private async Task ShowPayAsync()
    {
        if (!SelectedContractorId.HasValue || SelectedAccountPayableIds.Count == 0)
        {
            return;
        }

        var selectedAccountPayables = AccountPayables!
            .Where(x => SelectedAccountPayableIds.Contains(x.ContractorAccountPayableId))
            .ToList();

        var parameters = new Dictionary<string, object>
        {
            { "ContractorId", SelectedContractorId.Value },
            { "ContractorName", SelectedContractorName ?? string.Empty },
            { "ContractorAccountPayableIds", SelectedAccountPayableIds.ToList() },
            { "Total", selectedAccountPayables.Sum(x => x.Balance) },
            { "Title", "Registrar pago al contratista" }
        };

        await _modalService.ShowAsync(typeof(PayContractorPayment), parameters, async result =>
        {
            if (result.Succeeded)
            {
                ClearSelection();
                await LoadAsync(CurrentPage);
            }
        });
    }

    private void ClearSelection()
    {
        SelectedAccountPayableIds.Clear();
        SelectedContractorId = null;
        SelectedContractorName = null;
    }
}
