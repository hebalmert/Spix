using Microsoft.AspNetCore.Components;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractPppoePage;

public partial class EditContractPppoe
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private ContractPppoe? ContractPppoe;
    private string BaseUrl = "/api/v1/contractpppoes";
    private bool isLoading = false;
    private bool IsSaving = false;

    [Parameter] public ContractPppoe Model { get; set; } = null!;
    [Parameter] public string? Title { get; set; }

    //Para que el boton del usuario pueda proponer el numero del contrato, igual que al crear
    [Parameter] public string? ControlContrato { get; set; }

    [Parameter] public string? ClientLastName { get; set; }

    //Se trabaja sobre una copia: si el operador cancela, la pantalla de atras no queda
    //con los valores a medio editar.
    protected override void OnInitialized()
    {
        ContractPppoe = new ContractPppoe
        {
            ContractPppoeId = Model.ContractPppoeId,
            ContractClientId = Model.ContractClientId,
            ServerId = Model.ServerId,
            IpNetId = Model.IpNetId,
            Usuario = Model.Usuario,
            Clave = Model.Clave,
            MikrotikId = Model.MikrotikId,
            PppoeAccessState = Model.PppoeAccessState,
            ServerName = Model.ServerName,
            IpServer = Model.IpServer,
            IpCliente = Model.IpCliente,
            ProfileName = Model.ProfileName
        };
    }

    private async Task Edit()
    {
        IsSaving = true;
        var responseHttp = await _repository.PutAsync(BaseUrl, ContractPppoe);
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
