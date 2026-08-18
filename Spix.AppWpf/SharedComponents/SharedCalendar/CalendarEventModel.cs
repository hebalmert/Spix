using Spix.DomainLogic.EnumTypes;

namespace Spix.AppWpf.SharedComponents.SharedCalendar;

// Representa el evento que FullCalendar muestra dentro del WebView2 de WPF.
public class CalendarEventModel
{
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public bool AllDay { get; set; }

    public ScheduleOrigin Origin { get; set; }

    public Guid? ServiceRequestId { get; set; }

    public string? Color { get; set; }

    public string? TextColor { get; set; }
}

// Entrega la fecha seleccionada por FullCalendar a la vista WPF que lo contiene.
public sealed class CalendarDateClickEventArgs : EventArgs
{
    public CalendarDateClickEventArgs(string date)
    {
        Date = date;
    }

    public string Date { get; }
}

// Entrega el identificador del evento seleccionado por FullCalendar a WPF.
public sealed class CalendarEventClickEventArgs : EventArgs
{
    public CalendarEventClickEventArgs(string eventId)
    {
        EventId = eventId;
    }

    public string EventId { get; }
}
