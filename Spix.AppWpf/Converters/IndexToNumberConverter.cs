using System.Globalization;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// El numero de la fila para el usuario: la lista empieza en cero y la gente cuenta desde uno.
public class IndexToNumberConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int indice ? indice + 1 : 1;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
