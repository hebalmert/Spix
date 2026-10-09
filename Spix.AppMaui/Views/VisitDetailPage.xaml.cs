using Spix.AppMaui.ViewModels;

namespace Spix.AppMaui.Views;

//La visita abierta: donde el tecnico trabaja
public partial class VisitDetailPage : ContentPage
{
    public VisitDetailPage(VisitDetailViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
