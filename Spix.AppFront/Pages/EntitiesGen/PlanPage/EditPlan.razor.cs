using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.AppFront.ShareLoading;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.PlanPage;

public partial class EditPlan
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private Plan? Plan;

    private string BaseUrl = "/api/v1/plans";
    private bool isLoading = false;
    private bool IsSaving = false;
    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        StateHasChanged(); // fuerza mostrar el modal con spinner inmediatamente

        var responseHttp = await _repository.GetAsync<Plan>($"{BaseUrl}/{Id}");
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        Plan = responseHttp.Response;
    }

    private async Task Edit()
    {
        if (Plan!.TasaReuso == 0 || Plan.PlanName == null || Plan.PlanName == string.Empty || Plan.Price <= 0)
        {
            await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_ValidationWarningTitle)], Localizer[nameof(Resource.msg_ValidationWarningMessage)], SweetAlertIcon.Warning);
            return;
        }
        IsSaving = true;
        var responseHttp = await _repository.PutAsync($"{BaseUrl}", Plan);
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