using Microsoft.AspNetCore.Components;

namespace Spix.AppFront.SharedUtility;

public partial class GenericSelectWrapper<Tmodel>
{
    [Parameter, EditorRequired] public IEnumerable<Tmodel>? Items { get; set; }
    [Parameter, EditorRequired] public RenderFragment Body { get; set; } = null!;

}