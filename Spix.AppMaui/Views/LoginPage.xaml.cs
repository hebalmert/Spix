using Spix.AppMaui.ViewModels;

namespace Spix.AppMaui.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
