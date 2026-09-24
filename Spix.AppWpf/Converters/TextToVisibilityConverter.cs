using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Muestra un control solo cuando el texto trae algo: se usa para la X que limpia la
// busqueda, que no tiene por que verse con la caja vacia.
public class TextToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
