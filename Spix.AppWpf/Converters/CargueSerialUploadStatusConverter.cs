using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using System.Globalization;
using System.Windows.Data;

namespace Spix.AppWpf.Converters;

// Resume visualmente si el cargue aun requiere seriales antes de poder completarse.
public class CargueSerialUploadStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Cargue cargue)
        {
            return string.Empty;
        }

        if (cargue.Status == CargueType.Completado)
        {
            return "Cargue completado";
        }

        var pendingSerials = Math.Max(0, cargue.CantToUp - cargue.TotalSeriales);
        if (pendingSerials == 0)
        {
            return "Listo para cerrar";
        }

        return $"Faltan {pendingSerials:N0} seriales";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

// Habilita el boton de subir mientras haya seriales pendientes y el cargue permanezca abierto.
public class CargueCanUploadSerialConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Cargue cargue)
        {
            return false;
        }

        return cargue.Status == CargueType.Pendiente &&
               cargue.TotalSeriales < cargue.CantToUp;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
