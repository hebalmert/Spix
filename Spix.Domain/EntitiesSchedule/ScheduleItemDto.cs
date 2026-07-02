using Spix.DomainLogic.EnumTypes;

namespace Spix.Domain.EntitiesSchedule;

public class ScheduleItemDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }

    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public bool IsAllDay { get; set; }

    public Guid TechnicianId { get; set; }
    public string? TechnicianName { get; set; }

    public bool IsRecurring { get; set; }
    public string? RecurrenceRule { get; set; }

    public ScheduleStatus? ScheduleStatus { get; set; }

    public ScheduleOrigin Origin { get; set; }

    public Guid? ServiceRequestId { get; set; }
}
