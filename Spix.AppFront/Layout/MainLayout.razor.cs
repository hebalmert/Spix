using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;

namespace Spix.AppFront.Layout;

public partial class MainLayout
{
    [Inject] private ModalService _modalService { get; set; } = null!;

    private bool isMenuVisible = false;
    private bool isSidebarCollapsed = false;
    private Type? modalType;
    private Dictionary<string, object>? modalParameters;
    private string SidebarCssClass => isSidebarCollapsed ? "sidebar sidebar-collapsed" : "sidebar";
    private string SidebarToggleIcon => isSidebarCollapsed ? "fa-chevron-right" : "fa-chevron-left";
    private string SidebarToggleTitle => isSidebarCollapsed ? "Expandir menu" : "Contraer menu";

    protected override void OnInitialized()
    {
        _modalService.OnShow += ShowModal;
        _modalService.OnClose += CloseModal;
    }

    private Task ShowModal(Type type, Dictionary<string, object>? parameters)
    {
        modalType = type;
        modalParameters = parameters;
        InvokeAsync(StateHasChanged);
        return Task.CompletedTask;
    }

    private void CloseModal()
    {
        modalType = null;
        modalParameters = null;
        InvokeAsync(StateHasChanged);
    }

    private void ShowMenu() => isMenuVisible = true;

    private void HideMenu() => isMenuVisible = false;

    private void ToggleSidebar() => isSidebarCollapsed = !isSidebarCollapsed;
}
