using Spix.AppWpf.ViewModels.EntitiesMK.QueueType;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesMK.QueueType;

// Presenta el listado paginado de Queue Types del Backend existente.
public partial class QueueTypeIndexView : UserControl
{
    public QueueTypeIndexView(QueueTypeIndexViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadAsync();
    }
}
