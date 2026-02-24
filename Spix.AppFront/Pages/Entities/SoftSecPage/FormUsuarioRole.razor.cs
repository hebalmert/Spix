using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Enum;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.Entities.SoftSecPage;

public partial class FormUsuarioRole
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public UsuarioRole UsuarioRole { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }

    private List<IntItemModel>? ListUserType;

    protected override async Task OnInitializedAsync()
    {
        await LoadRoles();
    }

    private async Task LoadRoles()
    {
        var responseHTTP = await _repository.GetAsync<List<IntItemModel>>($"api/v1/usuarioRoles/loadCombo");
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled) return;
        ListUserType = responseHTTP.Response;
    }

    private void UsertTypeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e?.Value?.ToString(), out int modelo))
        {
            if (modelo == 2) { UsuarioRole.UserType = UserType.Administrator; }
            if (modelo == 3) { UsuarioRole.UserType = UserType.Auxiliar; }
            if (modelo == 4) { UsuarioRole.UserType = UserType.Cachier; }
            if (modelo == 5) { UsuarioRole.UserType = UserType.Collector; }
            if (modelo == 7) { UsuarioRole.UserType = UserType.Technician; }
            if (modelo == 9) { UsuarioRole.UserType = UserType.WarehouseLead; }
        }
    }
}