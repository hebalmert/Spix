using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.Domain.Resources;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesGen.ProductPage;

public partial class IndexProductCategory
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
    private int PageSize = 15;  //Cantidad de registros por pagina

    private const string baseUrl = "api/v1/productcategories";
    public List<ProductCategory>? ProductCategories { get; set; }

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
                { "Id", id }
            };
            dialog = await _dialogService.ShowAsync<EditProductCategory>($"Editar Categoria", parameters, options);
        }
        else
        {
            dialog = await _dialogService.ShowAsync<CreateProductCategory>($"Nueva Categoria", options);
        }

        var result = await dialog.Result;
        if (result!.Canceled)
        {
            await Cargar();
        }
    }

    private async Task Cargar(int page = 1)
    {
        var url = $"{baseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }
        var responseHttp = await _repository.GetAsync<List<ProductCategory>>(url);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        ProductCategories = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
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