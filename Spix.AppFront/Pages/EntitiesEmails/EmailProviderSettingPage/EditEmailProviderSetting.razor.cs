using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesEmails;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesEmails.EmailProviderSettingPage;

public partial class EditEmailProviderSetting
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private EmailProviderSetting? EmailProvider;
    private const string BaseUrl = "/api/v1/emailproviders";
    private bool isLoading = false;
    private bool IsSaving = false;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        StateHasChanged();

        var responseHttp = await _repository.GetAsync<EmailProviderSetting>($"{BaseUrl}/{Id}");
        isLoading = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        EmailProvider = responseHttp.Response;
    }

    private async Task Edit()
    {
        IsSaving = true;
        var responseHttp = await _repository.PutAsync(BaseUrl, EmailProvider);
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
