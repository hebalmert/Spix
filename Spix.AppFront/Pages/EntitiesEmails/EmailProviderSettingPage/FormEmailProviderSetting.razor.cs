using Microsoft.AspNetCore.Components;
using Spix.Domain.EntitiesEmails;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppFront.Pages.EntitiesEmails.EmailProviderSettingPage;

public partial class FormEmailProviderSetting
{
    [Parameter, EditorRequired] public EmailProviderSetting EmailProvider { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private bool showSendGridSecret;
    private bool showSmtpSecret;
    private string SendGridInputType => showSendGridSecret ? "text" : "password";
    private string SmtpPasswordInputType => showSmtpSecret ? "text" : "password";

    private void ToggleSendGridSecret()
    {
        showSendGridSecret = !showSendGridSecret;
    }

    private void ToggleSmtpSecret()
    {
        showSmtpSecret = !showSmtpSecret;
    }

    private void ProviderChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out int providerType))
        {
            EmailProvider.ProviderType = (EmailProviderType)providerType;
        }

        if (EmailProvider.ProviderType == EmailProviderType.Gmail)
        {
            EmailProvider.SmtpHost ??= "smtp.gmail.com";
            EmailProvider.SmtpPort ??= 587;
            EmailProvider.SmtpUseSsl = true;
        }
    }
}
