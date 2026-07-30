using Microsoft.AspNetCore.Components;
using Spix.DomainLogic.EntitiesDashboardDTO;
using Spix.HttpService;

namespace Spix.AppFront.Pages;

public partial class SaasDashboard
{
    [Inject] private IRepository Repository { get; set; } = null!;

    private bool IsLoading = true;
    private int TotalCorporations;
    private int ActiveCorporations;

    protected override async Task OnInitializedAsync()
    {
        var responseHttp = await Repository.GetAsync<SaasDashboardSummaryDto>("api/v1/dashboard/saas-summary");
        if (!responseHttp.Error && responseHttp.Response is not null)
        {
            TotalCorporations = responseHttp.Response.TotalCorporations;
            ActiveCorporations = responseHttp.Response.ActiveCorporations;
        }

        IsLoading = false;
    }
}
