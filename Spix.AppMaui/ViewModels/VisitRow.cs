using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppMaui.ViewModels;

//Una fila ya lista para pintar: la pagina no calcula la hora, ni el color, ni si se
//puede llamar. Misma idea que ServiceRequestRow de la web y del escritorio.
public class VisitRow
{
    public TechVisitDto Item { get; }

    public string ClientName => Item.ClientFullName;

    public string Meta => $"#{Item.RequestNumber} - {Item.OriginName}";

    public string? Address => Item.Address;

    public string WhenText { get; }

    public Color WhenBack { get; }

    public Color WhenText2 { get; }

    public bool CanCall => !string.IsNullOrWhiteSpace(Item.ContactPhone);

    public bool CanMap => Item.ContractLatitude is not null && Item.ContractLongitude is not null;

    public VisitRow(TechVisitDto item)
    {
        Item = item;

        var estado = (ScheduleStatus)item.Status;
        var cerrada = estado == ScheduleStatus.Completed ||
                      estado == ScheduleStatus.PhoneResolved ||
                      estado == ScheduleStatus.Cancelled;

        if (cerrada)
        {
            WhenText = item.StatusText ?? "Cerrada";
            WhenBack = Color("SpixOkBack");
            WhenText2 = Color("SpixOkText");
            return;
        }

        if (estado == ScheduleStatus.InProgress)
        {
            WhenText = "En sitio";
            WhenBack = Color("SpixInfoBack");
            WhenText2 = Color("SpixInfoText");
            return;
        }

        var fecha = item.ScheduledAtUtc?.ToLocalTime();
        if (fecha is null)
        {
            WhenText = "Sin fecha";
            WhenBack = Color("SpixNoneBack");
            WhenText2 = Color("SpixNoneText");
            return;
        }

        var dias = (fecha.Value.Date - DateTime.Now.Date).Days;

        //Vencida en rojo: en la calle eso es lo primero que hay que ver
        if (dias < 0)
        {
            WhenText = $"Vencida {Math.Abs(dias)}d";
            WhenBack = Color("SpixFarBack");
            WhenText2 = Color("SpixFarText");
            return;
        }

        WhenText = dias == 0 ? $"Hoy {fecha:HH:mm}"
            : dias == 1 ? $"Manana {fecha:HH:mm}"
            : fecha.Value.ToString("dd/MM HH:mm");

        WhenBack = dias == 0 ? Color("SpixInfoBack") : Color("SpixNoneBack");
        WhenText2 = dias == 0 ? Color("SpixInfoText") : Color("SpixNoneText");
    }

    private static Color Color(string clave)
    {
        if (Application.Current?.Resources.TryGetValue(clave, out var valor) == true && valor is Color color)
        {
            return color;
        }

        return Colors.Gray;
    }
}
