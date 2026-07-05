using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesPayment.CxCBillPage;

public partial class PayCxCBill
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private CxCBill? Model;
    private CxCBillPaymentDto Payment = new();
    private bool isLoading;
    private bool IsSaving;
    private const string BaseUrl = "api/v1/cxcbills";
    private static readonly int[] DiscountOptions = [0, 25, 50, 75, 100];

    private decimal Debt => Model?.Balance ?? 0;
    private decimal DiscountAmount => Math.Round((Debt * Payment.DiscountPercent) / 100, 2);
    private decimal PaymentAmount => Debt - DiscountAmount;
    private decimal Balance => Debt - DiscountAmount - PaymentAmount;

    protected override async Task OnInitializedAsync()
    {
        Payment.CxCBillId = Id;
        Payment.PaymentMode = "Cash";
        await LoadModelAsync();
    }

    private async Task LoadModelAsync()
    {
        isLoading = true;
        var responseHttp = await _repository.GetAsync<CxCBill>($"{BaseUrl}/{Id}");
        isLoading = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Model = responseHttp.Response;
    }

    private void PaymentModeChanged(ChangeEventArgs e)
    {
        Payment.PaymentMode = e.Value?.ToString() ?? "Cash";
    }

    private void DiscountChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
            Payment.DiscountPercent = value;
    }

    private async Task PayAsync()
    {
        if (Payment.DiscountPercent > 0 && string.IsNullOrWhiteSpace(Payment.Detail))
        {
            await _sweetAlert.FireAsync("Validacion", "Debe especificar la razon del descuento.", SweetAlertIcon.Warning);
            return;
        }

        IsSaving = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/pay", Payment);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_CreateSuccessTitle)], "Pago registrado correctamente.", SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
