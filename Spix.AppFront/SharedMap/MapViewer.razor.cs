using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Spix.xLanguage.Resources;
using Spix.xNetwork.MapHelper;

namespace Spix.AppFront.SharedMap;

//Pinta uno o dos puntos en el mapa; con dos, calcula la distancia en linea recta
public partial class MapViewer
{
    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    [Parameter] public decimal? Latitude { get; set; }
    [Parameter] public decimal? Longitude { get; set; }
    [Parameter] public decimal? SecondLatitude { get; set; }
    [Parameter] public decimal? SecondLongitude { get; set; }
    [Parameter] public string? FirstLabel { get; set; }
    [Parameter] public string? SecondLabel { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public int Height { get; set; } = 420;

    private string MapId { get; } = $"spix-map-{Guid.NewGuid():N}";

    private double? DistanceKm =>
        Latitude.HasValue && Longitude.HasValue && SecondLatitude.HasValue && SecondLongitude.HasValue
            ? GeoDistance.Kilometers(Latitude.Value, Longitude.Value, SecondLatitude.Value, SecondLongitude.Value)
            : null;

    //Textos por defecto en el idioma del usuario
    protected override void OnParametersSet()
    {
        FirstLabel ??= Localizer[nameof(Resource.Client)];
        SecondLabel ??= Localizer["Map_Node"];
        Title ??= Localizer["Map_Map"];
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Latitude is null || Longitude is null) return;

        await JsRuntime.InvokeVoidAsync("spixMap.render", MapId, new
        {
            Latitude,
            Longitude,
            SecondLatitude,
            SecondLongitude,
            FirstLabel,
            SecondLabel,
            DistanceText = DistanceKm.HasValue ? $"{DistanceKm.Value:N2} Km" : null,
            Title
        });
    }
}
