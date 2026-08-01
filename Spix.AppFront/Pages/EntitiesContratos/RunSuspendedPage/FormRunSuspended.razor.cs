using Microsoft.AspNetCore.Components;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;

namespace Spix.AppFront.Pages.EntitiesContratos.RunSuspendedPage;

public partial class FormRunSuspended
{
    [Parameter, EditorRequired] public RunSuspended RunSuspended { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public List<IntItemModel> Months { get; set; } = new();
    [Parameter] public bool IsSaving { get; set; }
    [Parameter] public bool IsReadOnly { get; set; }
    [Parameter] public bool ShowButtons { get; set; } = true;

    private void YearChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var yearNumber))
        {
            RunSuspended.YearNumber = yearNumber;
        }
    }

    private void MonthChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value) &&
            Enum.IsDefined(typeof(MonthType), value))
        {
            RunSuspended.MonthType = (MonthType)value;
        }
    }
}
