using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.AppFront.Pages.EntitiesContratos.ContractClientPage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractIpPage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractMacPage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractBindPage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractMapPage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractNodePage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractPlanPage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractQuePage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractServerPage;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.EnumTypes;
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
    private ContractIp? ContractIp { get; set; } = new();
    private ContractMac? ContractMac { get; set; } = new();
    private ContractServer? ContractServer { get; set; } = new();
    private ContractPlan? ContractPlan { get; set; } = new();
    private ContractNode? ContractNode { get; set; } = new();
    private ContractMap? ContractMap { get; set; } = new();
    private ContractQue? ContractQue { get; set; } = new();
    private ContractBind? ContractBind { get; set; } = new();
    private bool UseHotSpotControl { get; set; }
    private bool HasContractQue => ContractQue is not null && ContractQue.ContractQueId != Guid.Empty;
    private bool HasContractBind => ContractBind is not null && ContractBind.ContractBindId != Guid.Empty;
    private bool HasHotSpotDependencies => HasContractQue || HasContractBind;

    private string BaseUrl = "/api/v1/contractcontrols";
    private string BaseContractIpUrl = "/api/v1/contractips";
    private string BaseContractMacUrl = "/api/v1/contractmacs";
    private string BaseContractServerUrl = "/api/v1/contractservers";
    private string BaseContractPlanUrl = "/api/v1/contractplans";
    private string BaseContractNodeUrl = "/api/v1/contractnodes";
    private string BaseContractMapUrl = "/api/v1/contractmaps";
    private string BaseContractQueUrl = "/api/v1/contractques";
    private string BaseContractBindUrl = "/api/v1/contractbinds";
    private string BaseConnectionMikrotikControlUrl = "/api/v1/connectionmikrotikcontrols";
    private bool isLoading = false;
    private bool IsSaving = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadContractClient();
            await LoadConnectionMikrotikControl();
            if (ContractClient!.ControlIpCount > 0)
            {
                await LoadContractip(Id);
            }
            if (ContractClient.ControlMacCount > 0)
            {
                await LoadContractMac(Id);
            }
            if (ContractClient.ControlServerCount > 0)
            {
                await LoadContractServer(Id);
            }
            if (ContractClient.ControlPlanCount > 0)
            {
                await LoadContractPlan(Id);
            }
            if (ContractClient.ControlNodeCount > 0)
            {
                await LoadContractNode(Id);
            }
            if (ContractClient.ControlMapCount > 0)
            {
                await LoadContractMap(Id);
            }
            if (UseHotSpotControl)
            {
                await LoadContractQue(Id);
                await LoadContractBind(Id);
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
                await LoadContractMac(Id);   //solo refresca si hubo cambios
        });
    }

    private async Task ShowContractServersAsyn(Guid? id)
    {
        Type component;
        Dictionary<string, object> parameters;

        component = typeof(CreateContractServer);
        parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Create_Server)]}"  }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractServer(Id);
        });
    }

    private async Task ShowContractPlansAsyn(Guid? id)
    {
        Type component;
        Dictionary<string, object> parameters;

        component = typeof(CreateContractPlan);
        parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.ClientPlan)]}"  }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractPlan(Id);
        });
    }

    private async Task ShowContractNodesAsyn(Guid? id)
    {
        Type component;
        Dictionary<string, object> parameters;

        component = typeof(CreateContractNode);
        parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.ClientAP)]}"  }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractNode(Id);
        });
    }

    private async Task ShowContractMapAsyn(Guid? id)
    {
        Type component;
        Dictionary<string, object> parameters;

        component = typeof(CreateContractMap);
        parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", "Client Map" }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractMap(Id);
        });
    }

    private async Task ShowContractMapEditAsync(ContractMap? model)
    {
        if (model is null || model.ContractMapId == Guid.Empty)
        {
            return;
        }

        Type component;
        Dictionary<string, object> parameters;

        component = typeof(CreateContractMap);
        parameters = new Dictionary<string, object>
            {
                { "Model", model },
                { "Title", "Editar Client Map" }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractMap(Id);
        });
    }

    private async Task ShowContractMapViewAsync(ContractMap? model)
    {
        if (model is null || !model.Latitude.HasValue || !model.Longitude.HasValue)
        {
            await _sweetAlert.FireAsync("Client Map", "No hay coordenadas para mostrar.", SweetAlertIcon.Warning);
            return;
        }

        Type component;
        Dictionary<string, object> parameters;

        component = typeof(ViewContractMap);
        parameters = new Dictionary<string, object>
            {
                { "Latitude", model.Latitude },
                { "Longitude", model.Longitude },
                { "FirstLabel", "Cliente" },
                { "Title", "Client Map" }
            };

        if (ContractNode?.Node?.Latitude is not null && ContractNode.Node.Longitude is not null)
        {
            parameters.Add("SecondLatitude", ContractNode.Node.Latitude);
            parameters.Add("SecondLongitude", ContractNode.Node.Longitude);
            parameters.Add("SecondLabel", ContractNode.Node.NodesName ?? "Nodo");
        }

        await _modalService.ShowAsync(component, parameters);
    }

    private async Task ShowContractQuesAsyn(Guid? id)
    {
        Type component;
        Dictionary<string, object> parameters;

        component = typeof(CreateContractQue);
        parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", "Queues Velocidad" },
                { "ContractServer", ContractServer! },
                { "ContractIp", ContractIp! },
                { "ContractPlan", ContractPlan! }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractQue(Id);
        });
    }

    private async Task ShowContractBindsAsyn(Guid? id)
    {
        Type component;
        Dictionary<string, object> parameters;

        component = typeof(CreateContractBind);
        parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", "IpBinding Acceso" },
                { "ContractServer", ContractServer! },
                { "ContractIp", ContractIp! },
                { "ContractMac", ContractMac! }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractBind(Id);
        });
    }

    private async Task ShowContractBindEditAsync(ContractBind? model)
    {
        if (model is null || model.ContractBindId == Guid.Empty)
        {
            return;
        }

        Type component;
        Dictionary<string, object> parameters;

        component = typeof(EditContractBind);
        parameters = new Dictionary<string, object>
            {
                { "Model", model },
                { "Title", "Editar IpBinding Acceso" }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractBind(Id);
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
        await LoadContractMac(Id);
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

    private async Task DeleteContractServerAsync(Guid id)
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

        var responseHttp = await _repository.DeleteAsync($"{BaseContractServerUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await LoadContractServer(Id);
    }

    private async Task DeleteContractPlanAsync(Guid id)
    {
        if (HasHotSpotDependencies)
        {
            await _sweetAlert.FireAsync(
                "Plan Cliente",
                "Debe eliminar Queues Velocidad e IpBinding Acceso antes de cambiar el Plan Cliente.",
                SweetAlertIcon.Warning);
            return;
        }

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

        var responseHttp = await _repository.DeleteAsync($"{BaseContractPlanUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await LoadContractPlan(Id);
    }

    private async Task DeleteContractNodeAsync(Guid id)
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

        var responseHttp = await _repository.DeleteAsync($"{BaseContractNodeUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await LoadContractNode(Id);
    }

    private async Task DeleteContractMapAsync(Guid id)
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

        var responseHttp = await _repository.DeleteAsync($"{BaseContractMapUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await LoadContractMap(Id);
    }

    private async Task DeleteContractQueAsync(Guid id)
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

        var responseHttp = await _repository.DeleteAsync($"{BaseContractQueUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await LoadContractQue(Id);
    }

    private async Task DeleteContractBindAsync(Guid id)
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

        var responseHttp = await _repository.DeleteAsync($"{BaseContractBindUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await LoadContractBind(Id);
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

    private async Task LoadContractMac(Guid? id)
    {
        isLoading = true;
        var responseHTTP = await _repository.GetAsync<ContractMac>($"{BaseContractMacUrl}/{Id}");
        isLoading = false;
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            ContractMac = null;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ContractMac = responseHTTP.Response;

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadContractServer(Guid? id)
    {
        isLoading = true;
        var responseHTTP = await _repository.GetAsync<ContractServer>($"{BaseContractServerUrl}/{Id}");
        isLoading = false;
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            ContractServer = null;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ContractServer = responseHTTP.Response;

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadContractPlan(Guid? id)
    {
        isLoading = true;
        var responseHTTP = await _repository.GetAsync<ContractPlan>($"{BaseContractPlanUrl}/{Id}");
        isLoading = false;
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            ContractPlan = null;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ContractPlan = responseHTTP.Response;

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadContractNode(Guid? id)
    {
        isLoading = true;
        var responseHTTP = await _repository.GetAsync<ContractNode>($"{BaseContractNodeUrl}/{Id}");
        isLoading = false;
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            ContractNode = null;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ContractNode = responseHTTP.Response;

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadContractMap(Guid? id)
    {
        isLoading = true;
        var responseHTTP = await _repository.GetAsync<ContractMap>($"{BaseContractMapUrl}/{Id}");
        isLoading = false;
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            ContractMap = null;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ContractMap = responseHTTP.Response;

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadContractQue(Guid? id)
    {
        isLoading = true;
        var responseHTTP = await _repository.GetAsync<ContractQue>($"{BaseContractQueUrl}/{Id}");
        isLoading = false;
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            ContractQue = null;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ContractQue = responseHTTP.Response;

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadContractBind(Guid? id)
    {
        isLoading = true;
        var responseHTTP = await _repository.GetAsync<ContractBind>($"{BaseContractBindUrl}/{Id}");
        isLoading = false;
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            ContractBind = null;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ContractBind = responseHTTP.Response;

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadConnectionMikrotikControl()
    {
        var responseHTTP = await _repository.GetAsync<List<ConnectionMikrotikControl>>($"{BaseConnectionMikrotikControlUrl}?page=1&recordsnumber=1");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            UseHotSpotControl = false;
            await InvokeAsync(StateHasChanged);
            return;
        }

        var connectionControl = responseHTTP.Response?.FirstOrDefault();
        UseHotSpotControl = connectionControl?.MikrotikControlType == MikrotikControlType.HotSpot;

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
