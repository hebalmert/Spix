using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesOper;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesOper.ContractorPage;

public partial class EditContractor
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private Contractor? Contractor;

    private string BaseUrl = "/api/v1/contractors";
    private bool isLoading = false;
    private bool IsSaving = false;
    private bool IsSendingEmail = false;

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        StateHasChanged(); // fuerza mostrar el modal con spinner inmediatamente

        var responseHttp = await _repository.GetAsync<Contractor>($"{BaseUrl}/{Id}");
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        Contractor = responseHttp.Response;
    }

    private async Task Edit()
    {
        IsSaving = true;
        // Regla de negocio: si el cliente está inactivo, no puede tener cuenta
        if (!Contractor!.Active)
        {
            Contractor.CreateAccount = false;
        }
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", Contractor);
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
        if (Contractor is null)
            return;

        IsSendingEmail = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/{Contractor.ContractorId}/re-email", new { });
        IsSendingEmail = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Re-Email", "Correo de activacion enviado correctamente.", SweetAlertIcon.Success);
    }
}
