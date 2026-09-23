using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesPayment.TechnicianCollectionPage;

//Lo que recogio un tecnico entre dos fechas: para recibirle la plata y cuadrar.
//Lee de los abonos, que es donde vive el dinero: nada se duplica en otra tabla.
public partial class IndexTechnicianCollection
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v1/technician-collections";
    private const int PageSize = 15;

    private List<GuidItemModel>? Collectors;
    private List<TechnicianCollectionDto>? Collections = new();
    private TechnicianCollectionSummaryDto? Summary;

    private Guid UserId;
    private string DateStart = DateTime.Today.AddDays(-7).ToString("yyyy-MM-dd");
    private string DateEnd = DateTime.Today.ToString("yyyy-MM-dd");
    private int CurrentPage = 1;
    private int TotalPages;
    private bool IsLoading;

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>($"{BaseUrl}/combocollectors");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Collectors = responseHttp.Response ?? new();
    }

    private void CollectorChanged(ChangeEventArgs e)
    {
        UserId = Guid.TryParse(e.Value?.ToString(), out var id) ? id : Guid.Empty;
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
        if (UserId == Guid.Empty)
            return;

        CurrentPage = 1;
        IsLoading = true;
        await LoadSummaryAsync();
        await LoadCollectionsAsync();
        IsLoading = false;
    }

    private async Task LoadSummaryAsync()
    {
        var responseHttp = await _repository.GetAsync<TechnicianCollectionSummaryDto>(
            $"{BaseUrl}/summary/{UserId}?{Periodo()}");

        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Summary = responseHttp.Response;
    }

    private async Task LoadCollectionsAsync(int page = 1)
    {
        var responseHttp = await _repository.GetAsync<List<TechnicianCollectionDto>>(
            $"{BaseUrl}/{UserId}?page={page}&recordsnumber={PageSize}&{Periodo()}");

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Collections = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    private string Periodo() =>
        $"datestart={Uri.EscapeDataString(DateStart)}&dateend={Uri.EscapeDataString(DateEnd)}";

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadCollectionsAsync(page);
    }

    //El modo de pago, en palabras
    private string ModeName(string? mode) => mode switch
    {
        "Cash" => Localizer["Pay_Cash"],
        "Card" => Localizer["Pay_Card"],
        "Transfer" => Localizer["Pay_Transfer"],
        _ => mode ?? string.Empty
    };
}
