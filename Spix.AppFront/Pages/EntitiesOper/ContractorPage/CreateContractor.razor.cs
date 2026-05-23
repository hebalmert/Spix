using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesOper;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesOper.ContractorPage;

public partial class CreateContractor
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string? Title { get; set; }

    private Contractor Contractor = new();

    private string BaseUrl = "/api/v1/contractors";
    private bool isLoading = false;
    private bool IsSaving = false;

    protected override void OnInitialized()
    {
        Contractor.CreateAccount = true;
        Contractor.Active = true;
    }

    private async Task Create()
    {
        IsSaving = true;
        // Regla de negocio: si el cliente está inactivo, no puede tener cuenta
        if (!Contractor.Active)
        {
            Contractor.CreateAccount = false;
        }
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", Contractor);
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