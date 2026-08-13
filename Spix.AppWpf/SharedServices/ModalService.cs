using Microsoft.Extensions.DependencyInjection;
using Spix.AppWpf.SharedComponents;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedServices;

// Coordina modales desde cualquier ViewModel con el mismo resultado Ok o Cancel de Blazor.
public class ModalService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<SharedModalWindow> _activeModals = new();

    public ModalService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<ModalResult> ShowAsync<TContent>(
        string title,
        IReadOnlyDictionary<string, object>? parameters = null)
        where TContent : UserControl, ISharedModalContent
    {
        var content = _serviceProvider.GetRequiredService<TContent>();
        content.SetParameters(parameters);

        var modalWindow = _serviceProvider.GetRequiredService<SharedModalWindow>();
        modalWindow.Owner = GetActiveWindow();
        modalWindow.Configure(title, content);
        _activeModals.Push(modalWindow);

        try
        {
            modalWindow.ShowDialog();
        }
        finally
        {
            _activeModals.Pop();
        }

        return Task.FromResult(modalWindow.Result);
    }

    // Permite que el formulario cierre el host sin depender de una ventana concreta.
    public Task CloseAsync(ModalResult result)
    {
        if (_activeModals.TryPeek(out var activeModal))
        {
            activeModal.Complete(result);
        }

        return Task.CompletedTask;
    }

    private static Window? GetActiveWindow()
    {
        return Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive)
            ?? Application.Current.MainWindow;
    }
}
