using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.Domain.EntitiesData;
using Spix.Domain.Resources;

namespace Spix.AppFront.Pages.EntitiesData.ChannelPage;

public partial class FormChannel
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Parameter, EditorRequired] public Channel Channel { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
}