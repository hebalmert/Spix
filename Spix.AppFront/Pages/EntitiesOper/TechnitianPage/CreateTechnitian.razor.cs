using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesOper;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesOper.TechnitianPage;

public partial class CreateTechnitian
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string? Title { get; set; }

    private Technician Technician = new();

    private string BaseUrl = "/api/v1/technitians";
    private bool isLoading = false;
    private bool IsSaving = false;

    protected override void OnInitialized()
    {
        Technician.Active = true;
    }

    private async Task Create()
    {
        IsSaving = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", Technician);
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