using ClosedXML.Excel;
using System.Text.RegularExpressions;
using Spix.DomainLogic.DTOs;

namespace Spix.AppInfra.SmsHelper;

public class ExcelParser : IExcelParser
{
    public List<SmsRecipient> ParseRecipientsFromBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        using var ms = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(ms);
        var worksheet = workbook.Worksheet(1);

        var recipients = new List<SmsRecipient>();

        foreach (var row in worksheet.RangeUsed()!.RowsUsed().Skip(1))
        {
            var nombre = row.Cell(1).GetString().Trim();
            var telefono = row.Cell(2).GetString().Trim();

            if (!string.IsNullOrEmpty(nombre) && EsTelefonoValido(telefono))
            {
                recipients.Add(new SmsRecipient
                {
                    Nombre = nombre,
                    Telefono = telefono
                });
            }
        }

        return recipients;
    }

    private static bool EsTelefonoValido(string telefono)
    {
        var regex = new Regex(@"^\+\d{10,15}$");
        return regex.IsMatch(telefono);
    }
}