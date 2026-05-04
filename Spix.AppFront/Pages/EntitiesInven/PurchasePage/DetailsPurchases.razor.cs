using CurrieTechnologies.Razor.SweetAlert2;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesInven.PurchasePage;

public partial class DetailsPurchases
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private const string baseUrl = "/api/v1/purchaseDetails";

    public Purchase? Purchase { get; set; }
    public List<PurchaseDetail>? PurchaseDetails { get; set; }

    [Parameter] public Guid Id { get; set; }  //Codigo del PurchaseId
    [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;

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

    private async Task Cargar(int page = 1)
    {
        var url = $"{baseUrl}?GuidId={Id}&page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }
        var responseHttpCountry = await _repository.GetAsync<Purchase>($"/api/v1/purchases/{Id}");
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttpCountry);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        var responseHttp = await _repository.GetAsync<List<PurchaseDetail>>(url);
        // Centralizamos el manejo de errores
        bool errorHandled2 = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled2)
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);

        Purchase = responseHttpCountry.Response;
        PurchaseDetails = responseHttp.Response;

        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowModalAsync(Guid? id = null, bool isEdit = false)
    {
        Type component;
        Dictionary<string, object> parameters;
        if (isEdit)
        {
            component = typeof(EditPurchaseDetails);
            parameters = new Dictionary<string, object>
        {
            { "Id", id! },
            { "Title", $"{Localizer[nameof(Resource.Edit_Items)]}"  }
        };
        }
        else
        {
            component = typeof(CreatePurchaseDetails);
            parameters = new Dictionary<string, object>
        {
            { "Id", Id },
            { "Title", $"{Localizer[nameof(Resource.Create_Items)]}"  }
        };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await Cargar(CurrentPage);   //solo refresca si hubo cambios
        });
    }

    private async Task ClosePurchaseAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Desea Cerrar Compra",
            Text = "¿Al Cerrar la Compra, no podra volver editar y los Inventarios se actualizaran, Continuar?",
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

        var responseHttp = await _repository.PostAsync($"{baseUrl}/CerrarPurchase", Purchase);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        await Cargar(CurrentPage);
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
        await Cargar(CurrentPage);
    }
}