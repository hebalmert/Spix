using Microsoft.AspNetCore.Components;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;

namespace Spix.AppFront.Pages.EntitiesPayment.ContractExoneratedPage;

public partial class FormContractExonerated
{
    [Parameter, EditorRequired] public ContractExonerated ContractExonerated { get; set; } = null!;
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
        ContractExonerated.ContractClientId = contract.ContractClientId;
        ContractExonerated.ClientId = contract.ClientId;
        ContractExonerated.PlanId = contract.PlanId;
        ContractExonerated.TaxRate = contract.TaxRate ?? 0;
        ContractExonerated.UnitPrice = contract.PlanPrice ?? 0;
        ContractExonerated.PriceWithTax = contract.PlanPriceWithTax ?? 0;
        Contracts.Clear();
        await SelectedContractChanged.InvokeAsync(contract);
    }

    private void DateChanged(ChangeEventArgs e)
    {
        if (DateTime.TryParse(e.Value?.ToString(), out var date))
            ContractExonerated.DateExonerated = date;
    }

    private void YearChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
            ContractExonerated.YearNumber = value;
    }

    private void MonthChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value) && Enum.IsDefined(typeof(MonthType), value))
            ContractExonerated.MonthType = (MonthType)value;
    }
}
