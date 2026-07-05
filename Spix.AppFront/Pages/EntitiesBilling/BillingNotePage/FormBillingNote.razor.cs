using Microsoft.AspNetCore.Components;
using Spix.Domain.EntitiesBilling;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;

namespace Spix.AppFront.Pages.EntitiesBilling.BillingNotePage;

public partial class FormBillingNote
{
    [Parameter, EditorRequired] public BillingNote BillingNote { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter] public List<IntItemModel>? Months { get; set; }
    [Parameter] public bool IsSaving { get; set; }
    [Parameter] public bool IsReadOnly { get; set; }
    [Parameter] public bool ShowButtons { get; set; } = true;

    private void DateChanged(ChangeEventArgs e)
    {
        if (!DateTime.TryParse(e.Value?.ToString(), out var date))
            return;

        BillingNote.DateBill = date;
        BillingNote.YearNumber = date.Year;
        BillingNote.MonthType = (MonthType)date.Month;
    }

    private void YearChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
            BillingNote.YearNumber = value;
    }

    private void MonthChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value) && Enum.IsDefined(typeof(MonthType), value))
            BillingNote.MonthType = (MonthType)value;
    }
}
