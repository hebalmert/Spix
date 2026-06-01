using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractIpPage;

public partial class CreateContractIp
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    private ContractIp ContractIp = new();

    private string BaseUrl = "/api/v1/contractips";
    private bool isLoading = false;
    private bool IsSaving = false;
    [Parameter] public Guid Id { get; set; }  //ContractiClientId
    [Parameter] public string? Title { get; set; }

    private async Task Create()
    {
        IsSaving = true;
        ContractIp.ContractClientId = Id;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}", ContractIp);
        IsSaving = false;
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}