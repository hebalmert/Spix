using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitesSoftSec;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.Entities.SoftSecPage;

public partial class EditUsuario
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private Usuario? Usuario;
    private string BaseUrl = "/api/v1/usuarios";
    private bool isLoading = false;
    private bool IsSaving = false;
    private bool IsSendingEmail = false;
    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        var responseHttp = await _repository.GetAsync<Usuario>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;
        Usuario = responseHttp.Response;
        isLoading = false;
    }

    private async Task Edit()
    {
        IsSaving = true;
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", Usuario);
        IsSaving = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_UpdateSuccessTitle)], Localizer[nameof(Resource.msg_UpdateSuccessMessage)], SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }

    private async Task ResendActivationEmailAsync()
    {
        if (Usuario is null)
            return;

        IsSendingEmail = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/{Usuario.UsuarioId}/re-email", new { });
        IsSendingEmail = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Re-Email", "Correo de activacion enviado correctamente.", SweetAlertIcon.Success);
    }
}
