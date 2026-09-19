using System.Net;

namespace Spix.xNotification.Templates;

public sealed class SignatureRequestEmailTemplateModel
{
    public required string Subject { get; init; }
    public required string Eyebrow { get; init; }
    public required string Title { get; init; }
    public required string Hello { get; init; }
    public required string Introduction { get; init; }
    public required string Instruction { get; init; }
    public required string ButtonText { get; init; }
    public required string SecurityNotice { get; init; }
    public required string Footer { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public required string ContractNumber { get; init; }
    public required string DocumentsList { get; init; }
    public required string SignatureLink { get; init; }
}

/// <summary>
/// Correo que invita al cliente a firmar su contrato y su consentimiento.
/// El enlace solo lleva al portal: para firmar hay que entrar con la cuenta y validar el codigo.
/// </summary>
public static class SignatureRequestEmailTemplate
{
    public static string Build(SignatureRequestEmailTemplateModel model)
    {
        string recipientName = WebUtility.HtmlEncode($"{model.FirstName} {model.LastName}".Trim());
        string link = WebUtility.HtmlEncode(model.SignatureLink);
        string documents = WebUtility.HtmlEncode(model.DocumentsList);
        string contract = WebUtility.HtmlEncode(model.ContractNumber);

        return $"""
            <!doctype html>
            <html lang="en">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>{model.Subject}</title></head>
            <body style="margin:0;padding:0;background-color:#f3f6fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f3f6fb;"><tr><td align="center" style="padding:40px 16px;">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:620px;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.10);">
                        <tr><td style="padding:32px 38px;background:linear-gradient(135deg,#0a1a3f 0%,#1478ff 100%);color:#ffffff;">
                            <div style="font-size:13px;letter-spacing:1.4px;text-transform:uppercase;opacity:.85;">{model.Eyebrow}</div>
                            <h1 style="margin:8px 0 0;font-size:27px;line-height:1.3;font-weight:700;">{model.Title}</h1>
                        </td></tr>
                        <tr><td style="padding:30px 38px 8px;">
                            <p style="margin:0 0 14px;font-size:16px;">{model.Hello} <strong>{recipientName}</strong>,</p>
                            <p style="margin:0 0 14px;font-size:15px;line-height:1.6;">{model.Introduction}</p>
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:6px 0 18px;background:#f6f9ff;border:1px solid #d7e1f3;border-radius:12px;">
                                <tr><td style="padding:14px 18px;font-size:15px;">
                                    <div style="color:#5b6784;font-size:13px;">#{contract}</div>
                                    <div style="margin-top:4px;font-weight:700;color:#0a1a3f;">{documents}</div>
                                </td></tr>
                            </table>
                            <p style="margin:0 0 20px;font-size:15px;line-height:1.6;">{model.Instruction}</p>
                            <p style="margin:0 0 26px;text-align:center;">
                                <a href="{link}" style="display:inline-block;padding:13px 30px;background:#0d6efd;color:#ffffff;text-decoration:none;border-radius:10px;font-weight:700;font-size:15px;">{model.ButtonText}</a>
                            </p>
                            <p style="margin:0 0 18px;font-size:13px;line-height:1.6;color:#5b6784;">{model.SecurityNotice}</p>
                        </td></tr>
                        <tr><td style="padding:18px 38px 30px;border-top:1px solid #eef1f7;font-size:12px;color:#8a93a8;">{model.Footer}</td></tr>
                    </table>
                </td></tr></table>
            </body>
            </html>
            """;
    }
}
