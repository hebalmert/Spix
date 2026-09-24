using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Esconde un bloque cuando la ventana se hace angosta: se le pasa el ancho actual y el
// minimo que necesita ese bloque para verse bien.
//
// Es lo que reemplaza a las media queries de la web: en WPF no existen, asi que el ancho
// se enlaza y aqui se decide.
public class WidthToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        double ancho = value is double actual ? actual : 0;
        double minimo = 980;

        if (parameter != null)
        {
            double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out minimo);
        }

        return ancho >= minimo ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
