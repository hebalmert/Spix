using System.Globalization;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Dice si el valor es igual al parametro. Lo usan los chips del catalogo para saber
// cual esta puesto, sin tener que declarar un booleano por chip en el ViewModel.
public class EqualsToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.Equals(
            value?.ToString(),
            parameter?.ToString(),
            StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
