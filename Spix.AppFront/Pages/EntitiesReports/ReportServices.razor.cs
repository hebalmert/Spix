using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//Lo que paso con las solicitudes de servicio del periodo: como entraron, como terminaron,
//cuales se repiten y quien las resolvio
public partial class ReportServices
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports-operation";

    private ReportServicesSummaryDto? Summary;
    private List<ReportServiceDto>? Services;
    private List<ReportTechnicianDto>? Technicians;

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

        var resumen = await _repository.GetAsync<ReportServicesSummaryDto>($"{BaseUrl}/services/summary?{periodo}");
        if (!await _responseHandler.HandleErrorAsync(resumen))
            Summary = resumen.Response;

        var servicios = await _repository.GetAsync<List<ReportServiceDto>>($"{BaseUrl}/services/top?{periodo}");
        if (!await _responseHandler.HandleErrorAsync(servicios))
            Services = servicios.Response ?? new();

        var tecnicos = await _repository.GetAsync<List<ReportTechnicianDto>>($"{BaseUrl}/services/technicians?{periodo}");
        if (!await _responseHandler.HandleErrorAsync(tecnicos))
            Technicians = tecnicos.Response ?? new();

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }
}
