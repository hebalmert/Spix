using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.Domain.Resources;
using Spix.DomainLogic.ResponcesSec;
using Spix.HttpService;

namespace Spix.AppFront.Pages.Auth;

public partial class ChangePassword
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigation { get; set; } = null!;
    [Inject] private HttpResponseHandler _httpHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    private ChangePasswordDTO changePasswordDTO = new();

    private async Task ChangePasswordAsync()
    {
        var responseHttp = await _repository.PostAsync("/api/v1/accounts/changePassword", changePasswordDTO);
        if (await _httpHandler.HandleErrorAsync(responseHttp)) return;
        _modalService.Close();
        _navigation.NavigateTo("/dashboard");
        await _sweetAlert.FireAsync(Localizer[nameof(Resource.PasswordUpdateTitle)], Localizer[nameof(Resource.PasswordUpdateMsg)], SweetAlertIcon.Success);
    }

    private void ReturnAction()
    {
        _modalService.Close();
        _navigation.NavigateTo("/dashboard");
    }
}