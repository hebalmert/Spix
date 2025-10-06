using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.ProductPage;

public partial class CreateProduct
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private Product Product = new();

    private string BaseUrl = "/api/v1/products";
    private string BaseView = "/products/details";

    [Parameter] public Guid Id { get; set; }  //ProductCategoryId

    private async Task Create()
    {
        Product.ProductCategoryId = Id;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", Product);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"/productcategories");
            return;
        }
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }

    private void Return()
    {
        _navigationManager.NavigateTo($"{BaseView}/{Id}");
    }
}