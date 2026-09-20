using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Net;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractSuspendedPage;

public partial class IndexContractSuspended
{
    private const string HotSpotActivationRequirementsMessage = "MikroTik Hotspot";

    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private const string BaseUrl = "api/v1/contractsuspended";

    private string Filter { get; set; } = string.Empty;
    private DateTime? Desde { get; set; }
    private DateTime? Hasta { get; set; }
    private bool SoloAbiertas { get; set; } = true;

    private SuspendedListDTO? Data;
    private List<SuspendedRecordDTO>? Records;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var url = $"{BaseUrl}/records?soloAbiertas={SoloAbiertas}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Uri.EscapeDataString(Filter)}";
        }
        if (Desde.HasValue)
        {
            url += $"&desde={Desde.Value:yyyy-MM-dd}";
        }
        if (Hasta.HasValue)
        {
            url += $"&hasta={Hasta.Value:yyyy-MM-dd}";
        }

        var responseHttp = await _repository.GetAsync<SuspendedListDTO>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Data = responseHttp.Response;
        Records = Data?.Records ?? new();

        await InvokeAsync(StateHasChanged);
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await LoadAsync();
    }

    private async Task DesdeChanged(ChangeEventArgs e)
    {
        Desde = DateTime.TryParse(e.Value?.ToString(), out var fecha) ? fecha : null;
        await LoadAsync();
    }

    private async Task HastaChanged(ChangeEventArgs e)
    {
        Hasta = DateTime.TryParse(e.Value?.ToString(), out var fecha) ? fecha : null;
        await LoadAsync();
    }

    private async Task SoloAbiertasChanged(ChangeEventArgs e)
    {
        SoloAbiertas = bool.TryParse(e.Value?.ToString(), out var valor) && valor;
        await LoadAsync();
    }

    //Suspender: se elige un contrato activo y el backend hace todo el proceso
    private async Task ShowSuspendAsync()
    {
        await _modalService.ShowAsync(typeof(CreateContractSuspended), null, async result =>
        {
            if (result.Succeeded)
            {
                await LoadAsync();
                await _sweetAlert.FireAsync("Suspension", "El contrato quedo suspendido.", SweetAlertIcon.Success);
            }
        });
    }

    private async Task ShowMotivoAsync(SuspendedRecordDTO item)
    {
        await _sweetAlert.FireAsync(Localizer["Audit_Reason"], item.Motivo, SweetAlertIcon.Info);
    }

    //Rastro de la suspension: quien la registro, cuando, de donde vino y, si ya se
    //reactivo, quien la cerro. Va en un boton para no gastar una columna de la tabla.
    private async Task ShowAuditAsync(SuspendedRecordDTO item)
    {
        await AuditAlert.ShowAsync(_sweetAlert, Localizer["Audit_Title"],
            (Localizer["Audit_RegisteredBy"], item.UserByName),
            (Localizer["Audit_Date"], item.DateSuspended.ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
            (Localizer["Audit_Origin"], item.Origin == SuspendedOrigin.Corte ? "Corte" : "Manual"),
            (Localizer["Audit_Reason"], item.Motivo),
            (Localizer["Audit_ReactivatedBy"], item.UserByNameReactivated),
            (Localizer["Audit_ReactivatedDate"], item.DateReactivated?.ToLocalTime().ToString("dd/MM/yyyy HH:mm")));
    }

    //Reactivar: MikroTik a bypassed, contrato a Activo y se cierra el registro
    private async Task ActivateAsync(SuspendedRecordDTO item)
    {
        //Se pregunta antes de tocar el equipo, igual que el borrado de los demas modulos:
        //un clic por error no puede devolverle el servicio a un cliente suspendido.
        var confirmation = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["Suspend_ActivateTitle"],
            Text = Localizer["Suspend_ActivateQuestion", item.ControlContrato, item.ClientName],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer["Suspend_ActivateButton"],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirmation.IsDismissed || confirmation.Value != "true")
            return;

        var responseHttp = await _repository.PostAsync($"{BaseUrl}/{item.ContractClientId}/activate", new { });

        if (responseHttp.HttpResponseMessage?.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorMessage = await responseHttp.GetErrorMessageAsync();
            var mikrotikConnectionMessage = Localizer[nameof(Resource.Mikrotik_Connection_Error)].Value;

            if (string.Equals(errorMessage, mikrotikConnectionMessage, StringComparison.OrdinalIgnoreCase))
            {
                await _sweetAlert.FireAsync(
                    "No se pudo conectar con MikroTik",
                    "No fue posible activar el acceso remoto. El contrato continuara suspendido.",
                    SweetAlertIcon.Warning);
                return;
            }

            if (errorMessage?.Contains(HotSpotActivationRequirementsMessage, StringComparison.OrdinalIgnoreCase) == true)
            {
                await _sweetAlert.FireAsync(
                    "Contrato pendiente de configuracion MikroTik",
                    "La corporacion maneja MikroTik HotSpot y el contrato aun no tiene Contract Queue e IpBinding. No puede retirarse de Suspendidos ni pasar a Active.",
                    SweetAlertIcon.Warning);
                return;
            }
        }

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await LoadAsync();
        await _sweetAlert.FireAsync(
            Localizer["Suspend_ActivateTitle"],
            Localizer["Suspend_ActivateOk"],
            SweetAlertIcon.Success);
    }
}
