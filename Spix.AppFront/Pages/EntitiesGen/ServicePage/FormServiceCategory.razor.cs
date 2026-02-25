using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.Domain.EntitiesGen;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.ServicePage;

public partial class FormServiceCategory
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Parameter, EditorRequired] public ServiceCategory ServiceCategory { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }

}