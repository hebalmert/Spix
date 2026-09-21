using Spix.Domain.EntitiesSchedule;

namespace Spix.AppFront.Helper;

//El color de cada estatus de la agenda, en un solo lugar: lo usan el calendario y su leyenda.
//Es la misma paleta del ScheduleStatusBadge, para que la lista y el calendario se lean igual.
public static class ScheduleColors
{
    //La cita manual de la agenda puede no traer estatus: cae en el azul del sistema
    public static string Get(ScheduleStatus? status) => status switch
    {
        ScheduleStatus.Requested => "#0DCAF0",
        ScheduleStatus.PhoneResolved => "#20C997",
        ScheduleStatus.Pending => "#FD7E14",
        ScheduleStatus.InProgress => "#6F42C1",
        ScheduleStatus.OnHold => "#E67700",
        ScheduleStatus.Rescheduled => "#0D6EFD",
        ScheduleStatus.Completed => "#6B8E23",
        ScheduleStatus.Cancelled => "#B02A37",
        _ => "#16305E"
    };

    //Para la leyenda, que recibe el valor del combo que arma el backend
    public static string Get(int status) => Get((ScheduleStatus)status);
}
