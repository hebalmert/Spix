using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Al reves que EqualsToVisibilityConverter: muestra el bloque cuando el valor NO es igual
// al parametro.
//
// Lo usa el listado de seriales, donde el boton de editar solo aparece mientras el serial
// no este Operativo, igual que en la web.
public class NotEqualsToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.OrdinalIgnoreCase)
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
