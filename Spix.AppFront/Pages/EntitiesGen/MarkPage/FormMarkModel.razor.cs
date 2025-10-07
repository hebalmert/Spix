using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.Domain.EntitiesGen;
using Spix.Domain.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.MarkPage;

public partial class FormMarkModel
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Parameter, EditorRequired] public MarkModel MarkModel { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
}