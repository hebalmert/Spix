using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Muestra un bloque solo cuando el valor es igual al parametro.
//
// Se usa para los campos que dependen de una opcion: por ejemplo la clave de SendGrid,
// que solo se pide cuando el proveedor es SendGrid.
public class EqualsToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var actual = value?.ToString();
        var esperado = parameter?.ToString();

        return string.Equals(actual, esperado, StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
