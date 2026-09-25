using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Spix.AppWpf.Converters;

// Un intento del ping. El tiempo llega en milisegundos y el -1 significa que no respondio.
//
// Con el parametro se elige que devolver para pintar su pastilla:
//   (sin parametro) -> "23 ms" o "Timeout"
//   fondo           -> verde claro si respondio, rojo claro si no
//   borde           -> el borde de esa pastilla
//   color           -> el color del texto
public class PingAttemptConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var tiempo = value is long milisegundos ? milisegundos : -1;
        var respondio = tiempo >= 0;

        return parameter?.ToString()?.ToLowerInvariant() switch
        {
            "fondo" => Pincel(respondio, "BrushIndexPillOnBack", "BrushIndexPillOffBack"),
            "borde" => Pincel(respondio, "BrushIndexPillOnBorder", "BrushIndexPillOffBorder"),
            "color" => Pincel(respondio, "BrushIndexPillOnText", "BrushIndexPillOffText"),
            _ => respondio ? $"{tiempo} ms" : "Timeout"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static Brush Pincel(bool respondio, string claveSi, string claveNo)
    {
        var clave = respondio ? claveSi : claveNo;

        return Application.Current.TryFindResource(clave) as Brush ?? Brushes.Gray;
    }
}
