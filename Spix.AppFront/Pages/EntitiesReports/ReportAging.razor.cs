using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//La cartera que esta viva hoy, repartida por lo vieja que es la nota
public partial class ReportAging
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports-finance";

    //Solo se muestran los 20 que mas deben: la lista es para salir a cobrar, no para leerla entera
    private const int TopDebtors = 20;

    private ReportAgingDto? Summary;
    private List<ReportDebtorDto>? Debtors;

    protected override async Task OnInitializedAsync()
    {
        var resumen = await _repository.GetAsync<ReportAgingDto>($"{BaseUrl}/aging/summary");
        if (!await _responseHandler.HandleErrorAsync(resumen))
            Summary = resumen.Response;

        var deudores = await _repository.GetAsync<List<ReportDebtorDto>>($"{BaseUrl}/aging/debtors?top={TopDebtors}");
        if (!await _responseHandler.HandleErrorAsync(deudores))
            Debtors = deudores.Response ?? new();

        await InvokeAsync(StateHasChanged);
    }
}
