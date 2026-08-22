using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Shared;

public partial class FilterComponent
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Parameter] public string FilterValue { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ApplyFilter { get; set; }

    //Tope de ancho del buscador. Sin esto se estira hasta el final de la columna y queda enorme
    //en las pantallas anchas. Se puede cambiar por pagina: <FilterComponent MaxWidth="480px" ... />
    [Parameter] public string MaxWidth { get; set; } = "620px";

    private async Task ClearFilter()
    {
        FilterValue = string.Empty;
        await ApplyFilter.InvokeAsync(FilterValue);
    }

    private async Task OnfilterApply()
    {
        await ApplyFilter.InvokeAsync(FilterValue);
    }
}