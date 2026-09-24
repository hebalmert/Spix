using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Muestra el aviso de "no hay registros" cuando la lista llega vacia.
//
// Se le pasa la coleccion: devuelve Visible si esta vacia y Collapsed si trae filas.
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        int total = value switch
        {
            null => 0,
            int numero => numero,
            ICollection coleccion => coleccion.Count,
            IEnumerable lista => lista.Cast<object>().Count(),
            _ => 0
        };

        return total == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
