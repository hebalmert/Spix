using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents.SharedCalendar;

// Aloja FullCalendar en WebView2 y comunica sus clics a las vistas WPF.
public partial class SharedFullCalendarView : UserControl
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private IReadOnlyCollection<CalendarEventModel> _events = Array.Empty<CalendarEventModel>();
    private bool _isBrowserReady;

    public SharedFullCalendarView()
    {
        InitializeComponent();
        Loaded += CalendarLoaded;
    }

    public event EventHandler<CalendarDateClickEventArgs>? DateClicked;

    public event EventHandler<CalendarEventClickEventArgs>? EventClicked;

    // Actualiza los eventos sin recrear la vista WPF que contiene el calendario.
    public async Task SetEventsAsync(IEnumerable<CalendarEventModel> events)
    {
        _events = events.ToList();

        if (_isBrowserReady)
        {
            await RenderCalendarAsync();
        }
    }

    private async void CalendarLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await CalendarBrowser.EnsureCoreWebView2Async();
            CalendarBrowser.CoreWebView2.WebMessageReceived += CalendarMessageReceived;
            _isBrowserReady = true;
            await RenderCalendarAsync();
        }
        catch
        {
            CalendarErrorText.Text = "No fue posible iniciar el calendario.";
            CalendarErrorText.Visibility = Visibility.Visible;
        }
    }

    private Task RenderCalendarAsync()
    {
        if (!_isBrowserReady)
        {
            return Task.CompletedTask;
        }

        CalendarErrorText.Visibility = Visibility.Collapsed;
        CalendarBrowser.CoreWebView2.NavigateToString(CreateCalendarDocument());
        return Task.CompletedTask;
    }

    // Recibe los mensajes controlados de fecha o evento enviados desde el documento HTML.
    private void CalendarMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            CalendarMessage? message = JsonSerializer.Deserialize<CalendarMessage>(
                e.TryGetWebMessageAsString(),
                _jsonOptions);

            if (message == null || string.IsNullOrWhiteSpace(message.Type))
            {
                return;
            }

            if (message.Type == "dateClick" && !string.IsNullOrWhiteSpace(message.Date))
            {
                DateClicked?.Invoke(this, new CalendarDateClickEventArgs(message.Date));
                return;
            }

            if (message.Type == "eventClick" && !string.IsNullOrWhiteSpace(message.EventId))
            {
                EventClicked?.Invoke(this, new CalendarEventClickEventArgs(message.EventId));
            }
        }
        catch (JsonException)
        {
            // Ignora mensajes que no pertenecen al calendario embebido.
        }
    }

    private string CreateCalendarDocument()
    {
        string eventsJson = JsonSerializer.Serialize(_events, _jsonOptions);

        return $$"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.css" />
                <style>
                    html, body, #calendar { width: 100%; height: 100%; margin: 0; }
                    body { font-family: "Segoe UI", Arial, sans-serif; background: #f7f9fc; color: #1e2b3c; }
                    #calendar { box-sizing: border-box; padding: 18px; }
                    .fc .fc-toolbar-title { font-size: 1.2rem; color: #102a4c; }
                    .fc .fc-button-primary { background: #14345e; border-color: #14345e; }
                    .fc .fc-button-primary:hover, .fc .fc-button-primary:not(:disabled).fc-button-active { background: #0d2442; border-color: #0d2442; }
                    .fc .fc-daygrid-day.fc-day-today, .fc .fc-timegrid-col.fc-day-today { background: #e7f0ff; }
                    .fc .fc-event { border-radius: 4px; padding: 2px 4px; cursor: pointer; }
                    .fc .fc-scrollgrid { border-color: #cbd5e1; }
                    .fc .fc-col-header-cell { background: #eef3fa; }
                </style>
            </head>
            <body>
                <div id="calendar"></div>
                <script src="https://cdn.jsdelivr.net/npm/fullcalendar@6.1.8/index.global.min.js"></script>
                <script>
                    const events = {{eventsJson}};
                    const calendarElement = document.getElementById('calendar');
                    const calendar = new FullCalendar.Calendar(calendarElement, {
                        themeSystem: 'standard',
                        timeZone: 'local',
                        headerToolbar: {
                            left: 'prev,next today',
                            center: 'title',
                            right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
                        },
                        initialView: 'dayGridMonth',
                        navLinks: true,
                        editable: false,
                        selectable: true,
                        nowIndicator: true,
                        weekNumbers: true,
                        dayMaxEvents: true,
                        events: events.map(event => ({
                            ...event,
                            display: event.color ? 'block' : undefined,
                            backgroundColor: event.color,
                            borderColor: event.color,
                            textColor: event.textColor
                        })),
                        dateClick: function(info) {
                            window.chrome.webview.postMessage(JSON.stringify({ type: 'dateClick', date: info.dateStr }));
                        },
                        eventClick: function(info) {
                            window.chrome.webview.postMessage(JSON.stringify({ type: 'eventClick', eventId: info.event.id }));
                        },
                        eventDidMount: function(info) {
                            info.el.setAttribute('title', info.event.title);
                        }
                    });
                    calendar.render();
                </script>
            </body>
            </html>
            """;
    }

    // Modela el contrato minimo usado por los mensajes enviados desde JavaScript.
    private sealed class CalendarMessage
    {
        public string? Type { get; set; }

        public string? Date { get; set; }

        public string? EventId { get; set; }
    }
}
