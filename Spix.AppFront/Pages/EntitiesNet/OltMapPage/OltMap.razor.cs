using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesNet.OltMapPage;

//Mapa de OLT: al entrar pinta TODAS las OLT con su nombre y cuantos clientes tiene cada una;
//al elegir una, pinta sus clientes con la distancia al equipo. Todo lo trae su propio
//controlador (api/v1/oltmap) y lo pinta su propio modulo JS (jslib/oltMap.js).
public partial class OltMap : IAsyncDisposable
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private const string BaseUrl = "api/v1/oltmap";
    //El JS propio de esta pantalla (jslib/oltMap.js), aparte del de los otros mapas
    private const string MapJs = "spixOltMap";

    private readonly string MapId = $"spix-oltmap-{Guid.NewGuid():N}";

    private List<GuidNameModel>? Olts;
    private List<IntItemModel>? Views;

    //Las dos vistas: Map con una OLT elegida, AllOlts cuando esta el neutro
    private OltMapDto? Map;
    private List<OltMapItemDto> AllOlts = new();

    private Guid SelectedOltId;
    //Con cientos de clientes las etiquetas de distancia se amontonan: se arranca con solo puntos
    private int SelectedView = (int)OltMapViewType.Points;
    private Guid SelectedUnlocatedId;

    //El mapa se pinta despues de renderizar, cuando el div ya tiene su tamano final
    private bool PendingMapRender;
    private bool PendingAllRender;

    //El tablero de la vista de todas
    private int AllWithPoint => AllOlts.Count(x => x.Latitude.HasValue && x.Longitude.HasValue);

    private int AllWithoutPoint => AllOlts.Count - AllWithPoint;

    private int AllClients => AllOlts.Sum(x => x.Clients);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadOltsAsync();
            await LoadViewsAsync();
            await LoadAllAsync();
            return;
        }

        if (PendingAllRender)
        {
            PendingAllRender = false;
            await JS.InvokeVoidAsync($"{MapJs}.renderAll", MapId, AllOlts, Localizer["OltMap_Clients"].Value);
            return;
        }

        if (PendingMapRender && Map is not null)
        {
            PendingMapRender = false;
            await JS.InvokeVoidAsync($"{MapJs}.render", MapId, Map, SelectedView);
        }
    }

    private async Task LoadOltsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<GuidNameModel>>($"{BaseUrl}/olts");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        Olts = responseHttp.Response;
        StateHasChanged();
    }

    private async Task LoadViewsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/views");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        Views = responseHttp.Response;
        StateHasChanged();
    }

    //Todas las OLT: un solo request, y el conteo de clientes ya viene hecho por la base
    private async Task LoadAllAsync()
    {
        var responseHttp = await _repository.GetAsync<List<OltMapItemDto>>($"{BaseUrl}/all");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        AllOlts = responseHttp.Response ?? new();
        PendingAllRender = true;
        StateHasChanged();
    }

    //Una OLT: un solo request con sus clientes, sus distancias y el tablero.
    //Con el neutro se vuelve a la vista de todas.
    private async Task OltChanged(ChangeEventArgs e)
    {
        SelectedOltId = Guid.TryParse(e.Value?.ToString(), out var id) ? id : Guid.Empty;
        SelectedUnlocatedId = Guid.Empty;
        Map = null;

        if (SelectedOltId == Guid.Empty)
        {
            await LoadAllAsync();
            return;
        }

        var responseHttp = await _repository.GetAsync<OltMapDto>($"{BaseUrl}/{SelectedOltId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        Map = responseHttp.Response;
        PendingMapRender = true;
    }

    //Cambiar la vista solo redibuja las lineas: no vuelve a pedir nada al servidor
    private async Task ViewChanged(ChangeEventArgs e)
    {
        SelectedView = int.TryParse(e.Value?.ToString(), out var view) ? view : (int)OltMapViewType.Points;
        if (Map is null) return;

        await JS.InvokeVoidAsync($"{MapJs}.setView", MapId, SelectedView);
    }

    //Un cliente sin ubicacion no se puede pintar: se ofrece ir a su contrato
    private void UnlocatedChanged(ChangeEventArgs e)
    {
        SelectedUnlocatedId = Guid.TryParse(e.Value?.ToString(), out var id) ? id : Guid.Empty;
    }

    //Al salir de la pantalla se suelta el mapa
    public async ValueTask DisposeAsync()
    {
        try
        {
            await JS.InvokeVoidAsync($"{MapJs}.dispose", MapId);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (JSException)
        {
        }
    }
}
