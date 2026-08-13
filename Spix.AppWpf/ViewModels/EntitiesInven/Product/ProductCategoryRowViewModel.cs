using Spix.Domain.EntitiesGen;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ProductEntity = Spix.Domain.EntitiesGen.Product;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Product;

// Agrega el estado visual del acordeon sin modificar la entidad que recibe el Backend.
public class ProductCategoryRowViewModel : ProductCategory, INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isProductsLoading;
    private ObservableCollection<ProductEntity> _products = new();
    private string _productsMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool IsProductsLoading
    {
        get => _isProductsLoading;
        set => SetProperty(ref _isProductsLoading, value);
    }

    public ObservableCollection<ProductEntity> Products
    {
        get => _products;
        set
        {
            if (!SetProperty(ref _products, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasProducts));
        }
    }

    public string ProductsMessage
    {
        get => _productsMessage;
        set
        {
            if (!SetProperty(ref _productsMessage, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasProductsMessage));
        }
    }

    public bool HasProducts => Products.Count > 0;

    public bool HasProductsMessage => !string.IsNullOrWhiteSpace(ProductsMessage);

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
