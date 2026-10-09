using Spix.AppMaui.ViewModels;

namespace Spix.AppMaui.Views;

//Lo que hay que resolver hoy
public partial class TodayPage : ContentPage
{
    private readonly TodayViewModel _viewModel;

    public TodayPage(TodayViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    //Al volver de cerrar una visita la lista tiene que estar al dia
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
