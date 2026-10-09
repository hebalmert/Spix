using Spix.AppMaui.ViewModels;

namespace Spix.AppMaui.Views;

//Lo que cerro hoy, para consultarlo sin llamar a la oficina
public partial class ClosedPage : ContentPage
{
    private readonly ClosedViewModel _viewModel;

    public ClosedPage(ClosedViewModel viewModel)
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
