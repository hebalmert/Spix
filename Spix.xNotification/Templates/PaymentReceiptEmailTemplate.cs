using System.Net;

namespace Spix.xNotification.Templates;

public sealed class PaymentReceiptLineModel
{
    public required string Concept { get; init; }
    public required string Amount { get; init; }
    public string? Origin { get; init; }
}

public sealed class PaymentReceiptEmailTemplateModel
{
    public required string Subject { get; init; }
    public required string Eyebrow { get; init; }
    public required string Title { get; init; }
    public required string Hello { get; init; }
    public required string Introduction { get; init; }
    public required string PaidBadge { get; init; }
    public required string Footer { get; init; }

    //Etiquetas, para que el comprobante salga en el idioma del sistema
    public required string AmountLabel { get; init; }
    public required string NoteLabel { get; init; }
    public required string ContractLabel { get; init; }
    public required string DateLabel { get; init; }
    public required string ConceptLabel { get; init; }
    public required string DebtLabel { get; init; }
    public required string DiscountLabel { get; init; }
    public required string BalanceLabel { get; init; }
    public required string ModeLabel { get; init; }
    public required string ReceivedByLabel { get; init; }

    //Los datos de la corporacion que emite el comprobante
    public string? CorporationName { get; init; }
    public string? CorporationDocument { get; init; }
    public string? CorporationAddress { get; init; }
    public string? CorporationPhone { get; init; }
    public string? CorporationLogo { get; init; }
    public string? ClientName { get; init; }
    public required string NoteNumber { get; init; }
    public required string Contract { get; init; }
    public required string Date { get; init; }
    public required string Debt { get; init; }
    public required string Discount { get; init; }
    public required string Payment { get; init; }
    public required string Balance { get; init; }
    public required string Mode { get; init; }
    public required string ReceivedBy { get; init; }

    public bool HasDiscount { get; init; }

    public List<PaymentReceiptLineModel> Lines { get; init; } = new();
}

/// <summary>
/// El comprobante de lo que el cliente acaba de pagar.
///
/// Va en el cuerpo del correo, no adjunto: se abre en el celular sin descargar nada.
/// Todo el maquetado es con tablas y estilos en linea, que es lo unico que respetan
/// Gmail, Outlook y compania; los degradados llevan su color solido de respaldo.
/// </summary>
public static class PaymentReceiptEmailTemplate
{
    public static string Build(PaymentReceiptEmailTemplateModel model)
    {
        string corporation = WebUtility.HtmlEncode(model.CorporationName ?? string.Empty);
        string client = WebUtility.HtmlEncode(model.ClientName ?? string.Empty);

        //El logo de la corporacion, si lo tiene cargado
        var logo = string.IsNullOrWhiteSpace(model.CorporationLogo) ||
                   !model.CorporationLogo.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : $@"<img src=""{WebUtility.HtmlEncode(model.CorporationLogo)}"" alt=""{corporation}"" height=""34"" style=""display:block;max-height:34px;border:0;"" />";

        //Sus datos, para el pie: documento, direccion y telefono
        var datos = string.Join(" &nbsp;&middot;&nbsp; ", new[]
        {
            model.CorporationDocument,
            model.CorporationAddress,
            model.CorporationPhone
        }.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => WebUtility.HtmlEncode(x!)));

