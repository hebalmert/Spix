using Spix.AppWpf.ViewModels.EntitiesSystem.Technitian;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSystem.Technitian;

// Recoge la foto que entrega el selector y se la pasa al formulario.
public partial class TechnitianFormView : UserControl
{
    public TechnitianFormView()
    {
        InitializeComponent();
    }

    // La foto llega en el mismo Base64 venga del disco o de la camara: al formulario le da igual
    private void PhotoSelected(object? sender, string base64)
    {
        if (DataContext is TechnitianFormViewModel viewModel)
        {
            viewModel.SetPhoto(base64);
        }
    }
}
