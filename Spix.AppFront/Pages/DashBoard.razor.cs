using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Spix.DomainLogic.EntitiesDashboardDTO;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Globalization;
using System.Security.Claims;

namespace Spix.AppFront.Pages;

public partial class DashBoard
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
    [Inject] private IRepository Repository { get; set; } = null!;

    private List<string> CurrentRoles = new();
    private bool IsLoadingMetrics = true;
    private int ActiveContracts;
    private int SuspendedContracts;
    private decimal MonthTotal;
    private decimal MonthCollected;
    private decimal MonthBalance;

    private int ContractTotal => ActiveContracts + SuspendedContracts;
    private decimal ContractActivePercent => ContractTotal == 0 ? 0 : Math.Round((decimal)ActiveContracts / ContractTotal * 100, 0);
    private decimal ContractSuspendedPercent => ContractTotal == 0 ? 0 : 100 - ContractActivePercent;
    private decimal MonthCollectedPercent => MonthTotal == 0 ? 0 : Math.Round(MonthCollected / MonthTotal * 100, 0);
    private decimal MonthBalancePercent => MonthTotal == 0 ? 0 : 100 - MonthCollectedPercent;
    private string ContractChartStyle => $"--active:{FormatCssPercent(ContractActivePercent)}%; --suspended:{FormatCssPercent(ContractSuspendedPercent)}%;";
    private string BillingChartStyle => $"--collected:{FormatCssPercent(MonthCollectedPercent)}%; --pending:{FormatCssPercent(MonthBalancePercent)}%;";
    private string CurrentMonthName => CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(DateTime.Now.ToString("MMMM yyyy", CultureInfo.CurrentUICulture));

    protected override async Task OnInitializedAsync()
    {
        await LoadUserRoles();
        await LoadMetricsAsync();
    }

    private async Task LoadUserRoles()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        CurrentRoles = user.Claims
                           .Where(c => c.Type == ClaimTypes.Role)
                           .Select(c => c.Value)
                           .ToList();
    }

    private async Task LoadMetricsAsync()
    {
        IsLoadingMetrics = true;

        var responseHttp = await Repository.GetAsync<DashboardSummaryDto>("api/v1/dashboard/summary");
        if (!responseHttp.Error && responseHttp.Response is not null)
        {
            ActiveContracts = responseHttp.Response.ActiveContracts;
            SuspendedContracts = responseHttp.Response.SuspendedContracts;
            MonthTotal = responseHttp.Response.MonthTotal;
            MonthCollected = responseHttp.Response.MonthCollected;
            MonthBalance = responseHttp.Response.MonthBalance;
        }

        IsLoadingMetrics = false;
    }

    private static string FormatCssPercent(decimal value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string FormatMoney(decimal value) =>
        value.ToString("N2", CultureInfo.CurrentCulture);
}
