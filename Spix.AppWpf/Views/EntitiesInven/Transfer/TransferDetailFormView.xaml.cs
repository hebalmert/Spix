using Spix.AppWpf.ViewModels.EntitiesInven.Transfer;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Transfer;

// Los dos combos van en cascada, asi que el cambio se avisa por SelectionChanged y NO por
// binding TwoWay: con TwoWay el valor se escribe antes del evento y el hijo se limpia solo.
public partial class TransferDetailFormView : UserControl
{
    public TransferDetailFormView()
    {
        InitializeComponent();
    }

    private async void CategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is TransferDetailFormViewModel viewModel &&
            !viewModel.IsInitializingForm &&
            e.AddedItems.Count > 0 &&
            e.AddedItems[0] is Spix.Domain.EntitiesGen.ProductCategory category)
        {
            await viewModel.ChangeCategoryAsync(category.ProductCategoryId);
        }
    }

    private async void ProductSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is TransferDetailFormViewModel viewModel &&
            !viewModel.IsInitializingForm &&
            e.AddedItems.Count > 0 &&
            e.AddedItems[0] is Spix.Domain.EntitiesGen.Product product)
        {
            await viewModel.ChangeProductAsync(product.ProductId);
        }
    }
}
