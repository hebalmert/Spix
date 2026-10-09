using Spix.AppMaui.Services;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppMaui.ViewModels;

//Las tres pestanas son la misma lista con otro filtro, por eso solo cambian el filtro.
//Separadas y no un condicional adentro: cada una se lee sola.

// Lo de hoy y lo que quedo atras sin hacer: es lo que el tecnico tiene que resolver YA
public class TodayViewModel : VisitListViewModel
{
    public TodayViewModel(IRepository repository, ApiResponseHandler responseHandler)
        : base(repository, responseHandler)
    {
    }

    public override string Title => "Hoy";

    public override string EmptyText => "No tienes visitas para hoy";

    protected override bool Entra(TechVisitDto visita)
    {
        if (Cerrada(visita))
        {
            return false;
        }

        var fecha = visita.ScheduledAtUtc?.ToLocalTime().Date;

        //Sin fecha tambien entra aqui: alguien tiene que mirarla
        return fecha is null || fecha <= DateTime.Now.Date;
    }

    internal static bool Cerrada(TechVisitDto visita)
    {
        var estado = (ScheduleStatus)visita.Status;

        return estado == ScheduleStatus.Completed ||
               estado == ScheduleStatus.PhoneResolved ||
               estado == ScheduleStatus.Cancelled ||
               estado == ScheduleStatus.Rescheduled;
    }
}

// Lo que viene: de manana en adelante
public class UpcomingViewModel : VisitListViewModel
{
    public UpcomingViewModel(IRepository repository, ApiResponseHandler responseHandler)
        : base(repository, responseHandler)
    {
    }

    public override string Title => "Proximas";

    public override string EmptyText => "No tienes visitas programadas";

    protected override bool Entra(TechVisitDto visita)
    {
        if (TodayViewModel.Cerrada(visita))
        {
            return false;
        }

        var fecha = visita.ScheduledAtUtc?.ToLocalTime().Date;

        return fecha is not null && fecha > DateTime.Now.Date;
    }
}

// Lo que cerro hoy, para consultarlo sin llamar a la oficina
public class ClosedViewModel : VisitListViewModel
{
    public ClosedViewModel(IRepository repository, ApiResponseHandler responseHandler)
        : base(repository, responseHandler)
    {
    }

    public override string Title => "Cerradas";

    public override string EmptyText => "Todavia no has cerrado ninguna hoy";

    protected override bool Entra(TechVisitDto visita) => TodayViewModel.Cerrada(visita);
}
