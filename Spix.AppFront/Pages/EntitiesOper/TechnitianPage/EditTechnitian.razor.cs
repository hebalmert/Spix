using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesOper;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesOper.TechnitianPage;

public partial class EditTechnitian
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private Technician? Technician;

    private string BaseUrl = "/api/v1/technitians";
    private bool isLoading = false;
    private bool IsSaving = false;
    private bool IsSendingEmail = false;

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        StateHasChanged(); // fuerza mostrar el modal con spinner inmediatamente

        var responseHttp = await _repository.GetAsync<Technician>($"{BaseUrl}/{Id}");
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        Technician = responseHttp.Response;
    }

    private async Task Edit()
    {
        IsSaving = true;
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", Technician);
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

    private async Task ResendActivationEmailAsync()
    {
        if (Technician is null)
            return;

        IsSendingEmail = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/{Technician.TechnicianId}/re-email", new { });
        IsSendingEmail = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Re-Email", "Correo de activacion enviado correctamente.", SweetAlertIcon.Success);
    }
}
