using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//Los contratos que hoy no estan dando servicio y lo que se dejo de facturar por ellos
public partial class ReportChurn
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports-operation";

    private ReportChurnDto? Summary;
    private List<ReportChurnZoneDto>? Zones;

    //Lo que se dejo de facturar por los que ya no estan: los suspendidos no cuentan, todavia
    //pueden volver
    private decimal Lost => Summary is null ? 0 : Summary.TerminatedAmount + Summary.CancelledAmount;

    protected override async Task OnInitializedAsync()
    {
        var resumen = await _repository.GetAsync<ReportChurnDto>($"{BaseUrl}/churn/summary");
        if (!await _responseHandler.HandleErrorAsync(resumen))
            Summary = resumen.Response;

        var zonas = await _repository.GetAsync<List<ReportChurnZoneDto>>($"{BaseUrl}/churn/zones");
        if (!await _responseHandler.HandleErrorAsync(zonas))
            Zones = zonas.Response ?? new();

        await InvokeAsync(StateHasChanged);
    }
}
