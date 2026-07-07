using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesEmails;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesEmails.EmailProviderSettingPage;

public partial class IndexEmailProviderSetting
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private string Filter { get; set; } = string.Empty;
    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private const string BaseUrl = "api/v1/emailproviders";

    public List<EmailProviderSetting>? EmailProviders { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Cargar();
        }
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await Cargar(page);
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await Cargar();
    }

    private async Task Cargar(int page = 1)
    {
        var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }

        var responseHttp = await _repository.GetAsync<List<EmailProviderSetting>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        EmailProviders = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);

        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowModalAsync(Guid? id = null, bool isEdit = false)
    {
        Type component = isEdit ? typeof(EditEmailProviderSetting) : typeof(CreateEmailProviderSetting);
        var parameters = new Dictionary<string, object>
        {
            { "Title", isEdit ? "Editar Setup Email" : "Nuevo Setup Email" }
        };

        if (isEdit)
        {
            parameters.Add("Id", id!);
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
            {
                await Cargar(CurrentPage);
                await _sweetAlert.FireAsync("Success", "Proceso realizado correctamente", SweetAlertIcon.Success);
            }
        });
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Eliminar",
            Text = "Desea eliminar este registro?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Delete",
            CancelButtonText = "Cancel"
        });

        if (result.IsDismissed || result.Value != "true")
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync($"{BaseUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await _sweetAlert.FireAsync("Deleted", "Registro eliminado correctamente", SweetAlertIcon.Success);
        await Cargar(CurrentPage);
    }

    private async Task TestEmailAsync(Guid id)
    {
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/{id}/email-test", new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await _sweetAlert.FireAsync("Email-test", "Correo de prueba enviado correctamente.", SweetAlertIcon.Success);
    }
}
