using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.TaxPage;

public partial class EditTax
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private Tax? Tax;
    private string BaseUrl = "/api/v1/taxes";
    private string BaseView = "/taxes";
    private bool IsVisible = false;
    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsVisible = true;
        var responseHttp = await _repository.GetAsync<Tax>($"{BaseUrl}/{Id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            IsVisible = false;
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        IsVisible = false;
        Tax = responseHttp.Response;
    }

    private async Task Edit()
    {
        IsVisible = true;
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", Tax);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            IsVisible = false;
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        IsVisible = false;
        await _modalService.CloseAsync(ModalResult.Ok());
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_UpdateSuccessTitle)], Localizer[nameof(Resource.msg_UpdateSuccessMessage)], SweetAlertIcon.Success);
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}