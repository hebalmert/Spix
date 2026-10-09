using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Localization;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Layout;

//El menu lateral. Solo un grupo queda abierto a la vez: por eso basta con guardar
//cual esta abierto, en vez de un booleano por grupo.
public partial class NavMenu : IDisposable
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private AuthenticationStateProvider _authProvider { get; set; } = null!;

    private bool collapseNavMenu = true;

    //0 = todos cerrados
    private int Expanded;

    private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    //Lo que hay por revisar, para el numerito del item Revision. Null = no se muestra.
    private int? ReviewCount;

    protected override async Task OnInitializedAsync()
    {
        _navigationManager.LocationChanged += AlCambiarDePantalla;
        await CargarRevisionAsync();
    }

    //Se recarga al cambiar de pantalla: mientras estas EN la bandeja el numero vive en
    //las pestañas, asi que no hace falta avisarle al menu desde alla.
    private void AlCambiarDePantalla(object? sender, LocationChangedEventArgs e)
    {
        _ = RefrescarRevisionAsync();
    }

    private async Task RefrescarRevisionAsync()
    {
        await CargarRevisionAsync();
        await InvokeAsync(StateHasChanged);
    }

    //Sin HttpResponseHandler a proposito: si falla, el menu se pinta sin numerito. Un
    //contador no puede sacar al usuario de la pantalla ni levantarle una alerta.
    private async Task CargarRevisionAsync()
    {
        if (!await EsDeOficinaAsync())
        {
            ReviewCount = null;
            return;
        }

        var responseHttp = await _repository.GetAsync<VisitReviewCountersDto>("/api/v1/visitreviews/counters");
        if (responseHttp.Error || responseHttp.Response is null)
        {
            ReviewCount = null;
            return;
        }

        ReviewCount = responseHttp.Response.Total;
    }

    //El endpoint es de la oficina: al tecnico y al cliente no se les pide
    private async Task<bool> EsDeOficinaAsync()
    {
        var state = await _authProvider.GetAuthenticationStateAsync();
        var user = state.User;

        return user.Identity?.IsAuthenticated == true &&
               (user.IsInRole("Administrator") || user.IsInRole("Auxiliar"));
    }

    public void Dispose()
    {
        _navigationManager.LocationChanged -= AlCambiarDePantalla;
    }

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
