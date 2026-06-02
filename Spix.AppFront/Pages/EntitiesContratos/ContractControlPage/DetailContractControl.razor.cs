using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.AppFront.Pages.EntitiesContratos.ContractClientPage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractIpPage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractMacPage;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Security.AccessControl;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage;

public partial class DetailContractControl
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private ContractClient? ContractClient { get; set; }
    private ContractIp? ContractIp { get; set; } = new();
    private ContractMac? ContractMac { get; set; } = new();

    private string BaseUrl = "/api/v1/contractcontrols";
    private string BaseContractIpUrl = "/api/v1/contractips";
    private string BaseContractMacUrl = "/api/v1/contractmacs";
    private bool isLoading = false;
    private bool IsSaving = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadContractClient();
            if (ContractClient!.ControlIpCount > 0)
            {
                await LoadContractip(Id);
            }
        }
    }

    private async Task ShowContractIpsAsyn(Guid? id)
    {
        Type component;
        Dictionary<string, object> parameters;

        component = typeof(CreateContractIp);
        parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Create_Ip)]}"  }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractip(Id);   //solo refresca si hubo cambios
        });
    }

    private async Task ShowContractMacsAsyn(Guid? id)
    {
        Type component;
        Dictionary<string, object> parameters;

        component = typeof(CreateContractMac);
        parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Create_Mac)]}"  }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractip(Id);   //solo refresca si hubo cambios
        });
    }

    private async Task DeleteContractMacAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{BaseContractMacUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await LoadContractip(Id);
    }

    private async Task DeleteContractIpAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{BaseContractIpUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await LoadContractip(Id);
    }

    private async Task LoadContractip(Guid? id)
    {
        isLoading = true;
        var responseHTTP = await _repository.GetAsync<ContractIp>($"{BaseContractIpUrl}/{Id}");
        isLoading = false;
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            ContractIp = null;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ContractIp = responseHTTP.Response;

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadContractClient()
    {
        isLoading = true;
        var responseHTTP = await _repository.GetAsync<ContractClient>($"{BaseUrl}/{Id}");
        isLoading = false;
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"/contractcontrol");
            return;
        }
        ContractClient = responseHTTP.Response;

        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowEditContractClient(Guid? id = null)
    {
        Type component;
        Dictionary<string, object> parameters;

        component = typeof(EditContractClient);
        parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Edit_ContractClient)]}"  }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractClient();   //solo refresca si hubo cambios
        });
    }

}