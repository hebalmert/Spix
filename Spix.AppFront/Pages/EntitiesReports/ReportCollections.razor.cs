using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//El recaudo de un periodo: cuanto entro, como entro, quien lo recogio y cuanto se emitio.
//Los tres bloques comparten el mismo rango de fechas, que es como se cierra el dia.
public partial class ReportCollections
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports-finance";

    private ReportCollectionSummaryDto? Summary;
    private List<ReportCollectorDto>? Collectors;
    private ReportNotesSummaryDto? Notes;

    private string DateStart = DateTime.Today.ToString("yyyy-MM-dd");
    private string DateEnd = DateTime.Today.ToString("yyyy-MM-dd");
    private bool IsLoading;

    //Arranca mostrando el dia de hoy, que es lo que mas se consulta
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

        var recaudo = await _repository.GetAsync<ReportCollectionSummaryDto>($"{BaseUrl}/collections/summary?{periodo}");
        if (!await _responseHandler.HandleErrorAsync(recaudo))
            Summary = recaudo.Response;

        var personas = await _repository.GetAsync<List<ReportCollectorDto>>($"{BaseUrl}/collections/collectors?{periodo}");
        if (!await _responseHandler.HandleErrorAsync(personas))
            Collectors = personas.Response ?? new();

        var notas = await _repository.GetAsync<ReportNotesSummaryDto>($"{BaseUrl}/notes/summary?{periodo}");
        if (!await _responseHandler.HandleErrorAsync(notas))
            Notes = notas.Response;

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }
}
