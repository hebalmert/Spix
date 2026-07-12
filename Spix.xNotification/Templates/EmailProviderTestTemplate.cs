using System.Net;

namespace Spix.xNotification.Templates;

public sealed class EmailProviderTestTemplateModel
{
    public required string Subject { get; init; }
    public required string Title { get; init; }
    public required string Result { get; init; }
    public required string SuccessMessage { get; init; }
    public required string ProviderLabel { get; init; }
    public required string ProviderName { get; init; }
    public required string ProviderType { get; init; }
    public required string Footer { get; init; }
}

/// <summary>
/// Centraliza la presentación HTML del correo de prueba para SendGrid y SMTP.
/// </summary>
public static class EmailProviderTestTemplate
{
    public static string Build(EmailProviderTestTemplateModel model)
    {
        string providerName = WebUtility.HtmlEncode(model.ProviderName);
        string providerType = WebUtility.HtmlEncode(model.ProviderType);

        return $"""
            <!doctype html>
            <html lang="en">
            <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>{model.Subject}</title></head>
            <body style="margin:0;padding:0;background-color:#f3f6fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f3f6fb;"><tr><td align="center" style="padding:40px 16px;">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:600px;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.10);">
                        <tr><td style="padding:30px 36px;background:linear-gradient(135deg,#6d3fd5 0%,#1478ff 100%);color:#ffffff;">
                            <div style="font-size:13px;letter-spacing:1.4px;text-transform:uppercase;opacity:.85;">EMAIL SERVICE</div>
                            <h1 style="margin:8px 0 0;font-size:26px;line-height:1.3;font-weight:700;">{model.Title}</h1>
                        </td></tr>
                        <tr><td style="padding:34px 36px 18px;">
                            <div style="text-align:center;margin-bottom:26px;">
                                <div style="display:inline-block;width:58px;height:58px;line-height:58px;border-radius:50%;background:#e8f8ef;color:#16834a;font-size:30px;font-weight:bold;">&#10003;</div>
                                <h2 style="margin:16px 0 8px;font-size:21px;color:#162033;">{model.Result}</h2>
                                <p style="margin:0;color:#64748b;font-size:15px;line-height:1.7;">{model.SuccessMessage}</p>
                            </div>
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background:#f8fafc;border:1px solid #e5eaf1;border-radius:12px;">
                                <tr><td style="padding:18px 20px;color:#64748b;font-size:13px;text-transform:uppercase;letter-spacing:.6px;">{model.ProviderLabel}</td></tr>
                                <tr><td style="padding:0 20px 20px;font-size:17px;font-weight:700;color:#1e293b;">{providerName}<span style="display:inline-block;margin-left:8px;padding:4px 9px;border-radius:20px;background:#e8efff;color:#315fc5;font-size:12px;font-weight:600;">{providerType}</span></td></tr>
                            </table>
                        </td></tr>
                        <tr><td style="padding:22px 36px 30px;text-align:center;color:#94a3b8;font-size:12px;line-height:1.6;border-top:1px solid #eef2f7;">{model.Footer}</td></tr>
                    </table>
                </td></tr></table>
            </body></html>
            """;
    }
}
