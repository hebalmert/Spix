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
                backgroundColor: e.color,
                borderColor: e.color
            })),

            dateClick: function (info) {
                dotnetRef.invokeMethodAsync('OnDateClick', info.dateStr);
            },

            eventClick: function (info) {
                dotnetRef.invokeMethodAsync('OnEventClick', info.event.id);
            },

            eventDidMount: function (info) {
                info.el.setAttribute("title", info.event.title);
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
                    backgroundColor: e.color,
                    borderColor: e.color
                }))
            );
        }
    }
};

