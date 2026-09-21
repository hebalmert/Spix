using Spix.AppFront.Pages.EntitiesContratos.ContractAuditPage;
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.AppFront.Pages.EntitiesContratos.ContracIDPicPage;
using Spix.AppFront.Pages.EntitiesContratos.ContractDocumentTemplatePage;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractClientPage;

public partial class IndexContractClient
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private string Filter { get; set; } = string.Empty;
    private int StatusFilter;  //0 = todos; si no, el valor de ContractState

    private int CurrentPage = 1;  //Pagina seleccionada
    private int TotalPages;      //Cantidad total de paginas
    private int TotalRecords;   //Total de registros (header Counting)
    private int PageSize = 20;  //Cantidad de registros por pagina

    private const string baseUrl = "api/v1/contractclients";
    private const string baseUrlDocuments = "api/v1/contractdocuments";
    public List<ContractClient>? ContractClients { get; set; }

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
        if (StatusFilter > 0)
        {
            url += $"&id={StatusFilter}";
        }
        var responseHttp = await _repository.GetAsync<List<ContractClient>>(url);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        ContractClients = responseHttp.Response;
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

    private async Task StatusFilterChanged(ChangeEventArgs e)
    {
        StatusFilter = int.TryParse(e.Value?.ToString(), out var value) ? value : 0;
        CurrentPage = 1;
        await Cargar();
    }

    private static string GetEstratoNumber(string? estratoSocialName)
    {
        if (string.IsNullOrWhiteSpace(estratoSocialName))
        {
            return "-";
        }

        string[] values = estratoSocialName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return values.FirstOrDefault(value => int.TryParse(value, out _)) ?? "-";
    }

    private async Task ShowModalAsync(Guid? id = null, bool isEdit = false)
    {
        Type component;
        Dictionary<string, object> parameters;
        if (isEdit)
        {
            component = typeof(EditContractClient);
            parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Edit_ContractClient)]}" }
            };
        }
        else
        {
            component = typeof(CreateContractClient);
            parameters = new Dictionary<string, object>
            {
                { "Title", $"{Localizer[nameof(Resource.Create_ContractClient)]}" }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await Cargar(CurrentPage);   //solo refresca si hubo cambios
        });
    }

    private async Task ShowModalidpicsAsync(Guid id, Guid? idPic = null)
    {
        Type component;
        Dictionary<string, object> parameters;
        if (idPic != null)
        {
            component = typeof(EditContractIDPic);
            parameters = new Dictionary<string, object>
            {
                { "Id", idPic! },
                { "Title", $"{Localizer[nameof(Resource.Picture_ID)]}" }
            };
        }
        else
        {
            component = typeof(CreateContractIDPic);
            parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Picture_ID)]}" }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await Cargar(CurrentPage);   //solo refresca si hubo cambios
        });
    }

    private async Task ShowContractDocumentAsync(Guid id, ContractDocumentType documentType)
    {
        var title = documentType == ContractDocumentType.Contract ? "Contrato" : "Consentimiento";
        var parameters = new Dictionary<string, object>
        {
            { "ContractClientId", id },
            { "DocumentType", documentType },
            { "Title", title }
        };

        await _modalService.ShowAsync(typeof(SignContractDocument), parameters, async result =>
        {
            if (result.Succeeded)
                await Cargar(CurrentPage);
        });
    }

    private async Task ApproveAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Aprobar contrato",
            Text = "Ya tiene fotos del documento, Consentimiento y Contrato firmados. Desea pasarlo a In Progress?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Aprobar",
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.PutAsync($"{baseUrl}/approve/{id}", new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Aprobado", "El contrato paso a In Progress.", SweetAlertIcon.Success);
        await Cargar(CurrentPage);
    }

    //Envia al cliente el correo con la solicitud de firma (ver docs/Firma-Electronica-Part11.md)
    private async Task SendSignatureRequestAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = "Enviar solicitud de firma",
            Text = "Se enviara al correo del cliente el enlace para entrar y firmar sus documentos. Desea enviarlo?",
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = "Enviar",
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.PostAsync($"{baseUrlDocuments}/request-signature/{id}", new { });
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync("Enviado", "El cliente recibio la solicitud de firma en su correo.", SweetAlertIcon.Success);
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

    //Rastro del contrato: la bitacora completa, no solo lo que guarda esta tabla.
    //Creacion, estados, firma, suspension y exoneraciones salen de ContractAudit.
    private async Task ShowAuditAsync(ContractClient item)
    {
        var parametros = new Dictionary<string, object>
        {
            { "ContractClientId", item.ContractClientId }
        };

        await _modalService.ShowAsync(typeof(ContractAuditModal), parametros);
    }
}
