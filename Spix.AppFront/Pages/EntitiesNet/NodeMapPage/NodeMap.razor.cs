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

namespace Spix.AppFront.Pages.EntitiesNet.NodeMapPage;

//Mapa de nodos: elige un nodo y pinta sus clientes como puntos, con lineas al AP o con lineas y distancia.
//Todo lo trae su propio controlador (api/v1/nodemap), en un request por nodo, y el mapa lo pinta
//su propio modulo JS (NodeMap.razor.js): no comparte nada con los otros mapas.
public partial class NodeMap : IAsyncDisposable
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private const string BaseUrl = "api/v1/nodemap";
    //El JS propio de esta pantalla (jslib/nodeMap.js), aparte del de los otros mapas
    private const string MapJs = "spixNodeMap";

    private readonly string MapId = $"spix-nodemap-{Guid.NewGuid():N}";

    private List<GuidNameModel>? Nodes;
    private List<IntItemModel>? Views;
    private List<IntItemModel>? Coverages;
    private NodeMapDto? Map;

    private Guid SelectedNodeId;
    //Con cientos de clientes, las etiquetas de distancia se amontonan: se arranca con solo puntos
    private int SelectedView = (int)NodeMapViewType.Points;
    private Guid SelectedUnlocatedId;

    //Mascara del transmisor: su ancho en grados y hacia donde apunta
    private int SelectedCoverage;
    private int Azimuth;

    //El mapa se pinta despues de renderizar, cuando el div ya tiene su tamano final
    private bool PendingMapRender;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadNodesAsync();
            await LoadViewsAsync();
            await LoadCoveragesAsync();
            return;
        }

        if (PendingMapRender && Map is not null)
        {
            PendingMapRender = false;
            await JS.InvokeVoidAsync($"{MapJs}.render", MapId, Map, SelectedView);
            await DrawCoverageAsync();
        }
    }

    private async Task LoadNodesAsync()
    {
        var responseHttp = await _repository.GetAsync<List<GuidNameModel>>($"{BaseUrl}/nodes");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        Nodes = responseHttp.Response;
        StateHasChanged();
    }

    private async Task LoadViewsAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/views");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        Views = responseHttp.Response;
        StateHasChanged();
    }

    private async Task LoadCoveragesAsync()
    {
        var responseHttp = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/coverages");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        Coverages = responseHttp.Response;
        StateHasChanged();
    }

    //Un nodo: un solo request con sus clientes, sus distancias y el tablero
    private async Task NodeChanged(ChangeEventArgs e)
    {
        SelectedNodeId = Guid.TryParse(e.Value?.ToString(), out var id) ? id : Guid.Empty;
        SelectedUnlocatedId = Guid.Empty;
        Map = null;

        if (SelectedNodeId == Guid.Empty)
        {
            await JS.InvokeVoidAsync($"{MapJs}.dispose", MapId);
            return;
        }

        var responseHttp = await _repository.GetAsync<NodeMapDto>($"{BaseUrl}/{SelectedNodeId}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;

        Map = responseHttp.Response;
        PendingMapRender = true;
    }

    //Cambiar la vista solo redibuja las lineas: no vuelve a pedir nada al servidor
    private async Task ViewChanged(ChangeEventArgs e)
    {
        SelectedView = int.TryParse(e.Value?.ToString(), out var view) ? view : (int)NodeMapViewType.Points;
        if (Map is null) return;

        await JS.InvokeVoidAsync($"{MapJs}.setView", MapId, SelectedView);
    }

    //Cambiar el ancho de la mascara: se redibuja el sector con el azimut actual
    private async Task CoverageChanged(ChangeEventArgs e)
    {
        SelectedCoverage = int.TryParse(e.Value?.ToString(), out var degrees) ? degrees : 0;

        await DrawCoverageAsync();
    }

    //Girar la mascara: el vertice sigue en el AP
    private async Task AzimuthChanged(ChangeEventArgs e)
    {
        Azimuth = int.TryParse(e.Value?.ToString(), out var azimuth) ? azimuth : 0;

        await DrawCoverageAsync();
    }

    //Los botones - y + mueven de a un grado; en 0 y 359 la vuelta se cierra
    private async Task StepAzimuthAsync(int step)
    {
        Azimuth = (Azimuth + step + 360) % 360;

        await DrawCoverageAsync();
    }

    //El sector llega un poco mas alla del cliente mas lejano, para que los abarque a todos
    private async Task DrawCoverageAsync()
    {
        if (Map is null) return;

        var radiusKm = Math.Max(0.5, (Map.FarthestKm ?? 0) * 1.15);

        await JS.InvokeVoidAsync($"{MapJs}.setCoverage", MapId, SelectedCoverage, Azimuth, radiusKm);
    }

    //Los clientes que caen dentro del sector: se sabe con el rumbo que calculo el backend
    private int InsideCoverage
    {
        get
        {
            if (Map is null || SelectedCoverage <= 0) return 0;

            var half = SelectedCoverage / 2d;

            return Map.Located.Count(x => x.BearingDeg.HasValue && Difference(x.BearingDeg.Value, Azimuth) <= half);
        }
    }

    //Cuantos grados separan dos rumbos, por el lado corto de la brujula
    private static double Difference(double bearing, double azimuth)
    {
        var diff = Math.Abs(bearing - azimuth) % 360;

        return diff > 180 ? 360 - diff : diff;
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
