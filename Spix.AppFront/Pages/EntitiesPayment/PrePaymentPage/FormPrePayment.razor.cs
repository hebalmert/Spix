using Microsoft.AspNetCore.Components;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;

namespace Spix.AppFront.Pages.EntitiesPayment.PrePaymentPage;

public partial class FormPrePayment
{
    [Parameter, EditorRequired] public PrePayment PrePayment { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public EventCallback<string> SearchContracts { get; set; }
    [Parameter] public List<IntItemModel>? Months { get; set; }
    [Parameter] public List<BillingContractDto> Contracts { get; set; } = new();
    [Parameter] public BillingContractDto? SelectedContract { get; set; }
    [Parameter] public EventCallback<BillingContractDto> SelectedContractChanged { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private string ContractFilter { get; set; } = string.Empty;

    protected override void OnParametersSet()
    {
        if (SelectedContract is not null && string.IsNullOrWhiteSpace(ContractFilter))
            ContractFilter = SelectedContract.ClientFullName;
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
