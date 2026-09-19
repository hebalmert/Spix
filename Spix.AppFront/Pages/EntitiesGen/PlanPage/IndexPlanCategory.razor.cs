using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.PlanPage;

public partial class IndexPlanCategory
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private string Filter { get; set; } = string.Empty;

    private int CurrentPage = 1;  //Pagina seleccionada
    private int TotalPages;      //Cantidad total de paginas
    private int PageSize = 15;  //Cantidad de registros por pagina

    private const string baseUrl = "api/v1/plancategories";
    private const string baseUrlPlans = "api/v1/plans";

    public List<PlanCategory>? PlanCategories { get; set; }
    public Dictionary<Guid, List<Plan>> PlansByCategoryId { get; set; } = new();
    public Guid? SelectedPlanCategoryId { get; set; }
    public HashSet<Guid> LoadingPlanCategoryIds { get; set; } = new();

    //Filtros del panel de planes
    private string PlanChip { get; set; } = "all";
    private string PlanFilter { get; set; } = string.Empty;

    private PlanCategory? SelectedCategory =>
        PlanCategories?.FirstOrDefault(x => x.PlanCategoryId == SelectedPlanCategoryId);

    private List<Plan> SelectedPlans =>
        SelectedPlanCategoryId is not null && PlansByCategoryId.TryGetValue(SelectedPlanCategoryId.Value, out var plans)
            ? plans
            : new List<Plan>();

    private int ActivePlansCount => SelectedPlans.Count(x => x.Active);

    private int InactivePlansCount => SelectedPlans.Count(x => !x.Active);

    private List<Plan> FilteredPlans =>
        PlanChip switch
        {
            "active" => SelectedPlans.Where(x => x.Active).ToList(),
            "inactive" => SelectedPlans.Where(x => !x.Active).ToList(),
            _ => SelectedPlans
        };

    private void SetPlanChip(string chip) => PlanChip = chip;

    //El texto va al servidor: asi busca en TODOS los planes de la categoria, no solo en los cargados
    private async Task SetPlanFilter(string value)
    {
        PlanFilter = value;

        if (SelectedPlanCategoryId is not null)
        {
            await LoadPlansAsync(SelectedPlanCategoryId.Value);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Cargar();
        }
    }

    private async Task SelectCategoryAsync(Guid planCategoryId)
    {
        SelectedPlanCategoryId = planCategoryId;
        PlanChip = "all";
        PlanFilter = string.Empty;

        if (!PlansByCategoryId.ContainsKey(planCategoryId))
        {
            await LoadPlansAsync(planCategoryId);
        }
    }

    private async Task LoadPlansAsync(Guid planCategoryId)
    {
        LoadingPlanCategoryIds.Add(planCategoryId);
        await InvokeAsync(StateHasChanged);

        var url = $"{baseUrlPlans}?guidId={planCategoryId}&page=1&recordsnumber=100";
        if (!string.IsNullOrWhiteSpace(PlanFilter))
        {
            url += $"&filter={Uri.EscapeDataString(PlanFilter)}";
        }

        var responseHttp = await _repository.GetAsync<List<Plan>>(url);

        LoadingPlanCategoryIds.Remove(planCategoryId);

        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            return;
        }

        PlansByCategoryId[planCategoryId] = responseHttp.Response ?? new List<Plan>();

        await InvokeAsync(StateHasChanged);
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await Cargar(page);
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        CurrentPage = 1;
        await Cargar();
    }

    private async Task ShowModalAsync(Guid? id = null, bool isEdit = false)
    {
        Type component;
        Dictionary<string, object> parameters;
        if (isEdit)
        {
            component = typeof(EditPlanCategory);
            parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Edit_Category)]}"  }
            };
        }
        else
        {
            component = typeof(CreatePlanCategory);
            parameters = new Dictionary<string, object>
            {
                { "Title", $"{Localizer[nameof(Resource.Create_Category)]}"  }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
                await Cargar(CurrentPage);   //solo refresca si hubo cambios
        });
    }

    private void ShowModalDetailsAsync(Guid? id = null)
    {
        _navigationManager.NavigateTo($"/plans/details/{id}");
    }

    private async Task Cargar(int page = 1)
    {
        var url = $"{baseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }
        var responseHttp = await _repository.GetAsync<List<PlanCategory>>(url);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        PlanCategories = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);

        PlansByCategoryId.Clear();
        LoadingPlanCategoryIds.Clear();

        //Se conserva la categoria elegida si sigue en la lista; si no, se toma la primera
        var previous = SelectedPlanCategoryId;
        SelectedPlanCategoryId = PlanCategories?.Any(x => x.PlanCategoryId == previous) == true
            ? previous
            : PlanCategories?.FirstOrDefault()?.PlanCategoryId;

        await InvokeAsync(StateHasChanged);

        if (SelectedPlanCategoryId is not null)
        {
            await LoadPlansAsync(SelectedPlanCategoryId.Value);
        }
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{baseUrl}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);

        if (SelectedPlanCategoryId == id)
        {
            SelectedPlanCategoryId = null;
        }

        await Cargar(CurrentPage);
    }

    private async Task ShowModalPlanAsync(Guid planCategoryId, Guid? planId = null, bool isEdit = false)
    {
        Type component;
        Dictionary<string, object> parameters;

        if (isEdit)
        {
            component = typeof(EditPlan);
            parameters = new Dictionary<string, object>
            {
                { "Id", planId! },
                { "Title", $"{Localizer[nameof(Resource.Edit_Plan)]}" }
            };
        }
        else
        {
            component = typeof(CreatePlan);
            parameters = new Dictionary<string, object>
            {
                { "Id", planCategoryId },
                { "Title", $"{Localizer[nameof(Resource.Create_Plan)]}" }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
            {
                // Se recarga el hijo y tambien el padre, porque cambia el contador de planes
                SelectedPlanCategoryId = planCategoryId;
                await Cargar(CurrentPage);

                await _sweetAlert.FireAsync(
                    Localizer[nameof(Resource.msg_SuccessTitle)],
                    Localizer[nameof(Resource.msg_SuccessMessage)],
                    SweetAlertIcon.Success
                );
            }
        });
    }

    private async Task DeletePlanAsync(Guid planCategoryId, Guid planId)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{baseUrlPlans}/{planId}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);

        SelectedPlanCategoryId = planCategoryId;
        await Cargar(CurrentPage);
    }
}
