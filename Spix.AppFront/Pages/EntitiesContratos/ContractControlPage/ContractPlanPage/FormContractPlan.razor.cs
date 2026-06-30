using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContractControlPage.ContractPlanPage;

public partial class FormContractPlan
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public ContractPlan ContractPlan { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private List<PlanCategory>? PlanCategories = new();
    private List<Plan>? Plans = new();
    private Guid PlanCategoryId;
    private string BaseView = "/contractcontrol";
    private string BaseComboPlansByCategory = "/api/v1/plans/loadComboByCategory";
    private string BaseComboPlanCategories = "/api/v1/plancategories/loadCombo";

    protected override async Task OnInitializedAsync()
    {
        await LoadPlanCategories();

        if (ContractPlan.PlanId != Guid.Empty)
        {
            await LoadSelectedPlanCategory();
        }

        await LoadPlans();
    }

    private async Task LoadPlanCategories()
    {
        var responseHttp = await _repository.GetAsync<List<PlanCategory>>(BaseComboPlanCategories);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        PlanCategories = responseHttp.Response;
    }

    private async Task LoadSelectedPlanCategory()
    {
        var responseHttp = await _repository.GetAsync<Plan>($"/api/v1/plans/{ContractPlan.PlanId}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        PlanCategoryId = responseHttp.Response?.PlanCategoryId ?? Guid.Empty;
    }

    private async Task LoadPlans()
    {
        var url = PlanCategoryId == Guid.Empty
            ? $"{BaseComboPlansByCategory}/{Guid.Empty}"
            : IsEditControl
                ? $"{BaseComboPlansByCategory}/{PlanCategoryId}/{ContractPlan.PlanId}"
                : $"{BaseComboPlansByCategory}/{PlanCategoryId}";

        var responseHttp = await _repository.GetAsync<List<Plan>>(url);
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }

        Plans = responseHttp.Response;
    }

    private async Task PlanCategoryChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var planCategoryId))
        {
            PlanCategoryId = planCategoryId;
            ContractPlan.PlanId = Guid.Empty;
            await LoadPlans();
        }
    }

    private void PlanChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e.Value?.ToString(), out var planId))
        {
            ContractPlan.PlanId = planId;
        }
    }
}
