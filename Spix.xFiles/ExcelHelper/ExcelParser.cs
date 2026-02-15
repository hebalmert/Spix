using ClosedXML.Excel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Spix.xFiles.ExcelHelper;

public class ExcelParser : IExcelParser
{
    public List<PatientControlExcel> ParseRecipientsFromBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        var worksheet = workbook.Worksheet(1);

        var recipients = new List<PatientControlExcel>();

        // Asumiendo que la primera fila es encabezado
        var rows = worksheet.RangeUsed()!.RowsUsed().Skip(1);

        int filaActual = 2; // empieza en 2 porque saltaste encabezado

        foreach (var row in rows)
        {
            try
            {
                var dobCell = row.Cell(1).GetString().Trim();
                var screenRand = row.Cell(2).GetString().Trim();
                var rawPhone = row.Cell(3).GetString().Trim();
                var lastName = row.Cell(4).GetString().Trim();
                var firstName = row.Cell(5).GetString().Trim();
                var status = row.Cell(6).GetString().Trim();

                // Parse fecha con formato tipo 20/Jun/1952
                if (!DateTime.TryParseExact(dobCell, "dd/MMM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dob))
                {
                    Console.WriteLine($"Fila {filaActual}: Fecha inválida '{dobCell}'");
                    filaActual++;
                    continue;
                }

                recipients.Add(new PatientControlExcel
                {
                    BoD = dob,
                    ScreenRand = string.IsNullOrWhiteSpace(screenRand) ? null : screenRand,
                    CellPhone = string.IsNullOrWhiteSpace(rawPhone) ? null : rawPhone,
                    LastName = lastName,
                    FirtName = firstName,
                    Status = string.IsNullOrWhiteSpace(status) ? null : status,
                });
            }
            catch (Exception ex)
            {
                // Puedes loggear fila inválida si quieres
                Console.WriteLine($"Fila {filaActual}: Error al procesar - {ex.Message}");
                continue;
            }
            filaActual++;
        }

        return recipients;
    }

    private static string? NormalizarTelefono(string telefono)
    {
        // Elimina todo lo que no sea dígito
        var soloDigitos = Regex.Replace(telefono, @"\D", "");

        // Verifica que tenga exactamente 10 dígitos
        if (soloDigitos.Length == 10)
            return soloDigitos;

        return null; // inválido
    }

    private static bool EsTelefonoValido(string telefono)
    {
        var regex = new Regex(@"^\+\d{10,15}$");
        return regex.IsMatch(telefono);
    }
}