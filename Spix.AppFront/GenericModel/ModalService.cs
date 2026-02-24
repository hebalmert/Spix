using Microsoft.AspNetCore.Components;

namespace Spix.AppFront.GenericModel;

public class ModalResult
{
    public bool Succeeded { get; set; }

    public static ModalResult Ok() => new() { Succeeded = true };
    public static ModalResult Cancel() => new() { Succeeded = false };
}

public class ModalService
{
    public event Func<Type, Dictionary<string, object>?, Task>? OnShow;

    public event Action? OnClose;
    public event Func<Task>? OnCloseAsync;

    private Func<ModalResult, Task>? _onCloseCallback;

    //Versión genérica
    public async Task ShowAsync<T>(
        Dictionary<string, object>? parameters = null,
        Func<ModalResult, Task>? onCloseCallback = null)
        where T : ComponentBase
    {
        _onCloseCallback = onCloseCallback;

        if (OnShow != null)
            await OnShow(typeof(T), parameters);
    }

    //Versión por Type (para evitar duplicación en el padre)
    public async Task ShowAsync(
        Type componentType,
        Dictionary<string, object>? parameters = null,
        Func<ModalResult, Task>? onCloseCallback = null)
    {
        _onCloseCallback = onCloseCallback;

        if (OnShow != null)
            await OnShow(componentType, parameters);
    }

    //Cerrar con resultado (Ok / Cancel)
    public async Task CloseAsync(ModalResult result)
    {
        if (_onCloseCallback != null)
            await _onCloseCallback(result);   // callback local del padre

        if (OnCloseAsync != null)
            await OnCloseAsync();            // callback global opcional

        OnClose?.Invoke();
    }

    //Cerrar sin resultado (por compatibilidad)
    public void Close()
    {
        OnClose?.Invoke();
    }
}
