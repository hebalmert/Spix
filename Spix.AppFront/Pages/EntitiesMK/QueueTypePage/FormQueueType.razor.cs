using Microsoft.AspNetCore.Components;
using Spix.Domain.EntitiesMK;

namespace Spix.AppFront.Pages.EntitiesMK.QueueTypePage;

public partial class FormQueueType
{
    [Parameter, EditorRequired] public QueueType QueueType { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter] public bool IsSaving { get; set; }
}
