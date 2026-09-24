using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//Los contratos que entraron en el periodo y lo que agregaron a la facturacion mensual
public partial class ReportContracts
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports-operation";

    private ReportContractsSummaryDto? Summary;
    private List<ReportPlanDto>? Plans;

    //Arranca con el mes en curso
    private string DateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString("yyyy-MM-dd");
    private string DateEnd = DateTime.Today.ToString("yyyy-MM-dd");
    private bool IsLoading;

    protected override async Task OnInitializedAsync()
    {
        await SearchAsync();
    }

    private void DateStartChanged(ChangeEventArgs e)
    {
        DateStart = e.Value?.ToString() ?? string.Empty;
    }

    private void DateEndChanged(ChangeEventArgs e)
    {
        DateEnd = e.Value?.ToString() ?? string.Empty;
    }

    private async Task SearchAsync()
    {
        IsLoading = true;

        var periodo = $"datestart={Uri.EscapeDataString(DateStart)}&dateend={Uri.EscapeDataString(DateEnd)}";

        var resumen = await _repository.GetAsync<ReportContractsSummaryDto>($"{BaseUrl}/contracts/summary?{periodo}");
        if (!await _responseHandler.HandleErrorAsync(resumen))
            Summary = resumen.Response;

        var planes = await _repository.GetAsync<List<ReportPlanDto>>($"{BaseUrl}/contracts/top-plans?{periodo}");
        if (!await _responseHandler.HandleErrorAsync(planes))
            Plans = planes.Response ?? new();

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }
}
