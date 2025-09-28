using Microsoft.AspNetCore.Components;

namespace Spix.AppFront.GenericModal;

public class ModalService
{
    public event Func<Type, Dictionary<string, object>?, Task>? OnShow;

    public event Action? OnClose;

    public async Task ShowAsync<T>(Dictionary<string, object>? parameters = null) where T : ComponentBase
    {
        if (OnShow != null)
            await OnShow(typeof(T), parameters);
    }

    public void Close()
    {
        OnClose?.Invoke();
    }
}