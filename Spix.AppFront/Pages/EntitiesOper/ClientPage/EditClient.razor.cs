using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesOper;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesOper.ClientPage;

public partial class EditClient
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private Client? Client;

    private string BaseUrl = "/api/v1/clients";
    private bool isLoading = false;
    private bool IsSaving = false;

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        StateHasChanged(); // fuerza mostrar el modal con spinner inmediatamente

        var responseHttp = await _repository.GetAsync<Client>($"{BaseUrl}/{Id}");
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        Client = responseHttp.Response;
    }

    private async Task Edit()
    {
        IsSaving = true;
        // Regla de negocio: si el cliente está inactivo, no puede tener cuenta
        if (!Client!.Active)
        {
            Client.CreateAccount = false;
        }
        Client.DocumentType = null;
        Client.Corporation = null;
        Client.ContractClients = null;

        var responseHttp = await _repository.PutAsync($"{BaseUrl}", Client);
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
