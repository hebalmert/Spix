using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractDocumentTemplatePage;

public partial class IndexContractDocumentTemplate
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private const string BaseUrl = "api/v1/contractdocuments/templates";
    private string Filter { get; set; } = string.Empty;
    private List<ContractDocumentTemplate>? Templates { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await LoadAsync();
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        CurrentPage = 1;
        await LoadAsync();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadAsync(page);
    }

    private async Task LoadAsync(int page = 1)
    {
        var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
            url += $"&filter={Uri.EscapeDataString(Filter)}";

        var responseHttp = await _repository.GetAsync<List<ContractDocumentTemplate>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        Templates = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowModalAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Title", "Nueva Plantilla PDF" }
        };

        await _modalService.ShowAsync(typeof(CreateContractDocumentTemplate), parameters, async result =>
        {
            if (result.Succeeded)
                await LoadAsync(CurrentPage);
        });
    }

    private async Task ShowEditAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", "Editar Plantilla PDF" }
        };

        await _modalService.ShowAsync(typeof(EditContractDocumentTemplate), parameters, async result =>
        {
            if (result.Succeeded)
                await LoadAsync(CurrentPage);
        });
    }

    private async Task ShowPdfAsync(string pdfUrl)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Title", "Plantilla PDF" },
            { "PdfUrl", pdfUrl }
        };

        await _modalService.ShowAsync(typeof(ViewContractPdf), parameters);
    }

    private async Task TestTemplateAsync(Guid id)
    {
        var responseHttp = await _repository.PostAsync<object, ContractDocumentTestDTO>($"{BaseUrl}/test/{id}", new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        if (string.IsNullOrWhiteSpace(responseHttp.Response?.FileFullPath))
            return;

        var parameters = new Dictionary<string, object>
        {
            { "Title", "Test Plantilla PDF" },
            { "PdfUrl", responseHttp.Response.FileFullPath }
        };

        await _modalService.ShowAsync(typeof(ViewContractPdf), parameters);
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Eliminar",
            Text = "Desea eliminar esta plantilla?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Eliminar",
            CancelButtonText = "Cancelar"
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{BaseUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Eliminado", "Registro eliminado correctamente.", SweetAlertIcon.Success);
        await LoadAsync(CurrentPage);
    }

    private static string GetDocumentTypeName(ContractDocumentType documentType) =>
        documentType == ContractDocumentType.Contract ? "Contrato" : "Consent Datos";
}
