using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesPayment.CxCBillPage;

public partial class CancelCxCBill
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private CxCBillCancelDto Model = new();
    private bool IsSaving;
    private const string BaseUrl = "api/v1/cxcbills";

    protected override void OnInitialized()
    {
        Model.CxCBillId = Id;
    }

    private async Task CancelAsync()
    {
        if (string.IsNullOrWhiteSpace(Model.DescriptionCancelled))
        {
            await _sweetAlert.FireAsync("Validacion", "Debe especificar el motivo de anulacion.", SweetAlertIcon.Warning);
            return;
        }

        IsSaving = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/cancel", Model);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Anulado", "Cuenta por cobrar anulada correctamente.", SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
