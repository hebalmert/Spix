using System.Net;

namespace Spix.xNotification.Templates;

public sealed class PasswordRecoveryEmailTemplateModel
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
    public required string RecoveryLink { get; init; }
}

/// <summary>
/// Centraliza la presentación HTML de la recuperación de contraseña.
/// </summary>
public static class PasswordRecoveryEmailTemplate
{
    public static string Build(PasswordRecoveryEmailTemplateModel model)
    {
        string recipientName = WebUtility.HtmlEncode($"{model.FirstName} {model.LastName}".Trim());
        string link = WebUtility.HtmlEncode(model.RecoveryLink);

        return $"""
            <!doctype html>
            <html lang="en">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>{model.Subject}</title></head>
            <body style="margin:0;padding:0;background-color:#f3f6fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f3f6fb;"><tr><td align="center" style="padding:40px 16px;">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:620px;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.10);">
                        <tr><td style="padding:32px 38px;background:linear-gradient(135deg,#6d3fd5 0%,#1478ff 100%);color:#ffffff;">
                            <div style="font-size:13px;letter-spacing:1.4px;text-transform:uppercase;opacity:.85;">{model.Eyebrow}</div>
                            <h1 style="margin:8px 0 0;font-size:27px;line-height:1.3;font-weight:700;">{model.Title}</h1>
                        </td></tr>
                        <tr><td style="padding:34px 38px 20px;">
                            <div style="text-align:center;margin-bottom:22px;"><div style="display:inline-block;width:60px;height:60px;line-height:60px;border-radius:50%;background:#eef3ff;color:#315fc5;font-size:27px;font-weight:bold;">&#128274;</div></div>
                            <p style="margin:0 0 14px;font-size:18px;font-weight:700;color:#172033;">{model.Hello} {recipientName},</p>
                            <p style="margin:0 0 12px;color:#64748b;font-size:15px;line-height:1.7;">{model.Introduction}</p>
                            <p style="margin:0 0 24px;color:#475569;font-size:15px;line-height:1.7;">{model.Instruction}</p>
                            <div style="text-align:center;margin-bottom:26px;"><a href="{link}" style="display:inline-block;padding:14px 30px;border-radius:10px;background:linear-gradient(135deg,#6d3fd5 0%,#1478ff 100%);color:#ffffff;text-decoration:none;font-size:15px;font-weight:700;box-shadow:0 8px 18px rgba(49,95,197,.24);">{model.ButtonText}</a></div>
                            <div style="padding:14px 16px;border-radius:10px;background:#fff8e6;border:1px solid #f7d98a;color:#7a5b10;font-size:13px;line-height:1.6;">{model.SecurityNotice}</div>
                        </td></tr>
                        <tr><td style="padding:22px 38px 30px;text-align:center;color:#94a3b8;font-size:12px;line-height:1.6;border-top:1px solid #eef2f7;">{model.Footer}</td></tr>
                    </table>
                </td></tr></table>
            </body></html>
            """;
    }
}
