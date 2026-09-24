using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//Como le fue al corte del periodo: cuantos se cortaron y cuantos pagaron por eso
public partial class ReportCutOff
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports-operation";

    private ReportCutOffSummaryDto? Summary;
    private List<ReportCutOffZoneDto>? Zones;

    //Arranca con el mes en curso
    private string DateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString("yyyy-MM-dd");
    private string DateEnd = DateTime.Today.ToString("yyyy-MM-dd");
    private bool IsLoading;

    //De cada cien cortados, cuantos pagaron
    private decimal Rate => Summary is null || Summary.Cut == 0
        ? 0
        : Math.Round(Summary.Recovered * 100m / Summary.Cut, 0);

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

        var resumen = await _repository.GetAsync<ReportCutOffSummaryDto>($"{BaseUrl}/cutoff/summary?{periodo}");
        if (!await _responseHandler.HandleErrorAsync(resumen))
            Summary = resumen.Response;

        var zonas = await _repository.GetAsync<List<ReportCutOffZoneDto>>($"{BaseUrl}/cutoff/zones?{periodo}");
        if (!await _responseHandler.HandleErrorAsync(zonas))
            Zones = zonas.Response ?? new();

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }
}
