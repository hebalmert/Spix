using System.Globalization;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Pone en mayusculas el titulo de las columnas, como en la web.
//
// Va aqui y no en el texto de cada pantalla: el titulo se escribe normal ("Documento") y
// la tabla decide como se muestra. Si un dia se quiere quitar, se cambia el estilo y ya.
public class UpperCaseConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString()?.ToUpper(culture) ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
