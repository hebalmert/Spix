using Spix.xNotification.Templates;
using Microsoft.Extensions.Localization;

namespace Spix.AppService.ImplementEmails;

/// <summary>
/// Traduce los recursos del request actual y entrega los datos a las
/// plantillas neutrales almacenadas en xNotification.
/// </summary>
internal static class LocalizedEmailTemplateFactory
{
    public static string BuildAccountActivation(IStringLocalizer localizer, string? firstName, string? lastName, string? userName, string? temporaryPassword, string confirmationLink)
        => AccountActivationEmailTemplate.Build(new AccountActivationEmailTemplateModel
        {
            Subject = localizer["AccountActivation_Subject"],
            Title = localizer["AccountActivation_Title"],
            Hello = localizer["AccountActivation_Hello"],
            Welcome = localizer["AccountActivation_Welcome"],
            UserNameLabel = localizer["AccountActivation_UserName"],
            TemporaryPasswordLabel = localizer["AccountActivation_TemporaryPassword"],
            Instruction = localizer["AccountActivation_Instruction"],
            ButtonText = localizer["AccountActivation_Button"],
            SecurityNotice = localizer["AccountActivation_SecurityNotice"],
            Footer = localizer["AccountActivation_Footer"],
            FirstName = firstName,
            LastName = lastName,
            UserName = userName,
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

    public static string BuildSignatureRequest(IStringLocalizer localizer, string? firstName, string? lastName,
        string contractNumber, string documentsList, string signatureLink)
        => SignatureRequestEmailTemplate.Build(new SignatureRequestEmailTemplateModel
        {
            Subject = localizer["SignatureRequest_Subject"],
            Eyebrow = localizer["SignatureRequest_Eyebrow"],
            Title = localizer["SignatureRequest_Title"],
            Hello = localizer["SignatureRequest_Hello"],
            Introduction = localizer["SignatureRequest_Introduction"],
            Instruction = localizer["SignatureRequest_Instruction"],
            ButtonText = localizer["SignatureRequest_Button"],
            SecurityNotice = localizer["SignatureRequest_SecurityNotice"],
            Footer = localizer["SignatureRequest_Footer"],
            FirstName = firstName,
            LastName = lastName,
            ContractNumber = contractNumber,
            DocumentsList = documentsList,
            SignatureLink = signatureLink
        });

    public static string BuildSignatureCode(IStringLocalizer localizer, string? firstName, string? lastName,
        string code, int minutes)
        => SignatureCodeEmailTemplate.Build(new SignatureCodeEmailTemplateModel
        {
            Subject = localizer["SignatureCode_Subject"],
            Eyebrow = localizer["SignatureCode_Eyebrow"],
            Title = localizer["SignatureCode_Title"],
            Hello = localizer["SignatureCode_Hello"],
            Introduction = localizer["SignatureCode_Introduction"],
            Expiration = string.Format(localizer["SignatureCode_Expiration"], minutes),
            SecurityNotice = localizer["SignatureCode_SecurityNotice"],
            Footer = localizer["SignatureCode_Footer"],
            FirstName = firstName,
            LastName = lastName,
            Code = code
        });
}
