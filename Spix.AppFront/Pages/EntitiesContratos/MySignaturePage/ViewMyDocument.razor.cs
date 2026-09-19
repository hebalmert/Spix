using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.MySignaturePage;

public partial class ViewMyDocument
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter, EditorRequired] public Guid ContractClientId { get; set; }
    [Parameter, EditorRequired] public ContractDocumentType DocumentType { get; set; }
    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/mysignatures";

    private string? fileUrl;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<SignatureLinkDTO>($"{BaseUrl}/link/{ContractClientId}/{DocumentType}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            isLoading = false;
            return;
        }

        fileUrl = responseHttp.Response?.Url;
        isLoading = false;
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
