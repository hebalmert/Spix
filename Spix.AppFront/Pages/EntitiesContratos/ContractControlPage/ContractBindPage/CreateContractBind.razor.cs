using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractBindPage;

public partial class CreateContractBind
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private ContractBind ContractBind = new();
    private string BaseUrl = "/api/v1/contractbinds";
    private bool isLoading = false;
    private bool IsSaving = false;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public ContractServer? ContractServer { get; set; }
    [Parameter] public ContractIp? ContractIp { get; set; }
    [Parameter] public ContractMac? ContractMac { get; set; }

    protected override void OnInitialized()
    {
        ContractBind = new ContractBind
        {
            ContractClientId = Id,
            ServerId = ContractServer?.ServerId ?? Guid.Empty,
            IpNetId = ContractIp?.IpNetId ?? Guid.Empty,
            CargueDetailId = ContractMac?.CargueDetailId ?? Guid.Empty,
            ServerName = ContractServer?.Server?.ServerName,
            IpServer = ContractServer?.Server?.IpNetwork?.Ip,
            IpCliente = ContractIp?.IpNet?.Ip,
            MacCliente = ContractMac?.CargueDetail?.MacWlan
        };
    }

    private async Task Create()
    {
        IsSaving = true;
        var responseHttp = await _repository.PostAsync(BaseUrl, ContractBind);
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
