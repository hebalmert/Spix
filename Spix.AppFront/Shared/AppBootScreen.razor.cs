using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Shared;

// Pantalla de espera con la misma imagen del splash de index.html.
// Se usa mientras el sistema verifica la sesion (Authorizing del Router y LoginPage).
public partial class AppBootScreen
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    [Parameter] public string? Message { get; set; }

    protected override void OnParametersSet()
    {
        Message ??= Localizer[nameof(Resource.App_Authorizing)];
    }
}
