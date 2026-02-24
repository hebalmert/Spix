using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.Entities.ManagerPage;

public partial class EditManager
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public int Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private Manager? _Manager;
    private bool isLoading = false;
    private string BaseUrl = "/api/v1/managers";
    private string BaseView = "/managers";

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<Manager>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;
        _Manager = responseHttp.Response;
    }

    private async Task Edit()
    {
        if (_Manager!.CorporationId == 0)
        {
            await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_ValidationWarningTitle)], Localizer[nameof(Resource.msg_ValidationWarningMessage)], SweetAlertIcon.Warning);
            return;
        }
        isLoading = true;
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", _Manager);
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled) { isLoading = false; return; }

        isLoading = false;
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_UpdateSuccessTitle)], Localizer[nameof(Resource.msg_UpdateSuccessMessage)], SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}