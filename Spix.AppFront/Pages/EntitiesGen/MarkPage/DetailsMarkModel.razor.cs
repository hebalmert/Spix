using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.MarkPage;

public partial class DetailsMarkModel
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private string Filter { get; set; } = string.Empty;

    private int CurrentPage = 1;  //Pagina seleccionada
    private int TotalPages;      //Cantidad total de paginas
    private int PageSize = 15;  //Cantidad de registros por pagina

    private const string baseUrl = "api/v1/marksmodels";

    public Mark? Mark { get; set; }
    public List<MarkModel>? MarkModels { get; set; }

    [Parameter] public Guid Id { get; set; }  //MarkId

    protected override async Task OnInitializedAsync()
    {
        await Cargar();
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

    private async Task ShowModalAsync(Guid? id = null, bool isEdit = false)
    {
        var options = new DialogOptions() { CloseOnEscapeKey = true, CloseButton = true };
        IDialogReference? dialog;
        if (isEdit)
        {
            var parameters = new DialogParameters
            {
                { "Id", id } //Manejamos el MarkModelId viene de razor
            };
            dialog = await _dialogService.ShowAsync<EditMarkModel>($"Editar Modelo", parameters, options);
        }
        else
        {
            var parameters = new DialogParameters
            {
                { "Id", Id }  //Manejamos el MarkId viene como Parametro
            };
            dialog = await _dialogService.ShowAsync<CreateMarkModel>($"Nuevo Modelo", parameters, options);
        }

        var result = await dialog.Result;
        if (result!.Canceled)
        {
            await Cargar();
        }
    }

    private async Task Cargar(int page = 1)
    {
        var url = $"{baseUrl}?guidId={Id}&page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }
        var responseHttp = await _repository.GetAsync<List<MarkModel>>(url);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/usuarios");
            return;
        }

        MarkModels = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);

        await LoadMark();
    }

    private async Task LoadMark()
    {
        var responseHTTP = await _repository.GetAsync<Mark>($"/api/v1/marks/{Id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"/usuarios");
            return;
        }
        Mark = responseHTTP.Response;
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Confirmación",
            Text = "¿Realmente deseas eliminar el registro?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            CancelButtonText = "No",
            ConfirmButtonText = "Si"
        });

        var confirm = string.IsNullOrEmpty(result.Value);
        if (confirm)
        {
            return;
        }

        var responseHttp = await _repository.DeleteAsync($"{baseUrl}/{id}");
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/usuarios");
            return;
        }

        await Cargar();
    }
}