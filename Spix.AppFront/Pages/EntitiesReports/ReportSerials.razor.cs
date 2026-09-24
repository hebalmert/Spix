using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//Cuantos equipos hay de cada producto y donde estan: bodega, cliente o averiados
public partial class ReportSerials
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports-inventory";

    private ReportSerialSummaryDto? Summary;
    private List<ReportSerialDto>? Serials;

    protected override async Task OnInitializedAsync()
    {
        var resumen = await _repository.GetAsync<ReportSerialSummaryDto>($"{BaseUrl}/serials/summary");
        if (!await _responseHandler.HandleErrorAsync(resumen))
            Summary = resumen.Response;

        var seriales = await _repository.GetAsync<List<ReportSerialDto>>($"{BaseUrl}/serials");
        if (!await _responseHandler.HandleErrorAsync(seriales))
            Serials = seriales.Response ?? new();

        await InvokeAsync(StateHasChanged);
    }
}
