using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.ServiceRequestPage;

public partial class DetailServiceRequest
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }

    private const string BaseUrl = "/api/v1/servicerequests";
    private const string BaseView = "/servicerequests";

    private ServiceRequestDto? Model;
    private bool isLoading = true;
    private bool IsSaving;

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<ServiceRequestDto>($"{BaseUrl}/{Id}");

        isLoading = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo(BaseView);
            return;
        }

        Model = responseHttp.Response;
    }

    //Guarda la tarjeta de la visita. El recorrido, los servicios y las fotos se
    //guardan solos, por eso aqui no se sale de la pantalla.
    private async Task SaveAsync()
    {
        if (!await ValidateAsync())
            return;

        IsSaving = true;
        var responseHttp = await _repository.PutAsync(BaseUrl, Model);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync(
            Localizer[nameof(Resource.msg_UpdateSuccessTitle)],
            Localizer[nameof(Resource.msg_UpdateSuccessMessage)],
            SweetAlertIcon.Success);

        //Al cerrar la orden ya no hay nada mas que hacer aqui
        if (Model!.ScheduleStatus == ScheduleStatus.Completed)
        {
            _navigationManager.NavigateTo(BaseView);
        }
    }

    private async Task<bool> ValidateAsync()
    {
        if (Model!.TechnicianId == Guid.Empty)
        {
            await _sweetAlert.FireAsync("Validacion", "Debe seleccionar un tecnico activo.", SweetAlertIcon.Warning);
            return false;
        }

        if (Model.ScheduledAtUtc == default)
        {
            await _sweetAlert.FireAsync("Validacion", "Debe seleccionar fecha y hora programada.", SweetAlertIcon.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(Model.ClientReason))
        {
            await _sweetAlert.FireAsync("Validacion", "Debe indicar la razon de la llamada.", SweetAlertIcon.Warning);
            return false;
        }

        return true;
    }

    private void Return()
    {
        _navigationManager.NavigateTo(BaseView);
    }
}
