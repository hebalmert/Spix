using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.DomainLogic.EntitiesSaaSDTO;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesSaaS;

public partial class SystemSettingsPage
{
    private const string BaseUrl = "api/v1/system-settings";

    [Inject]
    private IRepository _repository { get; set; } = null!;

    [Inject]
    private SweetAlertService _sweetAlert { get; set; } = null!;

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

        var response = await _repository.GetAsync<List<SecretSettingDTO>>(BaseUrl);

        Items = response.Error || response.Response is null
            ? new List<SecretSettingDTO>()
            : response.Response;

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
            var response = await _repository.PostAsync<List<SecretSettingDTO>, bool>(
                BaseUrl,
                Items);

            if (response.Error)
            {
                string? message = await response.GetErrorMessageAsync();

                await _sweetAlert.FireAsync(
                    "Unable to save",
                    message ?? "An unexpected error occurred.",
                    SweetAlertIcon.Error);

                return;
            }

            await _sweetAlert.FireAsync(
                "Saved",
                "The configuration was encrypted and saved successfully.",
                SweetAlertIcon.Success);

            await LoadAsync();
        }
        finally
        {
            IsSaving = false;
        }
    }
}
