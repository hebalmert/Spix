using Spix.xNotification.Templates;
using Microsoft.Extensions.Localization;

namespace Spix.AppService.ImplementEmails;

/// <summary>
/// Traduce los recursos del request actual y entrega los datos a las
/// plantillas neutrales almacenadas en xNotification.
/// </summary>
internal static class LocalizedEmailTemplateFactory
{
    public static string BuildAccountActivation(IStringLocalizer localizer, string? firstName, string? lastName, string? temporaryPassword, string confirmationLink)
        => AccountActivationEmailTemplate.Build(new AccountActivationEmailTemplateModel
        {
            Subject = localizer["AccountActivation_Subject"],
            Title = localizer["AccountActivation_Title"],
            Hello = localizer["AccountActivation_Hello"],
            Welcome = localizer["AccountActivation_Welcome"],
            TemporaryPasswordLabel = localizer["AccountActivation_TemporaryPassword"],
            Instruction = localizer["AccountActivation_Instruction"],
            ButtonText = localizer["AccountActivation_Button"],
            SecurityNotice = localizer["AccountActivation_SecurityNotice"],
            Footer = localizer["AccountActivation_Footer"],
            FirstName = firstName,
            LastName = lastName,
            TemporaryPassword = temporaryPassword,
            ConfirmationLink = confirmationLink
        });

    public static string BuildProviderTest(IStringLocalizer localizer, string providerName, string providerType)
        => EmailProviderTestTemplate.Build(new EmailProviderTestTemplateModel
        {
            Subject = localizer["EmailProviderTest_Subject"],
            Title = localizer["EmailProviderTest_Title"],
            Result = localizer["EmailProviderTest_Result"],
            SuccessMessage = localizer["EmailProviderTest_SuccessMessage"],
            ProviderLabel = localizer["EmailProviderTest_Provider"],
            ProviderName = providerName,
            ProviderType = providerType,
            Footer = localizer["EmailProviderTest_Footer"]
        });

    public static string BuildPasswordRecovery(IStringLocalizer localizer, string? firstName, string? lastName, string recoveryLink)
        => PasswordRecoveryEmailTemplate.Build(new PasswordRecoveryEmailTemplateModel
        {
            Subject = localizer["PasswordRecovery_Subject"], Eyebrow = localizer["PasswordRecovery_Eyebrow"],
            Title = localizer["PasswordRecovery_Title"], Hello = localizer["PasswordRecovery_Hello"],
            Introduction = localizer["PasswordRecovery_Introduction"], Instruction = localizer["PasswordRecovery_Instruction"],
            ButtonText = localizer["PasswordRecovery_Button"], SecurityNotice = localizer["PasswordRecovery_SecurityNotice"],
            Footer = localizer["PasswordRecovery_Footer"], FirstName = firstName, LastName = lastName, RecoveryLink = recoveryLink
        });
}