        //Lo que se le cobro, renglon por renglon
        var lines = string.Join(string.Empty, model.Lines.Select((line, i) => $@"
                                <tr style=""background:{(i % 2 == 0 ? "#ffffff" : "#f7f9fd")};"">
                                    <td style=""padding:11px 14px;color:#152238;font-size:14px;border-bottom:1px solid #edf1f7;"">
                                        {WebUtility.HtmlEncode(line.Concept)}
                                        {(string.IsNullOrWhiteSpace(line.Origin) ? string.Empty : $@"<div style=""color:#8b95ad;font-size:11px;margin-top:2px;"">{WebUtility.HtmlEncode(line.Origin)}</div>")}
                                    </td>
                                    <td style=""padding:11px 14px;color:#152238;font-size:14px;font-weight:600;text-align:right;white-space:nowrap;border-bottom:1px solid #edf1f7;"">
                                        {WebUtility.HtmlEncode(line.Amount)}
                                    </td>
                                </tr>"));

        //El descuento solo se muestra si de verdad hubo
        var discountRow = !model.HasDiscount ? string.Empty : $@"
                                <tr>
                                    <td style=""padding:3px 14px;color:#6b7690;font-size:13px;"">{WebUtility.HtmlEncode(model.DiscountLabel)}</td>
                                    <td style=""padding:3px 14px;color:#6f42c1;font-size:13px;font-weight:600;text-align:right;"">- {WebUtility.HtmlEncode(model.Discount)}</td>
                                </tr>";

        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
    <title>{WebUtility.HtmlEncode(model.Subject)}</title>
</head>
<body style=""margin:0;padding:0;background:#e9eef7;"">
    <div style=""display:none;max-height:0;overflow:hidden;opacity:0;"">{WebUtility.HtmlEncode(model.Title)} — {WebUtility.HtmlEncode(model.Payment)}</div>

    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#e9eef7;font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;"">
        <tr>
            <td align=""center"" style=""padding:28px 12px;"">
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:580px;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 14px 34px rgba(16,32,60,.16);"">

                    <!-- Cabecera azul con el monto: lo primero que se ve es cuanto pago -->
                    <tr>
                        <td bgcolor=""#16305e"" style=""background:#16305e;background-image:linear-gradient(135deg,#0a1a3f 0%,#16305e 55%,#1d4a95 100%);padding:30px 30px 26px;"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td style=""color:#ffffff;font-size:15px;font-weight:700;letter-spacing:.2px;"">{(string.IsNullOrWhiteSpace(logo) ? corporation : logo)}</td>
                                    <td align=""right"">
                                        <span style=""display:inline-block;background:#198754;color:#ffffff;font-size:11px;font-weight:700;letter-spacing:.6px;text-transform:uppercase;padding:5px 13px;border-radius:999px;"">{WebUtility.HtmlEncode(model.PaidBadge)}</span>
                                    </td>
                                </tr>
                            </table>

                            <div style=""color:#9fb6de;font-size:11px;letter-spacing:1.2px;text-transform:uppercase;margin-top:22px;"">{WebUtility.HtmlEncode(model.Eyebrow)}</div>
                            <div style=""color:#ffffff;font-size:21px;font-weight:700;margin-top:3px;"">{WebUtility.HtmlEncode(model.Title)}</div>

                            <div style=""margin-top:18px;padding-top:16px;border-top:1px solid rgba(255,255,255,.18);"">
                                <div style=""color:#9fb6de;font-size:11px;letter-spacing:.8px;text-transform:uppercase;"">{WebUtility.HtmlEncode(model.AmountLabel)}</div>
                                <div style=""color:#ffffff;font-size:34px;font-weight:700;line-height:1.15;margin-top:2px;"">{WebUtility.HtmlEncode(model.Payment)}</div>
                            </div>
                        </td>
                    </tr>

                    <!-- Saludo -->
                    <tr>
                        <td style=""padding:26px 30px 0;"">
                            <div style=""color:#152238;font-size:16px;font-weight:700;"">{WebUtility.HtmlEncode(model.Hello)} {client}</div>
                            <div style=""color:#5b6784;font-size:14px;line-height:1.55;margin-top:6px;"">{WebUtility.HtmlEncode(model.Introduction)}</div>
                        </td>
                    </tr>

                    <!-- Nota, contrato y fecha -->
                    <tr>
                        <td style=""padding:18px 30px 0;"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f7f9fd;border-left:4px solid #16305e;border-radius:10px;"">
                                <tr>
                                    <td style=""padding:12px 14px 4px;color:#6b7690;font-size:12px;"">{WebUtility.HtmlEncode(model.NoteLabel)}</td>
                                    <td style=""padding:12px 14px 4px;color:#16305e;font-size:13px;font-weight:700;text-align:right;"">{WebUtility.HtmlEncode(model.NoteNumber)}</td>
                                </tr>
                                <tr>
                                    <td style=""padding:2px 14px;color:#6b7690;font-size:12px;"">{WebUtility.HtmlEncode(model.ContractLabel)}</td>
                                    <td style=""padding:2px 14px;color:#152238;font-size:13px;text-align:right;"">{WebUtility.HtmlEncode(model.Contract)}</td>
                                </tr>
                                <tr>
                                    <td style=""padding:2px 14px 12px;color:#6b7690;font-size:12px;"">{WebUtility.HtmlEncode(model.DateLabel)}</td>
                                    <td style=""padding:2px 14px 12px;color:#152238;font-size:13px;text-align:right;"">{WebUtility.HtmlEncode(model.Date)}</td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- De que se compone lo que pago -->
                    <tr>
                        <td style=""padding:22px 30px 0;"">
                            <div style=""color:#6b7690;font-size:11px;font-weight:700;letter-spacing:.8px;text-transform:uppercase;margin-bottom:8px;"">{WebUtility.HtmlEncode(model.ConceptLabel)}</div>
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border:1px solid #e6ecf6;border-radius:10px;overflow:hidden;"">{lines}
                                <tr>
                                    <td style=""padding:10px 14px 3px;color:#6b7690;font-size:13px;"">{WebUtility.HtmlEncode(model.DebtLabel)}</td>
                                    <td style=""padding:10px 14px 3px;color:#152238;font-size:13px;text-align:right;"">{WebUtility.HtmlEncode(model.Debt)}</td>
                                </tr>{discountRow}
                                <tr>
                                    <td style=""padding:3px 14px 12px;color:#6b7690;font-size:13px;"">{WebUtility.HtmlEncode(model.BalanceLabel)}</td>
                                    <td style=""padding:3px 14px 12px;color:#198754;font-size:13px;font-weight:700;text-align:right;"">{WebUtility.HtmlEncode(model.Balance)}</td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- Como y quien lo recibio -->
                    <tr>
                        <td style=""padding:18px 30px 26px;"">
                            <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                <tr>
                                    <td width=""50%"" style=""padding-right:6px;"" valign=""top"">
                                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f7f9fd;border-radius:10px;"">
                                            <tr><td style=""padding:11px 13px 2px;color:#8b95ad;font-size:11px;text-transform:uppercase;letter-spacing:.5px;"">{WebUtility.HtmlEncode(model.ModeLabel)}</td></tr>
                                            <tr><td style=""padding:0 13px 11px;color:#152238;font-size:14px;font-weight:600;"">{WebUtility.HtmlEncode(model.Mode)}</td></tr>
                                        </table>
                                    </td>
                                    <td width=""50%"" style=""padding-left:6px;"" valign=""top"">
                                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f7f9fd;border-radius:10px;"">
                                            <tr><td style=""padding:11px 13px 2px;color:#8b95ad;font-size:11px;text-transform:uppercase;letter-spacing:.5px;"">{WebUtility.HtmlEncode(model.ReceivedByLabel)}</td></tr>
                                            <tr><td style=""padding:0 13px 11px;color:#152238;font-size:14px;font-weight:600;"">{WebUtility.HtmlEncode(model.ReceivedBy)}</td></tr>
                                        </table>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <tr>
                        <td bgcolor=""#0a1a3f"" style=""background:#0a1a3f;padding:18px 30px;color:#9fb6de;font-size:11px;line-height:1.7;text-align:center;"">
                            <div style=""color:#ffffff;font-size:13px;font-weight:700;"">{corporation}</div>
                            {(string.IsNullOrWhiteSpace(datos) ? string.Empty : $@"<div style=""margin-top:3px;"">{datos}</div>")}
                            <div style=""margin-top:8px;"">{WebUtility.HtmlEncode(model.Footer)}</div>
                        </td>
                    </tr>
                </table>

                <div style=""color:#8b95ad;font-size:11px;margin-top:14px;"">{corporation}</div>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
