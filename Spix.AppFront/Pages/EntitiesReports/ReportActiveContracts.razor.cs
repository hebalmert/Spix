using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//Los contratos activos con su plan y su monto
public partial class ReportActiveContracts
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports";
    private const int PageSize = 20;

    private List<ReportActiveContractDto>? Contracts { get; set; }
    private ReportActiveSummaryDto? Summary { get; set; }
    private string Filter { get; set; } = string.Empty;
    private int CurrentPage = 1;
    private int TotalPages;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        await LoadSummaryAsync();
        await LoadAsync();
    }

    private async Task LoadSummaryAsync()
    {
        var responseHttp = await _repository.GetAsync<ReportActiveSummaryDto>($"{BaseUrl}/active/summary");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Summary = responseHttp.Response;
    }

    private async Task LoadAsync(int page = 1)
    {
        var url = $"{BaseUrl}/active?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
            url += $"&filter={Uri.EscapeDataString(Filter)}";

        var responseHttp = await _repository.GetAsync<List<ReportActiveContractDto>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        Contracts = responseHttp.Response;
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
}
