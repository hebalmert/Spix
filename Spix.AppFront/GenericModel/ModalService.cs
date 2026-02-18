using Microsoft.AspNetCore.Components;

namespace Spix.AppFront.GenericModel;

public class ModalService
{
    public event Func<Type, Dictionary<string, object>?, Task>? OnShow;

    public event Action? OnClose;
    public event Func<Task>? OnCloseAsync;

    public async Task ShowAsync<T>(Dictionary<string, object>? parameters = null)
        where T : ComponentBase
    {
        if (OnShow != null)
            await OnShow(typeof(T), parameters);
    }

    public async Task CloseAsync()
    {
        if (OnCloseAsync != null)
            await OnCloseAsync();

        OnClose?.Invoke();
    }

    public void Close()
    {
        OnClose?.Invoke();
    }
}
