using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractBindPage;

public partial class EditContractBind
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private ContractBind? ContractBind;
    private string BaseUrl = "/api/v1/contractbinds";
    private bool isLoading = false;
    private bool IsSaving = false;

    [Parameter] public ContractBind? Model { get; set; }
    [Parameter] public string? Title { get; set; }

    protected override void OnInitialized()
    {
        isLoading = true;
        if (Model is not null)
        {
            ContractBind = new ContractBind
            {
                ContractBindId = Model.ContractBindId,
                ContractClientId = Model.ContractClientId,
                ServerId = Model.ServerId,
                IpNetId = Model.IpNetId,
                CargueDetailId = Model.CargueDetailId,
                HotSpotTypeId = Model.HotSpotTypeId,
                MikrotikId = Model.MikrotikId,
                ServerName = Model.ServerName,
                IpServer = Model.IpServer,
                IpCliente = Model.IpCliente,
                MacCliente = Model.MacCliente
            };
        }
        isLoading = false;
    }

    private async Task Edit()
    {
        IsSaving = true;
        var responseHttp = await _repository.PutAsync(BaseUrl, ContractBind);
        IsSaving = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
