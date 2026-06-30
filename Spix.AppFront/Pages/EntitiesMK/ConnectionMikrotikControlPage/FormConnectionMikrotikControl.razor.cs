using Microsoft.AspNetCore.Components;
using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppFront.Pages.EntitiesMK.ConnectionMikrotikControlPage;

public partial class FormConnectionMikrotikControl
{
    [Parameter, EditorRequired] public ConnectionMikrotikControl ConnectionMikrotikControl { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private IEnumerable<MikrotikControlType> ControlTypes => Enum.GetValues<MikrotikControlType>();
}
