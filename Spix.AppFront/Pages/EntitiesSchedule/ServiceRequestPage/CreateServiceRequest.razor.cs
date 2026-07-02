using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.ServiceRequestPage;

public partial class CreateServiceRequest
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string? Title { get; set; }

    private ServiceRequestDto Model = new()
    {
        ScheduledAtUtc = DateTime.Now.AddHours(1).ToUniversalTime(),
        ScheduleStatus = ScheduleStatus.Pending
    };

    private string BaseUrl = "/api/v1/servicerequests";
    private bool isLoading = false;
    private bool IsSaving = false;

    private async Task Create()
    {
        if (!await ValidateAsync())
            return;

        IsSaving = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", Model);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], Localizer[nameof(Resource.msg_CreateSuccessMessage)], SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task<bool> ValidateAsync()
    {
        if (Model.ContractClientId == Guid.Empty)
        {
            await _sweetAlert.FireAsync("Validacion", "Debe seleccionar un contrato activo.", SweetAlertIcon.Warning);
            return false;
        }

        if (Model.TechnicianId == Guid.Empty)
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

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
