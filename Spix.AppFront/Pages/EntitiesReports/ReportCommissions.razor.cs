using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//Lo que se le causo a cada contratista en el periodo y lo que se le queda debiendo
public partial class ReportCommissions
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports-finance";

    private List<ReportContractorCommissionDto>? Commissions;

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

        var comisiones = await _repository.GetAsync<List<ReportContractorCommissionDto>>($"{BaseUrl}/contractors?{periodo}");
        if (!await _responseHandler.HandleErrorAsync(comisiones))
            Commissions = comisiones.Response ?? new();

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }
}
