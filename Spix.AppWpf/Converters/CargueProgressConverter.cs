using Spix.DomainLogic.EntitiesInvenDTO;
using System.Globalization;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// El avance de un cargue: cuanto se subio de lo que dice la compra.
//
// Recibe la fila entera porque el dato sale de dos campos (Uploaded y CantToUp) y en la
// web se calcula igual, en el propio listado. Con el parametro se elige que devolver:
//   percent  -> el porcentaje para la barra (0 a 100)
//   missing  -> cuantos faltan por subir
public class CargueProgressConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var (subidos, total) = value switch
        {
            CargueListItemDto fila => (fila.Uploaded, fila.CantToUp),
            CargueProgressDto avance => (avance.Uploaded, avance.CantToUp),
            _ => (0, 0m)
        };

        var faltan = Math.Max(0, (int)total - subidos);

        if (string.Equals(parameter?.ToString(), "missing", StringComparison.OrdinalIgnoreCase))
        {
            return faltan;
        }

        return total <= 0 ? 0 : Math.Min(100, (int)(subidos * 100 / total));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
