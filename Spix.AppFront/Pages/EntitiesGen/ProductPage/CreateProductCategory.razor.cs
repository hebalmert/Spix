using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;
namespace Spix.AppFront.Pages.EntitiesGen.ProductPage;

public partial class CreateProductCategory
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private ProductCategory ProductCategory = new() { Active = true };

    private string BaseUrl = "/api/v1/productcategories";
    private bool isLoading = false;
    private bool IsSaving = false;
    [Parameter] public string? Title { get; set; }

    private async Task Create()
    {
        IsSaving = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", ProductCategory);
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