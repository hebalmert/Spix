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
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractOltPage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractPlanPage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractQuePage;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractServerPage;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesMK;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Net;

using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractPppoePage;
using Spix.Domain.EntitiesNet;

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
    private ContractOlt? ContractOlt { get; set; } = new();
    private ContractMap? ContractMap { get; set; } = new();
    private ContractQue? ContractQue { get; set; } = new();
    private ContractBind? ContractBind { get; set; } = new();
    private ContractPppoe? ContractPppoe { get; set; } = new();

    //Como trabaja el EQUIPO de este contrato. Antes era un bool que salia de la corporacion,
    //asi que PPPoE y Ninguno eran indistinguibles y la pantalla marcaba 6 de 6 completo sin
    //pedirle nada al equipo.
    private MikrotikControlType ControlMk { get; set; } = MikrotikControlType.Ninguno;

    //El perfil PPPoE del equipo: uno por servidor, creado al alistarlo
    private string? ServerPppProfile { get; set; }

    private bool UsaHotSpot => ControlMk == MikrotikControlType.HotSpot;
    private bool UsaPppoe => ControlMk == MikrotikControlType.PPPoE;
    private bool UsaControl => ControlMk != MikrotikControlType.Ninguno;

    private bool HasContractQue => ContractQue is not null && ContractQue.ContractQueId != Guid.Empty;
    private bool HasContractBind => ContractBind is not null && ContractBind.ContractBindId != Guid.Empty;
    private bool HasContractPppoe => ContractPppoe is not null && ContractPppoe.ContractPppoeId != Guid.Empty;

    //Las guardas de borrado de servidor, IP, plan y MAC cuelgan de esto: si el contrato ya
    //tiene piezas escritas en el equipo, no se le puede sacar lo que esas piezas usan.
    private bool HasHotSpotDependencies => HasContractQue || HasContractBind || HasContractPppoe;
    private bool CanActivateContract => ContractClient?.ContractState == ContractState.InProgress;

    //Estado de cada elemento para la vista (solo lectura; no cambia ninguna accion ni la activacion)
    private bool HasServer => ContractServer is not null && ContractServer.ContractServerId != Guid.Empty;
    private bool HasIp => ContractIp is not null && ContractIp.ContractIpId != Guid.Empty;
    private bool HasNode => ContractNode is not null && ContractNode.ContractNodeId != Guid.Empty;
    private bool HasOlt => ContractOlt is not null && ContractOlt.ContractOltId != Guid.Empty;
    private bool HasPlan => ContractPlan is not null && ContractPlan.ContractPlanId != Guid.Empty;
    private bool HasMac => ContractMac is not null && ContractMac.ContractMacId != Guid.Empty;
    private bool HasMap => ContractMap is not null && ContractMap.ContractMapId != Guid.Empty;

    //Progreso de configuracion: informativo, la activacion la sigue validando el servidor
    private List<string> MissingItems
    {
        get
        {
            var missing = new List<string>();
            if (!HasServer) missing.Add("Servidor Gateway");
            if (!HasIp) missing.Add("IP Cliente");
            if (!HasPlan) missing.Add("Plan Cliente");
            if (!HasMac) missing.Add("Mac Equipo");
            if (!HasMap) missing.Add("Ubicacion");
            if (!HasContractQue) missing.Add("Queue de Velocidad");

            //Por donde entra fisicamente: nodo si es inalambrico, OLT si es fibra. Lo decide
            //el servidor, asi que antes de elegirlo no se le pide ninguno de los dos.
            if (UsaHotSpot && !HasNode) missing.Add("Nodo Acceso");
            if (UsaPppoe && !HasOlt) missing.Add("OLT Acceso");

            if (UsaHotSpot && !HasContractBind) missing.Add("IpBinding Acceso");
            if (UsaPppoe && !HasContractPppoe) missing.Add("Credencial PPPoE");
            return missing;
        }
    }

    //Sin servidor son 6: servidor, IP, plan, MAC, ubicacion y queue. Al elegirlo entran
    //las dos piezas que dependen de su control: el acceso fisico y el acceso al servicio.
    private int TotalItems => UsaControl ? 8 : 6;
    private int DoneItems => TotalItems - MissingItems.Count;
    private int ProgressPercent => DoneItems * 100 / TotalItems;

    private static string StateClass(bool ok) => ok ? "is-ok" : "is-missing";
    private static string StateIcon(bool ok) => ok ? "fa fa-check" : "fa fa-circle-exclamation";
    private static string StateText(bool ok) => ok ? "Configurado" : "Falta";
    private static string CheckText(bool ok) => ok ? "✓" : "✗";

    private string BaseUrl = "/api/v1/contractcontrols";
    private string BaseContractIpUrl = "/api/v1/contractips";
    private string BaseContractMacUrl = "/api/v1/contractmacs";
    private string BaseContractServerUrl = "/api/v1/contractservers";
    private string BaseContractPppoeUrl = "/api/v1/contractpppoes";
    private string BaseServerUrl = "/api/v1/servers";
    private string BaseContractPlanUrl = "/api/v1/contractplans";
    private string BaseContractNodeUrl = "/api/v1/contractnodes";
    private string BaseContractOltUrl = "/api/v1/contractolts";
    private string BaseContractMapUrl = "/api/v1/contractmaps";
    private string BaseContractQueUrl = "/api/v1/contractques";
    private string BaseContractBindUrl = "/api/v1/contractbinds";
    private bool isLoading = false;
    private bool IsSaving = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadContractClient();

            //El servidor PRIMERO: de el sale como trabaja el equipo
            if (ContractClient!.ControlServerCount > 0)
            {
                await LoadContractServer(Id);
            }

            await LoadControlMk();

            if (ContractClient.ControlIpCount > 0)
            {
                await LoadContractip(Id);
            }
            if (ContractClient.ControlMacCount > 0)
            {
                await LoadContractMac(Id);
            }
            if (ContractClient.ControlPlanCount > 0)
            {
                await LoadContractPlan(Id);
            }
            if (ContractClient.ControlNodeCount > 0)
            {
                await LoadContractNode(Id);
            }
            if (ContractClient.ControlOltCount > 0)
            {
                await LoadContractOlt(Id);
            }
            if (ContractClient.ControlMapCount > 0)
            {
                await LoadContractMap(Id);
            }
            if (UsaControl)
            {
                await LoadContractQue(Id);
            }

            if (UsaHotSpot)
            {
                await LoadContractBind(Id);
            }

            if (UsaPppoe)
            {
                await LoadContractPppoe(Id);
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
            {
                await LoadContractServer(Id);

                //El servidor cambio, asi que el mecanismo puede haber cambiado: hay que
                //volver a preguntarselo al equipo o la tarjeta sigue mostrando la anterior.
                await LoadControlMk();
                await RecargarPiezasDeAccesoAsync();
            }
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

    private async Task ShowContractOltsAsyn(Guid? id)
    {
        Type component;
        Dictionary<string, object> parameters;

        component = typeof(CreateContractOlt);
        parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Olt)]}"  }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractOlt(Id);
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
                { "Title", $"{Localizer["Map_Title"]}" }
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
                { "Title", $"{Localizer["Map_EditTitle"]}" }
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
            await _sweetAlert.FireAsync(Localizer["Map_Title"], Localizer["Map_NoCoordinates"], SweetAlertIcon.Warning);
            return;
        }

        Type component;
        Dictionary<string, object> parameters;

        component = typeof(ViewContractMap);
        parameters = new Dictionary<string, object>
            {
                { "Latitude", model.Latitude },
                { "Longitude", model.Longitude },
                { "FirstLabel", $"{Localizer[nameof(Resource.Client)]}" },
                { "Title", $"{Localizer["Map_Title"]}" }
            };

        //El segundo punto es el equipo por donde entra: la OLT si es fibra, el nodo si no.
        if (UsaPppoe && ContractOlt?.Olt?.Latitude is not null && ContractOlt.Olt.Longitude is not null)
        {
            parameters.Add("SecondLatitude", ContractOlt.Olt.Latitude);
            parameters.Add("SecondLongitude", ContractOlt.Olt.Longitude);
            parameters.Add("SecondLabel", ContractOlt.Olt.OltName ?? Localizer[nameof(Resource.Olt)]);
        }
        else if (ContractNode?.Node?.Latitude is not null && ContractNode.Node.Longitude is not null)
        {
            parameters.Add("SecondLatitude", ContractNode.Node.Latitude);
            parameters.Add("SecondLongitude", ContractNode.Node.Longitude);
            parameters.Add("SecondLabel", ContractNode.Node.NodesName ?? Localizer["Map_Node"]);
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

    private async Task ShowContractPppoesAsyn(Guid? id)
    {
        Type component = typeof(CreateContractPppoe);

        var parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", "Credencial PPPoE" },
                { "ContractServer", ContractServer! },
                { "ContractIp", ContractIp! },
                { "ControlContrato", ContractClient is null ? string.Empty : ContractClient.ControlContrato.ToString() },
                { "ClientLastName", ContractClient?.Client?.LastName ?? string.Empty },
                { "ProfileName", ServerPppProfile ?? string.Empty }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractPppoe(Id);
        });
    }

    private async Task ShowContractPppoeEditAsync(ContractPppoe? model)
    {
        if (model is null || model.ContractPppoeId == Guid.Empty)
        {
            return;
        }

        Type component = typeof(EditContractPppoe);

        var parameters = new Dictionary<string, object>
            {
                { "Model", model },
                { "Title", "Editar Credencial PPPoE" },
                { "ControlContrato", ContractClient is null ? string.Empty : ContractClient.ControlContrato.ToString() },
                { "ClientLastName", ContractClient?.Client?.LastName ?? string.Empty }
            };

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await LoadContractPppoe(Id);
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
        ContractServer = new();
        await LoadContractServer(Id);
        await LoadControlMk();
        await RecargarPiezasDeAccesoAsync();
    }

    //Las piezas del grupo 3 dependen del mecanismo del equipo: al cambiar de servidor hay
    //que traer las del mecanismo nuevo y olvidar las del anterior.
    private async Task RecargarPiezasDeAccesoAsync()
    {
        ContractBind = new();
        ContractPppoe = new();

        if (UsaControl)
        {
            await LoadContractQue(Id);
        }

        if (UsaHotSpot)
        {
            await LoadContractBind(Id);
        }

        if (UsaPppoe)
        {
            await LoadContractPppoe(Id);
        }
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

    private async Task DeleteContractOltAsync(Guid id)
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

        var responseHttp = await _repository.DeleteAsync($"{BaseContractOltUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await LoadContractOlt(Id);
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

    private async Task DeleteContractPppoeAsync(Guid id)
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

        var responseHttp = await _repository.DeleteAsync($"{BaseContractPppoeUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        ContractPppoe = new();
        await LoadContractClient();
        await LoadContractPppoe(Id);
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

    //Cambio de estado del contrato: las opciones y las reglas las pone el backend
    private async Task ShowChangeStateAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "ContractClientId", Id },
            { "CurrentState", ContractClient!.ContractState }
        };

        await _modalService.ShowAsync(typeof(ChangeContractState), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await LoadContractClient();
                await _sweetAlert.FireAsync(Localizer["ContractState_ChangeTitle"], Localizer["ContractState_ChangeOk"], SweetAlertIcon.Success);
            }
        });
    }

    private async Task ActivateContractAsync()
    {
        var confirmation = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Activar contrato",
            Text = "Desea activar este contrato?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Activar",
            CancelButtonText = "Cancelar"
        });

        if (confirmation.IsDismissed || confirmation.Value != "true")
        {
            return;
        }

        IsSaving = true;
        var responseHttp = await _repository.PostAsync<object, ContractClient>(
            $"{BaseUrl}/{Id}/activate",
            new { });
        IsSaving = false;

        if (responseHttp.HttpResponseMessage?.StatusCode == HttpStatusCode.BadRequest)
        {
            var errorMessage = await responseHttp.GetErrorMessageAsync();
            if (errorMessage?.Contains("MikroTik HotSpot", StringComparison.OrdinalIgnoreCase) == true)
            {
                await _sweetAlert.FireAsync(
                    "Contrato pendiente de configuracion MikroTik",
                    "La corporacion maneja MikroTik HotSpot y el contrato debe tener Contract Queue e IpBinding antes de pasar a Active.",
                    SweetAlertIcon.Warning);
                return;
            }

            var mikrotikConnectionMessage = Localizer[nameof(Resource.Mikrotik_Connection_Error)].Value;
            if (string.Equals(errorMessage, mikrotikConnectionMessage, StringComparison.OrdinalIgnoreCase))
            {
                await _sweetAlert.FireAsync(
                    "No se pudo conectar con MikroTik",
                    "No fue posible activar el acceso remoto. El contrato continuara en InProgress.",
                    SweetAlertIcon.Warning);
                return;
            }
        }

        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        ContractClient = responseHttp.Response;
        await _sweetAlert.FireAsync(
            "Contrato activado",
            "El contrato fue activado correctamente.",
            SweetAlertIcon.Success);
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

    private async Task LoadContractOlt(Guid? id)
    {
        isLoading = true;
        var responseHTTP = await _repository.GetAsync<ContractOlt>($"{BaseContractOltUrl}/{Id}");
        isLoading = false;
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            ContractOlt = null;
            await InvokeAsync(StateHasChanged);
            return;
        }

        ContractOlt = responseHTTP.Response;

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

    //Como trabaja el equipo de este contrato. Se le pregunta al SERVIDOR, que es donde se
    //define: un equipo hace PPPoE o hace HotSpot, no las dos cosas.
    //
    //Sin servidor asignado no hay nada que preguntar: el contrato todavia no vive en ningun
    //equipo y el grupo 3 no se pinta.
    private async Task LoadControlMk()
    {
        ControlMk = MikrotikControlType.Ninguno;

        if (ContractServer is null || ContractServer.ServerId == Guid.Empty)
        {
            await InvokeAsync(StateHasChanged);
            return;
        }

        var responseHTTP = await _repository.GetAsync<Server>($"{BaseServerUrl}/{ContractServer.ServerId}");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            await InvokeAsync(StateHasChanged);
            return;
        }

        ControlMk = responseHTTP.Response?.ControlMk ?? MikrotikControlType.Ninguno;
        ServerPppProfile = responseHTTP.Response?.PppProfileName;

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadContractPppoe(Guid id)
    {
        var responseHTTP = await _repository.GetAsync<ContractPppoe>($"{BaseContractPppoeUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHTTP))
        {
            ContractPppoe = new();
            await InvokeAsync(StateHasChanged);
            return;
        }

        ContractPppoe = responseHTTP.Response ?? new();

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
