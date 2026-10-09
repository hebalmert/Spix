using Spix.AppMaui.ViewModels;

namespace Spix.AppMaui.Views;

//Lo que viene de manana en adelante
public partial class UpcomingPage : ContentPage
{
    private readonly UpcomingViewModel _viewModel;

    public UpcomingPage(UpcomingViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
