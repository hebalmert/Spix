using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractSuspendedAuditPage;

public partial class IndexContractSuspendedAudit
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;

    private const string BaseUrl = "api/v1/contractsuspendedaudits";
    private DateTime StartDate { get; set; } = DateTime.Today.AddDays(-7);
    private DateTime EndDate { get; set; } = DateTime.Today;
    private List<ContractSuspendedAuditDTO> Audits { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    private async Task ChangeStartDateAsync(ChangeEventArgs e)
    {
        if (DateTime.TryParse(e.Value?.ToString(), out var startDate))
        {
            StartDate = startDate;
        }

        await LoadAsync();
    }

    private async Task ChangeEndDateAsync(ChangeEventArgs e)
    {
        if (DateTime.TryParse(e.Value?.ToString(), out var endDate))
        {
            EndDate = endDate;
        }

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var responseHttp = await _repository.GetAsync<List<ContractSuspendedAuditDTO>>(
            $"{BaseUrl}?startDate={StartDate:yyyy-MM-dd}&endDate={EndDate:yyyy-MM-dd}");

        if (!await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Audits = responseHttp.Response ?? new();
        }
    }

    private void GoToContractControl(Guid contractId)
    {
        _navigationManager.NavigateTo($"/detailscontractcontrol/{contractId}");
    }
}
