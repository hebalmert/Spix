using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Globalization;

namespace Spix.AppFront.Pages.EntitiesPayment.ContractorPaymentPage;

//El pago al contratista contra su cuenta: completo o por partes
public partial class PayCxCContractor
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/contractor-payments";

    //Los modos que acepta el backend, con su icono
    private static readonly (string Key, string Icon, string Label)[] PaymentModes =
    [
        ("Cash", "fa fa-money-bill-wave", "Pay_Cash"),
        ("Card", "fa fa-credit-card", "Pay_Card"),
        ("Transfer", "fa fa-building-columns", "Pay_Transfer")
    ];

    private CxCContractor? Model;
    private decimal Payment;
    private string PaymentMode = "Cash";
    private string? Reference;
    private string? Detail;
    private bool IsLoading;
    private bool IsSaving;

    //Lo que quedaria debiendo despues de este pago
    private decimal Rest => (Model?.Balance ?? 0) - Payment;

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        var responseHttp = await _repository.GetAsync<CxCContractor>($"{BaseUrl}/cxc/{Id}");
        IsLoading = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        Model = responseHttp.Response;

        //Se propone el saldo completo: lo normal es pagarle todo
        Payment = Model?.Balance ?? 0;
    }

    private void PaymentChanged(ChangeEventArgs e)
    {
        var texto = e.Value?.ToString();
        if (!decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out var valor))
        {
            Payment = 0;
            return;
        }

        //Nunca mas de lo que se le debe
        Payment = Math.Clamp(Math.Round(valor, 2), 0, Model?.Balance ?? 0);
    }

    private void PayAll()
    {
        Payment = Model?.Balance ?? 0;
    }

    private void SetPaymentMode(string mode)
    {
        PaymentMode = mode;
    }

    private async Task PayAsync()
    {
        if (Model is null || IsSaving)
            return;

        if (Payment <= 0)
        {
            await _sweetAlert.FireAsync(Localizer["CxCContractor_Pay"], Localizer["Contractor_PaymentInvalid"], SweetAlertIcon.Warning);
            return;
        }

        var model = new CxCContractorPaymentDto
        {
            CxCContractorId = Model.CxCContractorId,
            Payment = Payment,
            PaymentMode = PaymentMode,
            Reference = Reference,
            Detail = Detail
        };

        IsSaving = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/cxc/pay", model);
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
