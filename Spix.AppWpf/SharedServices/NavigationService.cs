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
        //Se entrega la RECETA y no la vista ya hecha: al cambiar el idioma la ventana
        //vuelve a llamarla para levantar una pantalla nueva, que pide sus datos otra vez.
        UserControl Armar()
        {
            var vista = _serviceProvider.GetRequiredService<TView>();

            preparar?.Invoke(vista);

            return vista;
        }

        Requested?.Invoke(this, new NavigationRequest(Armar, title, subtitle));
    }
}

// Lo que la ventana principal necesita para presentar una pantalla
public class NavigationRequest
{
    public Func<UserControl> Build { get; }

    public string Title { get; }

    public string Subtitle { get; }

    public NavigationRequest(Func<UserControl> build, string title, string subtitle)
    {
        Build = build;
        Title = title;
        Subtitle = subtitle;
    }
}
