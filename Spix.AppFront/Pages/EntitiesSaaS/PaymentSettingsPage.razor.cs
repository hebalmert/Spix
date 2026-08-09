using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesSaaSDTO;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesSaaS;

public partial class PaymentSettingsPage
{
    private const string BaseUrl = "api/v1/payment-settings";

    [Inject]
    private IRepository _repository { get; set; } = null!;

    [Inject]
    private SweetAlertService _sweetAlert { get; set; } = null!;

    [Inject]
    private HttpResponseHandler _responseHandler { get; set; } = null!;

    private List<SecretSettingDTO>? Items { get; set; }

    private bool IsLoading { get; set; } = true;

    private bool IsSaving { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;

        var responseHttp = await _repository.GetAsync<List<SecretSettingDTO>>(BaseUrl);

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Items = new List<SecretSettingDTO>();
            IsLoading = false;
            return;
        }

        Items = responseHttp.Response ?? new List<SecretSettingDTO>();
        IsLoading = false;
    }

    private async Task SaveAsync()
    {
        if (IsSaving || Items is null)
        {
            return;
        }

        IsSaving = true;

        try
        {
            var responseHttp = await _repository.PostAsync<List<SecretSettingDTO>, bool>(
                BaseUrl,
                Items);

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                return;
            }

            await _sweetAlert.FireAsync(
                "Saved",
                "The payment configuration was encrypted and saved successfully.",
                SweetAlertIcon.Success);

            await LoadAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }
}
