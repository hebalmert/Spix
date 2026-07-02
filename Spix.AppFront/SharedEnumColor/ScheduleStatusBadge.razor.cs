using Microsoft.AspNetCore.Components;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.Domain.EntitiesSchedule;

namespace Spix.AppFront.SharedEnumColor;

public partial class ScheduleStatusBadge
{
    [Inject] private IEnumMultilLanguageService EnumMultilLanguageService { get; set; } = null!;

    [Parameter] public ScheduleStatus Value { get; set; }

    protected string Text => EnumMultilLanguageService.GetLocalizedName(Value);

    protected string Color => Value switch
    {
        ScheduleStatus.Pending => "#FD7E14",
        ScheduleStatus.InProgress => "#6F42C1",
        ScheduleStatus.OnHold => "#E67700",
        ScheduleStatus.Rescheduled => "#0D6EFD",
        ScheduleStatus.Completed => "#6B8E23",
        ScheduleStatus.Cancelled => "#B02A37",
        _ => "#6C757D"
    };
}
