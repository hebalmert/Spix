using Spix.DomainLogic.EnumTypes;
using System.Windows.Media;
using TransferEntity = Spix.Domain.EntitiesInven.Transfer;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Transfer;

// Los tres hechos del historial, ya armados. Se calculan una vez al abrir la ventana:
// no hay nada que el usuario pueda cambiar aqui, asi que no hacen falta notificaciones.
//
// El nombre y la fecha van SEPARADOS, como en la web: el nombre grande arriba y la
// fecha chica debajo.
public class AuditTransferRow
{
    public AuditTransferRow(TransferEntity transfer)
    {
        Creador = Quien(transfer.NombreUsuario);
        FechaCreado = Cuando(transfer.DateCreated);

        var cerrado = transfer.Status == TransferType.Completado;

        Cerrador = cerrado ? Quien(transfer.NombreUsuarioCierre) : "Sin cerrar";
        FechaCerrado = cerrado ? Cuando(transfer.DateClosed) : string.Empty;

        Recibe = Quien(transfer.ReceivedByName);

        //Verde si ya cerro, ambar si todavia esta abierto
        AcentoCierre = cerrado
            ? new SolidColorBrush(Color.FromRgb(25, 135, 84))
            : new SolidColorBrush(Color.FromRgb(245, 159, 0));
    }

    public string Creador { get; }

    public string FechaCreado { get; }

    public string Cerrador { get; }

    public string FechaCerrado { get; }

    public string Recibe { get; }

    public Brush AcentoCierre { get; }

    private static string Quien(string? nombre)
    {
        return string.IsNullOrWhiteSpace(nombre) ? "-" : nombre;
    }

    //Sin fecha no se escribe nada: el renglon queda vacio en vez de con un guion raro
    private static string Cuando(DateTime? fecha)
    {
        return fecha is null ? string.Empty : fecha.Value.ToString("dd/MM/yyyy HH:mm");
    }
}
