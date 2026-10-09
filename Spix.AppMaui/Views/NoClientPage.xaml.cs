using Spix.AppMaui.ViewModels;

namespace Spix.AppMaui.Views;

//Fui y no habia nadie
public partial class NoClientPage : ContentPage
{
    public NoClientPage(NoClientViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
