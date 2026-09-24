using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Spix.AppWpf.Converters;

// El color del stock del producto, con el mismo limite que la web: rojo si no queda
// nada, ambar si quedan 15 o menos, verde si hay de sobra.
public class StockToBrushConverter : IValueConverter
{
    private const decimal LowStockLimit = 15;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var stock = value is decimal numero ? numero : 0;

        var clave = stock == 0
            ? "BrushCatalogStockNone"
            : stock <= LowStockLimit
                ? "BrushCatalogStockLow"
                : "BrushCatalogStockOk";

        return Application.Current.TryFindResource(clave) as Brush ?? Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
