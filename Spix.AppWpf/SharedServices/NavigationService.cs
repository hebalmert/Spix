using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedServices;

// Abrir una PANTALLA (no un modal) desde un ViewModel.
//
// Hasta ahora todo el escritorio se movia por el menu, pero hay pantallas que se abren
// desde una fila: la orden de trabajo de una solicitud, igual que en la web, que va a
// /servicerequests/details/{id}. Una orden es demasiado para un modal.
//
// La ventana principal se suscribe y es la unica que sabe pintar: aqui solo se pide.
public class NavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public event EventHandler<NavigationRequest>? Requested;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // Resuelve la vista con su ViewModel inyectado y, si hace falta, la prepara antes de
    // mostrarla: asi la pantalla ya sabe de que registro es cuando aparece.
    public void Show<TView>(string title, string subtitle, Action<TView>? preparar = null)
        where TView : UserControl
    {
        var vista = _serviceProvider.GetRequiredService<TView>();

        preparar?.Invoke(vista);

        Requested?.Invoke(this, new NavigationRequest(vista, title, subtitle));
    }
}

// Lo que la ventana principal necesita para presentar una pantalla
public class NavigationRequest
{
    public UserControl View { get; }

    public string Title { get; }

    public string Subtitle { get; }

    public NavigationRequest(UserControl view, string title, string subtitle)
    {
        View = view;
        Title = title;
        Subtitle = subtitle;
    }
}
