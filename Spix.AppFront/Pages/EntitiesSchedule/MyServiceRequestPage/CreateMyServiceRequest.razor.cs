using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.MyServiceRequestPage;

//El cliente solo elige contrato y describe el problema: el tecnico y la fecha los
//pone la oficina despues de llamarlo.
public partial class CreateMyServiceRequest
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private const string BaseUrl = "api/v1/myservicerequests";

    private List<MyContractItemDto>? Contracts;
    private Guid ContractClientId;
    private string? Reason;
    private bool IsSaving;

    //A que numero llamarlo: el del contrato, o uno nuevo que puede quedar guardado
    private bool UseNewPhone;
    private string? ContactPhone;
    private bool UpdateContractPhone = true;

    private MyContractItemDto? Selected => ContractClientId == Guid.Empty
        ? null
        : Contracts?.FirstOrDefault(x => x.ContractClientId == ContractClientId);

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<List<MyContractItemDto>>($"{BaseUrl}/contracts");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        //Arranca en el neutro que manda el backend: el cliente elige su contrato
        Contracts = responseHttp.Response ?? new();
    }

    private void ContractChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var contractClientId))
        {
            ContractClientId = contractClientId;

            //Cada contrato tiene su telefono: se vuelve al del contrato elegido
            UseContractPhone();
        }
    }

    private void UseContractPhone()
    {
        UseNewPhone = false;
        ContactPhone = null;
    }

    private void UseAnotherPhone()
    {
        UseNewPhone = true;
    }

    private void PhoneChanged(ChangeEventArgs e)
    {
        ContactPhone = e.Value?.ToString();
    }

    private void UpdatePhoneChanged(ChangeEventArgs e)
    {
        UpdateContractPhone = e.Value is bool valor && valor;
    }

    private void ReasonChanged(ChangeEventArgs e)
    {
        Reason = e.Value?.ToString();
    }

    private async Task SaveAsync()
    {
        if (ContractClientId == Guid.Empty)
        {
            await _sweetAlert.FireAsync(Localizer["MyRequest_New"], Localizer["MyRequest_NeedContract"], SweetAlertIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(Reason))
        {
            await _sweetAlert.FireAsync(Localizer["MyRequest_New"], Localizer["MyRequest_NeedProblem"], SweetAlertIcon.Warning);
            return;
        }

        if (UseNewPhone && string.IsNullOrWhiteSpace(ContactPhone))
        {
            await _sweetAlert.FireAsync(Localizer["MyRequest_New"], Localizer["Contact_NeedPhone"], SweetAlertIcon.Warning);
            return;
        }

        //DTO propio del portal: el cliente no manda tecnico, fecha ni estado
        var dto = new MyServiceRequestDto
        {
            ContractClientId = ContractClientId,
            ClientReason = Reason.Trim(),
            ContactPhone = UseNewPhone ? ContactPhone?.Trim() : null,
            UpdateContractPhone = UseNewPhone && UpdateContractPhone
        };

        IsSaving = true;
        await InvokeAsync(StateHasChanged);

        var responseHttp = await _repository.PostAsync(BaseUrl, dto);

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
