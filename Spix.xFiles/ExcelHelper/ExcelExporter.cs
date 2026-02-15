using ClosedXML.Excel;
using System.Reflection;

namespace Spix.xFiles.ExcelHelper;

public class ExcelExporter : IExcelExporter
{
    public byte[] ExportToExcel<T>(IEnumerable<T> data)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.AddWorksheet("Data");

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Encabezados
        for (int i = 0; i < props.Length; i++)
        {
            ws.Cell(1, i + 1).Value = props[i].Name;
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        // Datos
        int row = 2;
        foreach (var item in data)
        {
            for (int col = 0; col < props.Length; col++)
            {
                var value = props[col].GetValue(item);
                ws.Cell(row, col + 1).SetValue(value?.ToString() ?? "");
            }
            row++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

}

