using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.ProductPage;

public partial class EditProduct
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private Product? Product;

    private string BaseUrl = "/api/v1/products";
    private string BaseView = "/products/details";

    [Parameter] public Guid Id { get; set; }  //ProductId

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<Product>($"{BaseUrl}/{Id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Product = responseHttp.Response;
    }

    private async Task Edit()
    {
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", Product);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}/{Product!.ProductCategoryId}");
            return;
        }
        _navigationManager.NavigateTo($"{BaseView}/{Product!.ProductCategoryId}");
    }

    private void Return()
    {
        _navigationManager.NavigateTo($"{BaseView}/{Product!.ProductCategoryId}");
    }
}