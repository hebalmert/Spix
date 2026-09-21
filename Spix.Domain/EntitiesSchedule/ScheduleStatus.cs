namespace Spix.Domain.EntitiesSchedule;

public enum ScheduleStatus
{
    Pending = 1,
    InProgress = 2,
    OnHold = 3,
    Rescheduled = 4,
    Completed = 5,
    Cancelled = 6,

    //La pidio el cliente desde su portal y la oficina todavia no la revisa:
    //no tiene tecnico ni fecha asignados.
    Requested = 7,

    //Se resolvio llamando al cliente, sin mandar a nadie
    PhoneResolved = 8
}
