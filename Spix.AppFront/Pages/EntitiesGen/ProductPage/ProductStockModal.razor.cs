using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.ProductPage;

public partial class ProductStockModal
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "/api/v1/productStocks";
    private bool IsLoading = true;

    public List<ProductStock>? ProductStocks { get; set; }
    public Product? Product { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadStock();
    }

    private async Task LoadStock()
    {
        IsLoading = true;

        var productResponse = await _repository.GetAsync<Product>($"/api/v1/products/{Id}");
        if (await _responseHandler.HandleErrorAsync(productResponse))
        {
            IsLoading = false;
            return;
        }

        var stockResponse = await _repository.GetAsync<List<ProductStock>>($"{BaseUrl}?guidId={Id}&page=1&recordsnumber=100");
        if (await _responseHandler.HandleErrorAsync(stockResponse))
        {
            IsLoading = false;
            return;
        }

        Product = productResponse.Response;
        ProductStocks = stockResponse.Response ?? new List<ProductStock>();
        IsLoading = false;
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
