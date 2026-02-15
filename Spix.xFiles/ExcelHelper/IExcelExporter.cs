namespace Spix.xFiles.ExcelHelper;

public interface IExcelExporter
{
    byte[] ExportToExcel<T>(IEnumerable<T> data);

}
