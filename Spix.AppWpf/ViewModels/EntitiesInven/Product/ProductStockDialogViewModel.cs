using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ProductEntity = Spix.Domain.EntitiesGen.Product;
using ProductStockEntity = Spix.Domain.EntitiesInven.ProductStock;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Product;

// Muestra en que bodegas esta repartido el stock de un producto. Es el mismo
// ProductStockModal de Blazor: solo consulta, no cambia nada.
public partial class ProductStockDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/productStocks";

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _productName = string.Empty;

    public ObservableCollection<ProductStockEntity> Stocks { get; } = new();

    public bool HasStocks => Stocks.Count > 0;

    public ProductStockDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _modalService = modalService;
        _responseHandler = responseHandler;
    }

    public async Task LoadAsync(Guid productId)
    {
        IsLoading = true;

        try
        {
            var producto = await _repository.GetAsync<ProductEntity>($"api/v1/products/{productId}");
            if (await _responseHandler.HandleErrorAsync(producto))
            {
                return;
            }

            ProductName = producto.Response?.ProductName ?? string.Empty;

            var existencias = await _repository.GetAsync<List<ProductStockEntity>>(
                $"{BaseUrl}?guidId={productId}&page=1&recordsnumber=100");

            if (await _responseHandler.HandleErrorAsync(existencias))
            {
                return;
            }

            Stocks.Clear();

            foreach (var item in existencias.Response ?? new List<ProductStockEntity>())
            {
                Stocks.Add(item);
            }

            OnPropertyChanged(nameof(HasStocks));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
