using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;

namespace Spix.AppFront.Layout;

public partial class LoginLayout
{
    [Inject] private ModalService _modalService { get; set; } = null!;

    private Type? modalType;
    private Dictionary<string, object>? modalParameters;

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
}