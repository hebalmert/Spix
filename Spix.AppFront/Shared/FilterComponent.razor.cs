using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Regix.Domain.Resources;

namespace Regix.AppFront.Shared;

public partial class FilterComponent
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Parameter] public string FilterValue { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ApplyFilter { get; set; }

    private async Task ClearFilter()
    {
        FilterValue = string.Empty;
        await ApplyFilter.InvokeAsync(FilterValue);
    }

    private async Task OnfilterApply()
    {
        await ApplyFilter.InvokeAsync(FilterValue);
    }
}