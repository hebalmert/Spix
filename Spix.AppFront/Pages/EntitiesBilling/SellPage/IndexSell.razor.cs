using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesBilling;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesBilling.SellPage;

public partial class IndexSell
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private const string BaseUrl = "api/v1/sells";
    private string Filter { get; set; } = string.Empty;
    private List<Sell>? Sells { get; set; }

    //Los numeros del mes, contados por la base
    private SellSummaryDto? Summary { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadSummaryAsync();
            await LoadAsync();
        }
    }

    //El tablero se pide una sola vez al abrir: no se recalcula en cada pagina
    private async Task LoadSummaryAsync()
    {
        var responseHttp = await _repository.GetAsync<SellSummaryDto>($"{BaseUrl}/summary");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Summary = responseHttp.Response;
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await LoadAsync();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadAsync(page);
    }

    private async Task LoadAsync(int page = 1)
    {
        var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
            url += $"&filter={Uri.EscapeDataString(Filter)}";

        var responseHttp = await _repository.GetAsync<List<Sell>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        Sells = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    //El detalle sale de lo que ya trajo el listado: no se vuelve a pedir nada al servidor
    private async Task ShowDetailsAsync(Sell sell)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Sell", sell },
            { "Title", $"{Localizer["Sell_Details"]} {sell.InvoiceNumber}" }
        };

        await _modalService.ShowAsync(typeof(DetailsSell), parameters);
    }
}
