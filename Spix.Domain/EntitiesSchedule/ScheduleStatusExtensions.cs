namespace Spix.Domain.EntitiesSchedule;

// Que significa que una solicitud este CERRADA, dicho en un solo sitio.
//
// Son dos estados, no uno: la que se completo en sitio y la que se resolvio por telefono.
// En las dos el trabajo ya quedo bien, asi que ninguna se modifica ni se borra.
//
// Antes cada servicio preguntaba "== Completed" por su cuenta, y por ahi se colaban las
// resueltas por telefono: se podian editar, cargarles servicios y fotos, y borrarlas.
public static class ScheduleStatusExtensions
{
    public static bool IsClosed(this ScheduleStatus status)
    {
        return status == ScheduleStatus.Completed ||
               status == ScheduleStatus.PhoneResolved;
    }

    // La cita manual de la agenda puede no traer estatus: eso no es una orden cerrada
    public static bool IsClosed(this ScheduleStatus? status)
    {
        return status.HasValue && status.Value.IsClosed();
    }
}
