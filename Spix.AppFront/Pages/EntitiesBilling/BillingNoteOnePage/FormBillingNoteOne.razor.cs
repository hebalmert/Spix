using Microsoft.AspNetCore.Components;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;

namespace Spix.AppFront.Pages.EntitiesBilling.BillingNoteOnePage;

public partial class FormBillingNoteOne
{
    [Parameter, EditorRequired] public BillingNoteOne BillingNoteOne { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public EventCallback<string> SearchContracts { get; set; }
    [Parameter] public List<IntItemModel>? Months { get; set; }
    [Parameter] public List<BillingContractDto> Contracts { get; set; } = new();
    [Parameter] public BillingContractDto? SelectedContract { get; set; }
    [Parameter] public EventCallback<BillingContractDto> SelectedContractChanged { get; set; }
    [Parameter] public bool IsSaving { get; set; }
    [Parameter] public bool IsReadOnly { get; set; }
    [Parameter] public bool ShowButtons { get; set; } = true;

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
        BillingNoteOne.ContractClientId = contract.ContractClientId;
        BillingNoteOne.ClientId = contract.ClientId;
        Contracts.Clear();
        await SelectedContractChanged.InvokeAsync(contract);
    }

    private void DateChanged(ChangeEventArgs e)
    {
        if (!DateTime.TryParse(e.Value?.ToString(), out var date))
            return;

        BillingNoteOne.DateBill = date;
        BillingNoteOne.YearNumber = date.Year;
        BillingNoteOne.MonthType = (MonthType)date.Month;
    }

    private void YearChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
            BillingNoteOne.YearNumber = value;
    }

    private void MonthChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value) && Enum.IsDefined(typeof(MonthType), value))
            BillingNoteOne.MonthType = (MonthType)value;
    }
}
