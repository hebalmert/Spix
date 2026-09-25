using System.Globalization;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Un enum visto como numero, para los combos cuya lista arma el Backend.
//
// La lista de estados llega como IntItemModel (Value es int) y la entidad guarda un enum:
// sin esto el ComboBox nunca marca el estado que trae el registro, porque compara un int
// contra un enum y nunca coinciden.
public class EnumToIntConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null ? null : System.Convert.ToInt32(value, culture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return null;
        }

        //El destino puede ser anulable: se convierte contra el tipo de adentro
        var destino = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (!destino.IsEnum)
        {
            return value;
        }

        return Enum.ToObject(destino, System.Convert.ToInt32(value, culture));
    }
}
