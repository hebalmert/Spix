using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Shared;

public partial class GenericList<Titem>
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Parameter] public RenderFragment? Loading { get; set; }

    [Parameter] public RenderFragment? NoRecords { get; set; }

    [Parameter, EditorRequired] public List<Titem> MyList { get; set; } = null!;

    [Parameter, EditorRequired] public RenderFragment Body { get; set; } = null!;
}