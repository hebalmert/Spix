namespace Spix.Domain.EntitiesSchedule;

//Quien levanto la solicitud
public enum ServiceRequestOrigin
{
    Office = 1,
    Client = 2,

    //Nace sola al aprobar el contrato: es la visita para instalar el servicio.
    //Va aqui y no en ScheduleStatus porque el estado dice en que punto va la visita
    //(pendiente, en progreso, completada) y esto dice de donde salio. Asi una
    //instalacion completada sigue siendo instalacion y se puede reportar.
    Installation = 3
}
