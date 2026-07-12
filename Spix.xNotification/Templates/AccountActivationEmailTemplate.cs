using System.Net;

namespace Spix.xNotification.Templates;

/// <summary>
/// Datos y textos ya localizados requeridos por el correo de activación.
/// </summary>
public sealed class AccountActivationEmailTemplateModel
{
    public required string Subject { get; init; }
    public required string Title { get; init; }
    public required string Hello { get; init; }
    public required string Welcome { get; init; }
    public required string TemporaryPasswordLabel { get; init; }
    public required string Instruction { get; init; }
    public required string ButtonText { get; init; }
    public required string SecurityNotice { get; init; }
    public required string Footer { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? TemporaryPassword { get; init; }
    public required string ConfirmationLink { get; init; }
}

/// <summary>
/// Centraliza únicamente la presentación HTML del correo de activación.
/// </summary>
public static class AccountActivationEmailTemplate
{
    public static string Build(AccountActivationEmailTemplateModel model)
    {
        string recipientName = WebUtility.HtmlEncode($"{model.FirstName} {model.LastName}".Trim());
        string password = WebUtility.HtmlEncode(model.TemporaryPassword ?? string.Empty);
        string link = WebUtility.HtmlEncode(model.ConfirmationLink);

        return $"""
            <!doctype html>
            <html lang="en">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>{model.Subject}</title></head>
            <body style="margin:0;padding:0;background-color:#f3f6fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f3f6fb;"><tr><td align="center" style="padding:40px 16px;">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:620px;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.10);">
                        <tr><td style="padding:32px 38px;background:linear-gradient(135deg,#6d3fd5 0%,#1478ff 100%);color:#ffffff;">
                            <div style="font-size:13px;letter-spacing:1.4px;text-transform:uppercase;opacity:.85;">ACCOUNT SECURITY</div>
                            <h1 style="margin:8px 0 0;font-size:27px;line-height:1.3;font-weight:700;">{model.Title}</h1>
                        </td></tr>
                        <tr><td style="padding:34px 38px 20px;">
                            <p style="margin:0 0 14px;font-size:18px;font-weight:700;color:#172033;">{model.Hello} {recipientName},</p>
                            <p style="margin:0 0 24px;color:#64748b;font-size:15px;line-height:1.7;">{model.Welcome}</p>
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background:#f8fafc;border:1px solid #e5eaf1;border-radius:12px;">
                                <tr><td style="padding:18px 20px 8px;color:#64748b;font-size:12px;text-transform:uppercase;letter-spacing:.7px;">{model.TemporaryPasswordLabel}</td></tr>
                                <tr><td style="padding:0 20px 20px;font-size:22px;font-weight:700;color:#1e293b;letter-spacing:1px;">{password}</td></tr>
                            </table>
                            <p style="margin:24px 0 18px;color:#475569;font-size:15px;line-height:1.7;text-align:center;">{model.Instruction}</p>
                            <div style="text-align:center;margin-bottom:24px;"><a href="{link}" style="display:inline-block;padding:14px 28px;border-radius:10px;background:linear-gradient(135deg,#6d3fd5 0%,#1478ff 100%);color:#ffffff;text-decoration:none;font-size:15px;font-weight:700;box-shadow:0 8px 18px rgba(49,95,197,.24);">{model.ButtonText}</a></div>
                            <div style="padding:14px 16px;border-radius:10px;background:#fff8e6;border:1px solid #f7d98a;color:#7a5b10;font-size:13px;line-height:1.6;">{model.SecurityNotice}</div>
                        </td></tr>
                        <tr><td style="padding:22px 38px 30px;text-align:center;color:#94a3b8;font-size:12px;line-height:1.6;border-top:1px solid #eef2f7;">{model.Footer}</td></tr>
                    </table>
                </td></tr></table>
            </body></html>
            """;
    }
}
