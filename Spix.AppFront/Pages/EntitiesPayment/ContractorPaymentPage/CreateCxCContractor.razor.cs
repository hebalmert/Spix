using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesPayment.ContractorPaymentPage;

//Arma la cuenta por pagar de un contratista: se eligen sus comisiones pendientes,
//o se agregan todas de una vez.
public partial class CreateCxCContractor
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;

    [Parameter] public string? Title { get; set; }

    private const string BaseUrl = "api/v1/contractor-payments";

    private List<GuidItemModel>? Contractors;
    private List<ContractorPendingDto> Pending = new();
    private HashSet<Guid> Selected = new();
    private Guid ContractorId;
    private bool IsLoading;
    private bool IsSaving;

    private bool AllSelected => Pending.Count > 0 && Selected.Count == Pending.Count;

    private decimal Total => Pending
        .Where(x => Selected.Contains(x.ContractorAccountPayableId))
        .Sum(x => x.Total);

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await _repository.GetAsync<List<GuidItemModel>>($"{BaseUrl}/combocontractors");
        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Contractors = responseHttp.Response ?? new();
    }

    private async Task ContractorChanged(ChangeEventArgs e)
    {
        ContractorId = Guid.TryParse(e.Value?.ToString(), out var id) ? id : Guid.Empty;
        Pending.Clear();
        Selected.Clear();

        if (ContractorId == Guid.Empty)
            return;

        IsLoading = true;
        var responseHttp = await _repository.GetAsync<List<ContractorPendingDto>>($"{BaseUrl}/pending/{ContractorId}");
        IsLoading = false;

        if (!await _responseHandler.HandleErrorAsync(responseHttp))
            Pending = responseHttp.Response ?? new();
    }

    private void ToggleAll()
    {
        if (AllSelected)
        {
            Selected.Clear();
            return;
        }

        Selected = Pending.Select(x => x.ContractorAccountPayableId).ToHashSet();
    }

    private void ToggleOne(ContractorPendingDto item, ChangeEventArgs args)
    {
        if (args.Value is bool marcado && marcado)
        {
            Selected.Add(item.ContractorAccountPayableId);
            return;
        }

        Selected.Remove(item.ContractorAccountPayableId);
    }

    private async Task CreateAsync()
    {
        if (ContractorId == Guid.Empty || Selected.Count == 0)
            return;

        var model = new CxCContractorCreateDto
        {
            ContractorId = ContractorId,
            ContractorAccountPayableIds = Selected.ToList()
        };

        IsSaving = true;
        var responseHttp = await _repository.PostAsync($"{BaseUrl}/cxc", model);
        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
