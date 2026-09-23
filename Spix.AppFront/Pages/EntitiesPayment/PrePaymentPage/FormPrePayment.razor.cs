using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.xLanguage.Resources;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;

namespace Spix.AppFront.Pages.EntitiesPayment.PrePaymentPage;

public partial class FormPrePayment
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    [Parameter, EditorRequired] public PrePayment PrePayment { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public EventCallback<string> SearchContracts { get; set; }
    [Parameter] public List<IntItemModel>? Months { get; set; }
    [Parameter] public List<BillingContractDto> Contracts { get; set; } = new();
    [Parameter] public BillingContractDto? SelectedContract { get; set; }
    [Parameter] public EventCallback<BillingContractDto> SelectedContractChanged { get; set; }
    [Parameter] public bool IsSaving { get; set; }
    [Parameter] public bool IsEditControl { get; set; }

    //Los servicios que puede adelantar el contrato elegido; los carga la pantalla padre
    [Parameter] public List<PrePaymentServiceDto>? Services { get; set; }

    private string ContractFilter { get; set; } = string.Empty;

    //Lo que va marcado: se guarda en las lineas del pago, y el backend rearma los precios
    private HashSet<Guid> SelectedServices = new();

    private decimal PlanTotal => SelectedContract?.PlanPriceWithTax ?? 0;

    private decimal ServicesTotal => Services is null
        ? 0
        : Services.Where(x => SelectedServices.Contains(x.ServiceRequestDetailId)).Sum(x => x.Total);

    protected override void OnParametersSet()
    {
        if (SelectedContract is not null && string.IsNullOrWhiteSpace(ContractFilter))
            ContractFilter = SelectedContract.ClientFullName;

        //Al editar, vienen marcados los servicios que ya tenia el pago
        if (SelectedServices.Count == 0 && PrePayment.PrePaymentDetails is not null)
        {
            SelectedServices = PrePayment.PrePaymentDetails
                .Where(x => x.ServiceRequestDetailId.HasValue)
                .Select(x => x.ServiceRequestDetailId!.Value)
                .ToHashSet();
        }
    }

    private bool IsSelected(Guid serviceRequestDetailId) => SelectedServices.Contains(serviceRequestDetailId);

    //Marcar o desmarcar un servicio: solo se guardan sus ids, los valores los pone el backend
    private void ToggleService(PrePaymentServiceDto service, ChangeEventArgs e)
    {
        var isChecked = e.Value is bool value && value;

        if (isChecked)
            SelectedServices.Add(service.ServiceRequestDetailId);
        else
            SelectedServices.Remove(service.ServiceRequestDetailId);

        ApplyLines();
    }

    private void ApplyLines()
    {
        PrePayment.PrePaymentDetails = SelectedServices
            .Select(id => new PrePaymentDetail { ServiceRequestDetailId = id })
            .ToList();
    }

    private async Task FilterChanged(ChangeEventArgs e)
    {
        ContractFilter = e.Value?.ToString() ?? string.Empty;
        await SearchContracts.InvokeAsync(ContractFilter);
    }

    private async Task SelectContract(BillingContractDto contract)
    {
        SelectedContract = contract;
        ContractFilter = contract.ClientFullName;
        PrePayment.ContractClientId = contract.ContractClientId;
        PrePayment.ClientId = contract.ClientId;
        PrePayment.PlanId = contract.PlanId;
        PrePayment.TaxRate = contract.TaxRate ?? 0;
        PrePayment.UnitPrice = contract.PlanPrice ?? 0;
        PrePayment.PriceWithTax = contract.PlanPriceWithTax ?? 0;

        //Otro contrato, otros servicios
        SelectedServices.Clear();
        ApplyLines();
        Contracts.Clear();
        await SelectedContractChanged.InvokeAsync(contract);
    }

    private void DateChanged(ChangeEventArgs e)
    {
        if (DateTime.TryParse(e.Value?.ToString(), out var date))
            PrePayment.DatePayment = date;
    }

    private void YearChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
            PrePayment.YearNumber = value;
    }

    private void MonthChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value) && Enum.IsDefined(typeof(MonthType), value))
            PrePayment.MonthType = (MonthType)value;
    }
}
