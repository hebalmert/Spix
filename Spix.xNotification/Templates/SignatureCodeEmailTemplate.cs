using System.Net;

namespace Spix.xNotification.Templates;

public sealed class SignatureCodeEmailTemplateModel
{
    public required string Subject { get; init; }
    public required string Eyebrow { get; init; }
    public required string Title { get; init; }
    public required string Hello { get; init; }
    public required string Introduction { get; init; }
    public required string Expiration { get; init; }
    public required string SecurityNotice { get; init; }
    public required string Footer { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public required string Code { get; init; }
}

/// <summary>
/// Codigo de un solo uso que el cliente necesita para firmar.
/// Es el segundo factor de la firma electronica (algo que tiene): sin el no se puede firmar.
/// </summary>
public static class SignatureCodeEmailTemplate
{
    public static string Build(SignatureCodeEmailTemplateModel model)
    {
        string recipientName = WebUtility.HtmlEncode($"{model.FirstName} {model.LastName}".Trim());
        string code = WebUtility.HtmlEncode(model.Code);

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
                            <p style="margin:0 0 18px;font-size:15px;line-height:1.6;">{model.Introduction}</p>
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="margin:6px 0 18px;background:#f6f9ff;border:1px solid #d7e1f3;border-radius:12px;">
                                <tr><td align="center" style="padding:22px 18px;">
                                    <div style="font-size:38px;letter-spacing:10px;font-weight:700;color:#0a1a3f;">{code}</div>
                                </td></tr>
                            </table>
                            <p style="margin:0 0 20px;font-size:15px;line-height:1.6;">{model.Expiration}</p>
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
