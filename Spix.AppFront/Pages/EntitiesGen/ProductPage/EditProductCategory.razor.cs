using CurrieTechnologies.Razor.SweetAlert2;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Spix.AppFront.Pages.EntitiesGen.ProductPage;
public partial class EditProductCategory
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private ProductCategory? ProductCategory;

    private string BaseUrl = "/api/v1/productcategories";
    private bool isLoading = false;
    private bool IsSaving = false;
    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        StateHasChanged(); // fuerza mostrar el modal con spinner inmediatamente

        var responseHttp = await _repository.GetAsync<ProductCategory>($"{BaseUrl}/{Id}");
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        ProductCategory = responseHttp.Response;
    }

    private async Task Edit()
    {
        IsSaving = true;
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", ProductCategory);
        IsSaving = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}