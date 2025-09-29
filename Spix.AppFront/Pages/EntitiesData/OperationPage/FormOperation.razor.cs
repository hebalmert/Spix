using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.Domain.EntitiesData;
using Spix.Domain.Resources;

namespace Spix.AppFront.Pages.EntitiesData.OperationPage;

public partial class FormOperation
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Parameter, EditorRequired] public Operation Operation { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
}