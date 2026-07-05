window.SpixCalendar = {
    calendars: {},

    init: function (elementId, dotnetRef, events) {
        var calendarEl = document.getElementById(elementId);

        var calendar = new FullCalendar.Calendar(calendarEl, {
            themeSystem: 'standard',

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

            events: events.map(e => ({
                ...e,
                display: e.color ? 'block' : undefined,
                backgroundColor: e.color,
                borderColor: e.color,
                textColor: e.textColor
            })),

            dateClick: function (info) {
                dotnetRef.invokeMethodAsync('OnDateClick', info.dateStr);
            },

            eventClick: function (info) {
                dotnetRef.invokeMethodAsync('OnEventClick', info.event.id);
            },

            eventDidMount: function (info) {
                info.el.setAttribute("title", info.event.title);
                window.SpixCalendar.applyEventStyle(info);
            }
        });

        calendar.render();

        // 🔥 Guardamos la instancia para refrescar después
        window.SpixCalendar.calendars[elementId] = calendar;
    },

    refreshEvents: function (elementId, events) {
        var calendar = window.SpixCalendar.calendars[elementId];

        if (calendar) {
            calendar.removeAllEvents();
            calendar.addEventSource(
                events.map(e => ({
                    ...e,
                    display: e.color ? 'block' : undefined,
                    backgroundColor: e.color,
                    borderColor: e.color,
                    textColor: e.textColor
                }))
            );
        }
    },

    applyEventStyle: function (info) {
        var backgroundColor = info.event.backgroundColor;
        var textColor = info.event.textColor;

        if (!backgroundColor || !textColor) {
            return;
        }

        info.el.style.backgroundColor = backgroundColor;
        info.el.style.borderColor = backgroundColor;
        info.el.style.color = textColor;

        info.el.querySelectorAll("a, td, .fc-event-title, .fc-event-time, .fc-list-event-title, .fc-list-event-time")
            .forEach(function (item) {
                item.style.backgroundColor = backgroundColor;
                item.style.borderColor = backgroundColor;
                item.style.color = textColor;
            });

        var dot = info.el.querySelector(".fc-daygrid-event-dot, .fc-list-event-dot");
        if (dot) {
            dot.style.borderColor = textColor;
        }
    }
};

