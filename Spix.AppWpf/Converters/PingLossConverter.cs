using Spix.xNetwork.PingHelper;
using System.Globalization;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Los intentos perdidos, escritos como en la web: "1 (25.0%)".
//
// Se arma aqui y no en PingResult porque ese modelo lo comparten el Backend, la web y el
// escritorio: no se le mete texto de pantalla.
public class PingLossConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not PingResult resultado)
        {
            return string.Empty;
        }

        return $"{resultado.Lost} ({resultado.LossPercent.ToString("0.0", culture)}%)";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
