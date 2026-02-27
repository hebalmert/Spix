using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.RegisterPage;

public partial class EditRegister
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private Register? Register;
    private string BaseUrl = "/api/v1/registers";
    private string BaseView = "/registers";
    private bool IsVisible = false;
    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsVisible = true;
        var responseHttp = await _repository.GetAsync<Register>($"{BaseUrl}/{Id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            IsVisible = false;
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        IsVisible = false;
        Register = responseHttp.Response;
    }

    private async Task Edit()
    {
        IsVisible = true;
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", Register);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            IsVisible = false;
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        IsVisible = false;
        await _modalService.CloseAsync(ModalResult.Ok());
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}