using ClosedXML.Excel;
using Microsoft.Win32;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents;

// Baja a Excel lo que el listado tiene en pantalla.
//
// Es un componente plano: se le pasa la lista y el nombre del archivo, y el se encarga.
// Ningun ViewModel cambia por esto, y sirve igual en las 20 pantallas.
//
// Arma la hoja igual que el ExcelExporter del Backend (Spix.xFiles): salta las columnas
// [NotMapped], usa el nombre de [Display] como titulo y ajusta el ancho. No se referencia
// ese proyecto para no arrastrarle al escritorio EF, Azure y los PDF.
public partial class SharedExportButton : UserControl
{
    public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
        nameof(Items),
        typeof(IEnumerable),
        typeof(SharedExportButton));

    public static readonly DependencyProperty FileNameProperty = DependencyProperty.Register(
        nameof(FileName),
        typeof(string),
        typeof(SharedExportButton),
        new PropertyMetadata("listado"));

    public SharedExportButton()
    {
        InitializeComponent();
    }

    public IEnumerable? Items
    {
        get => (IEnumerable?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public string FileName
    {
        get => (string)GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }

    private void ExportClick(object sender, RoutedEventArgs e)
    {
        var filas = Items?.Cast<object>().ToList() ?? new List<object>();
        if (filas.Count == 0)
        {
            MessageBox.Show("No hay nada que exportar.", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialogo = new SaveFileDialog
        {
            Filter = "Libro de Excel|*.xlsx",
            FileName = $"{FileName}-{DateTime.Now:yyyyMMdd-HHmm}.xlsx"
        };

        if (dialogo.ShowDialog() != true)
        {
            return;
        }

        try
        {
            GuardarExcel(filas, dialogo.FileName);
            MessageBox.Show("El archivo quedo guardado.", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Exportar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static void GuardarExcel(List<object> filas, string ruta)
    {
        //Solo las columnas que se pueden escribir en una celda: los objetos anidados y las
        //listas no caben en una hoja
        var propiedades = filas[0].GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.GetCustomAttribute<NotMappedAttribute>() == null)
            .Where(x => EsSimple(x.PropertyType))
            .ToList();

        using var libro = new XLWorkbook();
        var hoja = libro.AddWorksheet("Datos");

        //El encabezado, con el nombre bonito cuando la entidad lo trae
        for (int i = 0; i < propiedades.Count; i++)
        {
            var display = propiedades[i].GetCustomAttribute<DisplayAttribute>();

            hoja.Cell(1, i + 1).Value = display?.GetName() ?? propiedades[i].Name;
            hoja.Cell(1, i + 1).Style.Font.Bold = true;
        }

        //Los datos, cada tipo con su formato para que Excel los entienda como numero o fecha
        int fila = 2;
        foreach (var item in filas)
        {
            for (int col = 0; col < propiedades.Count; col++)
            {
                Escribir(hoja.Cell(fila, col + 1), propiedades[col].GetValue(item));
            }

            fila++;
        }

        //El encabezado se congela y trae filtros: es lo primero que uno hace en Excel
        hoja.SheetView.FreezeRows(1);
        hoja.Range(1, 1, Math.Max(fila - 1, 1), Math.Max(propiedades.Count, 1)).SetAutoFilter();
        hoja.Columns().AdjustToContents();

        libro.SaveAs(ruta);
    }

    private static void Escribir(IXLCell celda, object? valor)
    {
        switch (valor)
        {
            case null:
                celda.Value = string.Empty;
                break;

            case DateTime fecha:
                celda.Value = fecha;
                celda.Style.DateFormat.Format = "dd/MM/yyyy";
                break;

            case decimal numero:
                celda.Value = numero;
                celda.Style.NumberFormat.Format = "#,##0.00";
                break;

            case int entero:
                celda.Value = entero;
                break;

            case long largo:
                celda.Value = largo;
                break;

            case bool si:
                celda.Value = si ? "Si" : "No";
                break;

            case Enum opcion:
                celda.Value = opcion.ToString();
                break;

            default:
                celda.Value = valor.ToString() ?? string.Empty;
                break;
        }
    }

    private static bool EsSimple(Type tipo)
    {
        var real = Nullable.GetUnderlyingType(tipo) ?? tipo;

        return real.IsPrimitive ||
               real.IsEnum ||
               real == typeof(string) ||
               real == typeof(decimal) ||
               real == typeof(DateTime) ||
               real == typeof(Guid);
    }
}
