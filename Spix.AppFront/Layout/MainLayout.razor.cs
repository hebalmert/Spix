using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;

namespace Spix.AppFront.Layout;

public partial class MainLayout
{
    [Inject] private ModalService _modalService { get; set; } = null!;

    private bool isMenuVisible = false;
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

    }

    private void ShowMenu() => isMenuVisible = true;

    private void HideMenu() => isMenuVisible = false;
}
