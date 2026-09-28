using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractPppoePage;

public partial class CreateContractPppoe
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private ContractPppoe ContractPppoe = new();
    private string BaseUrl = "/api/v1/contractpppoes";
    private bool isLoading = false;
    private bool IsSaving = false;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }
    [Parameter] public ContractServer? ContractServer { get; set; }
    [Parameter] public ContractIp? ContractIp { get; set; }

    //El usuario arranca propuesto con el numero de contrato: es unico y el operador lo
    //reconoce. El perfil sale del equipo, que es donde esta definido.
    [Parameter] public string? ControlContrato { get; set; }

    [Parameter] public string? ClientLastName { get; set; }

    [Parameter] public string? ProfileName { get; set; }

    protected override void OnInitialized()
    {
        ContractPppoe = new ContractPppoe
        {
            ContractClientId = Id,
            ServerId = ContractServer?.ServerId ?? Guid.Empty,
            IpNetId = ContractIp?.IpNetId ?? Guid.Empty,
            ServerName = ContractServer?.Server?.ServerName,
            IpServer = ContractServer?.Server?.IpNetwork?.Ip,
            IpCliente = ContractIp?.IpNet?.Ip,
            ProfileName = ProfileName,
            Usuario = FormContractPppoe.ProponerUsuario(ClientLastName, ControlContrato),
            Clave = Guid.NewGuid().ToString("N").Substring(0, 10)
        };
    }

    private async Task Create()
    {
        IsSaving = true;
        var responseHttp = await _repository.PostAsync(BaseUrl, ContractPppoe);
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
