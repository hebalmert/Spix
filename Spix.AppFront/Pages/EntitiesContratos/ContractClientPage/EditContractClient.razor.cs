using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractClientPage;

public partial class EditContractClient
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public Guid Id { get; set; }
    [Parameter] public string? Title { get; set; }

    private ContractClient? ContractClient;
    private string BaseUrl = "/api/v1/contractclients";
    private bool isLoading = false;
    private bool IsSaving = false;
    protected override async Task OnInitializedAsync()
    {
        isLoading = true;
        var responseHttp = await _repository.GetAsync<ContractClient>($"{BaseUrl}/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp)) return;
        ContractClient = responseHttp.Response;
        isLoading = false;
    }

    private async Task Edit()
    {
        IsSaving = true;
        var nModelo = new ContractClient
        {
            ContractClientId = ContractClient!.ContractClientId,
            DateCreado = ContractClient.DateCreado,
            ControlContrato = ContractClient.ControlContrato,
            ContractorId = ContractClient.ContractorId,
            ClientId = ContractClient.ClientId,
            PhoneNumber = ContractClient.PhoneNumber,
            PhoneNumber2 = ContractClient.PhoneNumber2,
            Address = ContractClient.Address,
            ZoneId = ContractClient.ZoneId,
            ContractState = ContractClient.ContractState,
            EquipoEmpres = ContractClient.EquipoEmpres,
            EnvoiceClient = ContractClient.EnvoiceClient,
            CorporationId = ContractClient.CorporationId
        };

        var responseHttp = await _repository.PutAsync($"{BaseUrl}", nModelo);
        IsSaving = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_UpdateSuccessTitle)], Localizer[nameof(Resource.msg_UpdateSuccessMessage)], SweetAlertIcon.Success);
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}