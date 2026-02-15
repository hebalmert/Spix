using ClosedXML.Excel;

namespace Spix.xFiles.ExcelHelper;

public class ExcelImporter : IExcelImporter
{
    public List<T> ParseFromBase64<T>(string base64, Func<IXLRow, T> map)
    {
        var bytes = Convert.FromBase64String(base64);
        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        var worksheet = workbook.Worksheet(1);

        var result = new List<T>();

        // Saltamos encabezado
        var rows = worksheet.RowsUsed().Skip(1);

        foreach (var row in rows)
        {
            try
            {
                var item = map(row);
                result.Add(item);
            }
            catch
            {
                continue;
            }
        }

        return result;
    }
}
