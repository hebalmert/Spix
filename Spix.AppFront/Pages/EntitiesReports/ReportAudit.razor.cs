using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//La bitacora del dinero: cada movimiento que quedo anotado, del mas nuevo al mas viejo
public partial class ReportAudit
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports-finance";
    private const int PageSize = 20;

    private List<ReportAuditDto>? Movements;
    private List<IntItemModel>? Events;

    //Arranca con el mes en curso y con todos los movimientos
    private string DateStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).ToString("yyyy-MM-dd");
    private string DateEnd = DateTime.Today.ToString("yyyy-MM-dd");
    private string Filter = string.Empty;
    private int EventType;
    private int CurrentPage = 1;
    private int TotalPages;
    private bool IsLoading;

    protected override async Task OnInitializedAsync()
    {
        var eventos = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/audit/comboevents");
        if (!await _responseHandler.HandleErrorAsync(eventos))
            Events = eventos.Response ?? new();

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

    //Cambiar el tipo de movimiento vuelve a preguntar desde la primera pagina
    private async Task EventChanged(ChangeEventArgs e)
    {
        EventType = int.TryParse(e.Value?.ToString(), out var tipo) ? tipo : 0;
        await SearchAsync();
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadAsync();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;

        var url = $"{BaseUrl}/audit?page={CurrentPage}&recordsnumber={PageSize}&eventtype={EventType}" +
                  $"&datestart={Uri.EscapeDataString(DateStart)}&dateend={Uri.EscapeDataString(DateEnd)}";

        if (!string.IsNullOrWhiteSpace(Filter))
            url += $"&filter={Uri.EscapeDataString(Filter)}";

        var responseHttp = await _repository.GetAsync<List<ReportAuditDto>>(url);
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Movements = responseHttp.Response ?? new();
            TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        }

        IsLoading = false;
        await InvokeAsync(StateHasChanged);
    }
}
