using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractAuditPage;

//Muestra la bitacora completa de un contrato. Lee un solo endpoint: lo que los modulos
//fueron anotando (creacion, estados, firma, suspension y exoneraciones).
public partial class ContractAuditModal
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private const string BaseUrl = "api/v1/contractaudit";

    [Parameter, EditorRequired] public Guid ContractClientId { get; set; }

    private ContractAuditListDTO? Data;
    private bool IsLoading = true;

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<ContractAuditListDTO>($"{BaseUrl}/{ContractClientId}");

        IsLoading = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        Data = responseHttp.Response;
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
