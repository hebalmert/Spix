using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesPayment.ContractorPaymentPage;

public partial class PayContractorPayment
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    [Parameter] public Guid ContractorId { get; set; }
    [Parameter] public string ContractorName { get; set; } = string.Empty;
    [Parameter] public List<Guid> ContractorAccountPayableIds { get; set; } = new();
    [Parameter] public decimal Total { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/contractor-payments";
    private ContractorPaymentCreateDto Payment = new();
    private bool IsSaving;

    protected override void OnInitialized()
    {
        Payment.ContractorId = ContractorId;
        Payment.ContractorAccountPayableIds = ContractorAccountPayableIds.ToList();
        Payment.PaymentMode = "Cash";
    }

    private void PaymentModeChanged(ChangeEventArgs args)
    {
        Payment.PaymentMode = args.Value?.ToString() ?? "Cash";
    }

    private async Task PayAsync()
    {
        IsSaving = true;
        try
        {
            var responseHttp = await _repository.PostAsync<ContractorPaymentCreateDto, ContractorPayment>($"{BaseUrl}/pay", Payment);
            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                return;
            }

            var paymentNumber = responseHttp.Response?.PaymentNumber ?? string.Empty;
            var message = string.IsNullOrWhiteSpace(paymentNumber)
                ? "Pago al contratista registrado correctamente."
                : $"Pago al contratista registrado correctamente. Comprobante: {paymentNumber}.";

            await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], message, SweetAlertIcon.Success);
            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
