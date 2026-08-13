using Spix.AppWpf.ViewModels.EntitiesInven.Product;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Product;

// Conserva sincronizados los modelos cuando el usuario cambia la marca del producto.
public partial class ProductFormView : UserControl
{
    public ProductFormView()
    {
        InitializeComponent();
    }

    private async void MarkSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not ProductFormViewModel viewModel)
        {
            return;
        }

        if (e.AddedItems.Count == 0 || e.AddedItems[0] is not Spix.Domain.EntitiesGen.Mark mark)
        {
            return;
        }

        await viewModel.ChangeMarkAsync(mark.MarkId);
    }
}
