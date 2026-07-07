using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesEmails;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesEmails.EmailProviderSettingPage;

public partial class CreateEmailProviderSetting
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private EmailProviderSetting EmailProvider = new()
    {
        Active = true,
        IsDefault = true,
        ProviderType = EmailProviderType.SendGrid,
        SmtpHost = "smtp.gmail.com",
        SmtpPort = 587,
        SmtpUseSsl = true
    };

    private const string BaseUrl = "/api/v1/emailproviders";
    private bool isLoading = false;
    private bool IsSaving = false;

    [Parameter] public string? Title { get; set; }

    private async Task Create()
    {
        IsSaving = true;
        var responseHttp = await _repository.PostAsync(BaseUrl, EmailProvider);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
