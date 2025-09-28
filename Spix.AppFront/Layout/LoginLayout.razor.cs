using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModal;

namespace Spix.AppFront.Layout;

public partial class LoginLayout
{
    [Inject] private ILocalStorageService _localStorage { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private Type? modalType;
    private Dictionary<string, object>? modalParameters;

    protected override async Task OnInitializedAsync()
    {
        _modalService.OnShow += async (type, parameters) =>
        {
            modalType = type;
            modalParameters = parameters;
            StateHasChanged();
        };

        _modalService.OnClose += () =>
        {
            modalType = null;
            modalParameters = null;
            StateHasChanged();
        };
        await _localStorage.RemoveItemAsync("lastActivity");
    }
}