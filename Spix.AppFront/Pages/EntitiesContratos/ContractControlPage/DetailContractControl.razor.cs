using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.AppFront.Pages.EntitiesContratos.ContractClientPage;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using Spix.xLanguage.Resources;

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
    private string BaseUrl = "/api/v1/contractcontrols";
    private bool isLoading = false;
    private bool IsSaving = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadContractClient();
        }
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