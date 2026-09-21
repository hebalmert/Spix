using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.ServiceRequestPage;

//Le pone tecnico y fecha a una solicitud que pidio el cliente. Recien ahi nace la cita.
public partial class AssignServiceRequest
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private const string BaseUrl = "/api/v1/servicerequests";

    [Parameter, EditorRequired] public Guid ServiceRequestId { get; set; }

    private List<GuidItemModel>? Technicians = new();
    private Guid TechnicianId;
    private DateTime ScheduledAt = DateTime.Now.AddDays(1);
    private bool IsSaving;

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>("/api/v1/combosData/ComboTechnicians");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Technicians = responseHttp.Response ?? new();
    }

    private void TechnicianChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var technicianId))
        {
            TechnicianId = technicianId;
        }
    }

    private void ScheduledChanged(ChangeEventArgs e)
    {
        if (DateTime.TryParse(e.Value?.ToString(), out var fecha))
        {
            ScheduledAt = fecha;
        }
    }

    private async Task SaveAsync()
    {
        if (TechnicianId == Guid.Empty)
        {
            await _sweetAlert.FireAsync(Localizer["Request_Assign"], "Debe seleccionar un tecnico activo.", SweetAlertIcon.Warning);
            return;
        }

        IsSaving = true;
        await InvokeAsync(StateHasChanged);

        //La fecha viaja en UTC, como la guarda el sistema
        var url = $"{BaseUrl}/{ServiceRequestId}/assign" +
                  $"?technicianId={TechnicianId}&scheduledAtUtc={ScheduledAt.ToUniversalTime():O}";

        var responseHttp = await _repository.PostAsync<object, ServiceRequestDto>(url, new { });

        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
