using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Al reves que el de siempre: muestra cuando el booleano es falso.
// Lo usa el aviso de "elija una categoria", que solo aparece si NO hay ninguna elegida.
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
