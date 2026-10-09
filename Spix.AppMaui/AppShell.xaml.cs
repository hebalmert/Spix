using Spix.AppMaui.Views;

namespace Spix.AppMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        //Las dos pantallas que se abren desde una lista: se registran para poder
        //navegar con parametro (visit?id=...)
        Routing.RegisterRoute("visit", typeof(VisitDetailPage));
        Routing.RegisterRoute("noclient", typeof(NoClientPage));
        Routing.RegisterRoute("addservice", typeof(AddServicePage));
    }
}
