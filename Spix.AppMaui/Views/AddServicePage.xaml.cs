using Spix.AppMaui.ViewModels;

namespace Spix.AppMaui.Views;

//Cargar el servicio realizado: pagina propia para no llenar la visita de campos
public partial class AddServicePage : ContentPage
{
    public AddServicePage(AddServiceViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
