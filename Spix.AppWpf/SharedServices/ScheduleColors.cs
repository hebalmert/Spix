using Spix.Domain.EntitiesSchedule;
using System.Windows;
using System.Windows.Media;

namespace Spix.AppWpf.SharedServices;

// El color de cada estado de la agenda, igual que el ScheduleColors.cs de la web.
//
// Los valores NO se escriben aqui: viven en Colors.xaml como el resto de la paleta. Este
// helper solo dice que pincel le toca a cada estado, y lo entrega de las dos formas que
// hacen falta: como Brush para la leyenda de la pantalla, y como texto "#RRGGBB" para el
// calendario, que corre dentro del navegador y solo entiende CSS.
public static class ScheduleColors
{
    // La cita manual de la agenda puede no traer estatus: cae en el azul del sistema
    public static Brush Pincel(ScheduleStatus? estado)
    {
        return Recurso(Clave(estado));
    }

    public static Brush Pincel(int estado)
    {
        return Pincel((ScheduleStatus)estado);
    }

    // Lo que entiende el calendario del navegador
    public static string Hex(ScheduleStatus? estado)
    {
        return Hex(Pincel(estado));
    }

    private static string Clave(ScheduleStatus? estado)
    {
        return estado switch
        {
            ScheduleStatus.Requested => "BrushScheduleRequested",
            ScheduleStatus.PhoneResolved => "BrushSchedulePhoneResolved",
            ScheduleStatus.Pending => "BrushSchedulePending",
            ScheduleStatus.InProgress => "BrushScheduleInProgress",
            ScheduleStatus.OnHold => "BrushScheduleOnHold",
            ScheduleStatus.Rescheduled => "BrushScheduleRescheduled",
            ScheduleStatus.Completed => "BrushScheduleCompleted",
            ScheduleStatus.Cancelled => "BrushScheduleCancelled",
            _ => "BrushScheduleManual"
        };
    }

    private static Brush Recurso(string clave)
    {
        return Application.Current.TryFindResource(clave) as Brush ?? Brushes.Gray;
    }

    private static string Hex(Brush pincel)
    {
        if (pincel is not SolidColorBrush solido)
        {
            return "#16305E";
        }

        var color = solido.Color;

        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
