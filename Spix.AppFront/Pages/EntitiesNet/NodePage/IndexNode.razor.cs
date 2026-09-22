using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractMapPage;
using Spix.DomainLogic.EntitiesNetDTO;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesNet.NodePage;

public partial class IndexNode
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private string Filter { get; set; } = string.Empty;

    private int CurrentPage = 1;  //Pagina seleccionada
    private int TotalPages;      //Cantidad total de paginas
    private int TotalRecords;   //Total de registros (header Counting)
    private int PageSize = 15;  //Cantidad de registros por pagina

    private const string baseUrl = "api/v1/nodes";
    public List<NodeListItemDto>? Nodes { get; set; }
    private NetSummaryDto? Summary;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ReloadAsync();
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
        var url = $"{baseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }
        var responseHttp = await _repository.GetAsync<List<NodeListItemDto>>(url);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        Nodes = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);

        //El conteo total lo manda el backend en el header Counting. Es informativo:
        //si no viene, la pantalla funciona igual.
        if (responseHttp.HttpResponseMessage.Headers.TryGetValues("Counting", out var counting) &&
            double.TryParse(counting.FirstOrDefault(), out var total))
        {
            TotalRecords = (int)total;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowModalAsync(Guid? id = null, bool isEdit = false)
    {
        Type component;
        Dictionary<string, object> parameters;
        if (isEdit)
        {
            component = typeof(EditNode);
            parameters = new Dictionary<string, object>
                {
                    { "Id", id! },
                    { "Title", $"{Localizer[nameof(Resource.Edit_Node)]}"  }
                };
        }
        else
        {
            component = typeof(CreateNode);
            parameters = new Dictionary<string, object>
                {
                    { "Title", $"{Localizer[nameof(Resource.Create_Node)]}"  }
                };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
            {
                await ReloadAsync();   // refresca la tabla
                await _sweetAlert.FireAsync(
                    Localizer[nameof(Resource.msg_SuccessTitle)],
                    Localizer[nameof(Resource.msg_SuccessMessage)],
                    SweetAlertIcon.Success
                );
            }
        });
    }

    private async Task ShowMapAsync(NodeListItemDto node)
    {
        if (!node.Latitude.HasValue || !node.Longitude.HasValue)
        {
            await _sweetAlert.FireAsync(Localizer["Map_Map"], Localizer["Map_NodeNoCoordinates"], SweetAlertIcon.Warning);
            return;
        }

        Type component;
        Dictionary<string, object> parameters;

        component = typeof(ViewContractMap);
        parameters = new Dictionary<string, object>
            {
                { "Latitude", node.Latitude },
                { "Longitude", node.Longitude },
                { "Title", node.NodesName ?? Localizer["Map_Map"] }
            };

        await _modalService.ShowAsync(component, parameters);
    }

    private async Task DeleteAsync(Guid id)
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

        var responseHttp = await _repository.DeleteAsync($"{baseUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await ReloadAsync();
    }

    //El tablero se cuenta sobre todos los equipos, no solo la pagina visible
    private async Task LoadSummaryAsync()
    {
        var responseHttp = await _repository.GetAsync<NetSummaryDto>($"{baseUrl}/summary");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Summary = responseHttp.Response;
    }

    //Despues de crear, editar o borrar: tabla y tablero. Paginar y buscar solo recargan la tabla.
    private async Task ReloadAsync()
    {
        await LoadSummaryAsync();
        await Cargar(CurrentPage);
    }
}
