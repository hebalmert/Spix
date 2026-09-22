using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
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

    private const string BaseUrl = "/api/v1/purchaseDetails";
    private const string PurchaseUrl = "/api/v1/purchases";

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;

    [Parameter] public Guid Id { get; set; }  //Codigo del PurchaseId

    public Purchase? Purchase { get; set; }
    public List<PurchaseDetail>? PurchaseDetails { get; set; }

    //Solo una compra abierta se modifica
    private bool IsOpen => Purchase?.Status == PurchaseStatus.Pendiente;

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

    //La cabecera trae sus renglones para los totales; la tabla va paginada aparte
    private async Task Cargar(int page = 1)
    {
        var responseHttpPurchase = await _repository.GetAsync<Purchase>($"{PurchaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttpPurchase))
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        var responseHttp = await _repository.GetAsync<List<PurchaseDetail>>($"{BaseUrl}?GuidId={Id}&page={page}&recordsnumber={PageSize}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/purchases");
            return;
        }

        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        Purchase = responseHttpPurchase.Response;
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
                { "Title", $"{Localizer[nameof(Resource.Edit_Items)]}" }
            };
        }
        else
        {
            component = typeof(CreatePurchaseDetails);
            parameters = new Dictionary<string, object>
            {
                { "Id", Id },
                { "Title", $"{Localizer[nameof(Resource.Create_Items)]}" }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await Cargar(CurrentPage);
        });
    }

    //Cerrar sube el stock y abre los cargues: se confirma antes, y el aviso de exito va despues
    private async Task ClosePurchaseAsync()
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["Purchase_CloseTitle"],
            Text = Localizer["Purchase_CloseText"],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer["Purchase_Close"],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.PostAsync($"{BaseUrl}/CerrarPurchase", Purchase);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await Cargar(CurrentPage);
            return;
        }

        await Cargar(CurrentPage);
        await _sweetAlert.FireAsync(Localizer["Purchase_Closed"], Localizer["Purchase_ClosedText"], SweetAlertIcon.Success);
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

        var responseHttp = await _repository.DeleteAsync($"{BaseUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await Cargar(CurrentPage);
    }
}
