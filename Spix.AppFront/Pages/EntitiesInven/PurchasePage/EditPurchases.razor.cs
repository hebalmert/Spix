using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;
namespace Spix.AppFront.Pages.EntitiesInven.PurchasePage;

public partial class EditPurchases
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private string BaseUrl = "api/v1/purchases";
    private Purchase? Purchase;
    private bool isLoading = false;
    private FormPurchase? FormPurchase { get; set; }

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        var responseHTTP = await _repository.GetAsync<Purchase>($"{BaseUrl}/{Id}");
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        Purchase = responseHTTP.Response;
    }

    private async Task Edit()
    {
        isLoading = true;
        var responseHTTP = await _repository.PutAsync($"{BaseUrl}", Purchase);
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        await _modalService.CloseAsync(ModalResult.Ok());
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}