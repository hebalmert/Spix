using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Muestra un bloque segun si un numero es cero o no.
//
// Lo usan las reglas del inventario que ya estan en la web: un proveedor con compras no
// se borra, una bodega con existencias tampoco. El boton de borrar solo aparece cuando
// el contador esta en cero.
//
// Sin parametro: visible cuando el numero es CERO.
// Con ConverterParameter=positive: visible cuando el numero es MAYOR que cero.
public class NumberToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        decimal numero = value switch
        {
            int entero => entero,
            decimal decimalValue => decimalValue,
            double doubleValue => (decimal)doubleValue,
            long largo => largo,
            _ => 0
        };

        var esperaPositivo = string.Equals(
            parameter?.ToString(),
            "positive",
            StringComparison.OrdinalIgnoreCase);

        var mostrar = esperaPositivo ? numero > 0 : numero == 0;

        return mostrar ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
