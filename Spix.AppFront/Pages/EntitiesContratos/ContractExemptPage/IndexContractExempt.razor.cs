using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractExemptPage;

//Contratos exonerados. Exonerar NO toca el MikroTik: el cliente sigue navegando,
//lo unico que cambia es el estado del contrato y que deja de cobrarsele.
public partial class IndexContractExempt
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private const string BaseUrl = "api/v1/contractexempt";

    private string Filter { get; set; } = string.Empty;
    private DateTime? Desde { get; set; }
    private DateTime? Hasta { get; set; }
    private bool SoloAbiertas { get; set; } = true;

    private ExemptListDTO? Data;
    private List<ExemptRecordDTO>? Records;

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

        var responseHttp = await _repository.GetAsync<ExemptListDTO>(url);
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

    //Exonerar: se elige un contrato activo y el backend hace el proceso completo
    private async Task ShowExemptAsync()
    {
        await _modalService.ShowAsync(typeof(CreateContractExempt), null, async result =>
        {
            if (result.Succeeded)
            {
                await LoadAsync();
                await _sweetAlert.FireAsync(
                    Localizer["Exempt_Title"],
                    Localizer["Exempt_Ok"],
                    SweetAlertIcon.Success);
            }
        });
    }

    private async Task ShowMotivoAsync(ExemptRecordDTO item)
    {
        await _sweetAlert.FireAsync(Localizer["Audit_Reason"], item.Motivo, SweetAlertIcon.Info);
    }

    //Rastro de la exoneracion: quien la registro, cuando y, si ya se retiro,
    //quien la cerro. Va en un boton para no gastar una columna de la tabla.
    private async Task ShowAuditAsync(ExemptRecordDTO item)
    {
        await AuditAlert.ShowAsync(_sweetAlert, Localizer["Audit_Title"],
            (Localizer["Audit_RegisteredBy"], item.UserByName),
            (Localizer["Audit_Date"], item.DateExempt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
            (Localizer["Audit_Reason"], item.Motivo),
            (Localizer["Audit_ReactivatedBy"], item.UserByNameEnded),
            (Localizer["Audit_ReactivatedDate"], item.DateEnded?.ToLocalTime().ToString("dd/MM/yyyy HH:mm")));
    }

    //Retirar la exoneracion: el contrato vuelve a Activo y el registro se cierra
    private async Task ActivateAsync(ExemptRecordDTO item)
    {
        //Se pregunta primero, igual que el borrado de los demas modulos: un clic por
        //error no puede sacar a alguien del beneficio sin que se note.
        var confirmation = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["Exempt_ActivateTitle"],
            Text = Localizer["Exempt_ActivateQuestion", item.ControlContrato, item.ClientName],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer["Exempt_ActivateButton"],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (confirmation.IsDismissed || confirmation.Value != "true")
            return;

        var responseHttp = await _repository.PostAsync($"{BaseUrl}/{item.ContractClientId}/activate", new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await LoadAsync();
        await _sweetAlert.FireAsync(
            Localizer["Exempt_ActivateTitle"],
            Localizer["Exempt_ActivateOk"],
            SweetAlertIcon.Success);
    }
}
