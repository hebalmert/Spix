using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//Los contratos que estan en un servidor, con su plan y lo que genera ese servidor
public partial class ReportByServer
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports";
    private const int PageSize = 20;

    private List<GuidItemModel>? Servers;
    private List<ReportActiveContractDto>? Contracts = new();
    private ReportActiveSummaryDto? Summary;

    private Guid ServerId;
    private int CurrentPage = 1;
    private int TotalPages;

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>($"{BaseUrl}/comboservers");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Servers = responseHttp.Response ?? new();
    }

    private async Task ServerChanged(ChangeEventArgs e)
    {
        ServerId = Guid.TryParse(e.Value?.ToString(), out var id) ? id : Guid.Empty;

        if (ServerId == Guid.Empty)
        {
            Summary = null;
            Contracts = new();
            TotalPages = 0;
            return;
        }

        CurrentPage = 1;
        await LoadSummaryAsync();
        await LoadContractsAsync();
    }

    private async Task LoadSummaryAsync()
    {
        var responseHttp = await _repository.GetAsync<ReportActiveSummaryDto>($"{BaseUrl}/by-server/{ServerId}/summary");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Summary = responseHttp.Response;
    }

    private async Task LoadContractsAsync(int page = 1)
    {
        var responseHttp = await _repository.GetAsync<List<ReportActiveContractDto>>(
            $"{BaseUrl}/by-server/{ServerId}?page={page}&recordsnumber={PageSize}");

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Contracts = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadContractsAsync(page);
    }
}
