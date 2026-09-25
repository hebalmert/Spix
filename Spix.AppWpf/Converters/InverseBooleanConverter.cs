using System.Globalization;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Lo contrario de un booleano. Sirve para apagar un campo cuando algo YA paso: la orden
// cerrada no se edita, y en el ViewModel la propiedad que existe es "IsCompleted".
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not bool valor || !valor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is not bool valor || !valor;
    }
}
