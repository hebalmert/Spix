using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.TransferPage;

public partial class IndexTransfer
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private const string baseUrl = "api/v1/transfers";

    public List<Transfer>? Transfers { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Cargar();
        }
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await Cargar();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await Cargar(page);
    }

    //Las lineas del traslado viven en su propia pantalla
    private void ShowDetailsAsync(Guid id)
    {
        _navigationManager.NavigateTo($"/transfers/details/{id}");
    }

    //El historial se arma con la fila que ya esta en pantalla: no hace falta ir al servidor
    private async Task ShowAuditAsync(Transfer item)
    {
        var parameters = new Dictionary<string, object> { { "Transfer", item } };

        await _modalService.ShowAsync(typeof(AuditTransfer), parameters, null);
    }

    private async Task ShowModalAsync(Guid? id = null, bool isEdit = false)
    {
        Type component;
        Dictionary<string, object> parameters;

        if (isEdit)
        {
            component = typeof(EditTransfer);
            parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Edit_Transfer)]}" }
            };
        }
        else
        {
            component = typeof(CreateTransfer);
            parameters = new Dictionary<string, object>
            {
                { "Title", $"{Localizer[nameof(Resource.Create_Transfer)]}" }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
            {
                await Cargar();   //solo refresca si hubo cambios
            }
        });
    }

    private async Task Cargar(int page = 1)
    {
        var url = $"{baseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }

        var responseHttp = await _repository.GetAsync<List<Transfer>>(url);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            //Con la lista en null la tabla se queda en "Cargando..." para siempre. Si la
            //consulta fallo se deja vacia, que al menos dice la verdad.
            Transfers = new List<Transfer>();
            await InvokeAsync(StateHasChanged);
            return;
        }

        Transfers = responseHttp.Response ?? new List<Transfer>();

        //El header puede no venir si la respuesta no es la esperada: sin esto, un fallo al
        //leerlo dejaba la pantalla colgada aunque los datos ya hubieran llegado.
        TotalPages = responseHttp.HttpResponseMessage.Headers.TryGetValues("Totalpages", out var paginas) &&
                     int.TryParse(paginas.FirstOrDefault(), out var total)
            ? total
            : 1;

        await InvokeAsync(StateHasChanged);
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
        await Cargar();
    }
}
