using Spix.AppWpf.ViewModels.EntitiesInven.Purchase;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Purchase;

// Delega los cambios de select al ViewModel para conservar los productos dependientes de categoria.
public partial class PurchaseDetailFormView : UserControl
{
    public PurchaseDetailFormView()
    {
        InitializeComponent();
    }

    private async void CategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is PurchaseDetailFormViewModel viewModel &&
            !viewModel.IsInitializingForm &&
            e.AddedItems.Count > 0 &&
            e.AddedItems[0] is Spix.Domain.EntitiesGen.ProductCategory category)
        {
            await viewModel.ChangeCategoryAsync(category.ProductCategoryId);
        }
    }

    private async void ProductSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is PurchaseDetailFormViewModel viewModel &&
            !viewModel.IsInitializingForm &&
            e.AddedItems.Count > 0 &&
            e.AddedItems[0] is Spix.Domain.EntitiesGen.Product product)
        {
            await viewModel.ChangeProductAsync(product.ProductId);
        }
    }
}
