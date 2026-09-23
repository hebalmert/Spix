using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Layout;

//El menu lateral. Solo un grupo queda abierto a la vez: por eso basta con guardar
//cual esta abierto, en vez de un booleano por grupo.
public partial class NavMenu
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private bool collapseNavMenu = true;

    //0 = todos cerrados
    private int Expanded;

    private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }

    //Abre el grupo, o lo cierra si ya estaba abierto
    private void Toggle(int group)
    {
        Expanded = Expanded == group ? 0 : group;
    }
}
