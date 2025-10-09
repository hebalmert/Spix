using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitesSoftSec;
using Spix.Domain.EntitiesGen;
using Spix.Domain.Enum;
using Spix.Domain.Resources;
using Spix.HttpService;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Spix.AppFront.Pages.EntitiesGen.PlanPage;

public partial class FormPlan
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private Tax? SelectedTax;
    private List<Tax>? Taxes;

    private IntItemModel? SelectedUserTypeUp;
    private List<IntItemModel>? ListUserTypeUp;

    private IntItemModel? SelectedUserTypeDown;
    private List<IntItemModel>? ListUserTypeDown;

    [Parameter, EditorRequired] public Plan Plan { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadTaxes();
        await LoadSpeedUp();
    }

    private async Task LoadSpeedUp()
    {
        var responseHTTP = await _repository.GetAsync<List<IntItemModel>>($"api/v1/plans/ComboUp");
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/plancategories");
            return;
        }
        ListUserTypeUp = responseHTTP.Response;
        ListUserTypeDown = responseHTTP.Response;

        if (IsEditControl == true)
        {
            SelectedUserTypeUp = ListUserTypeUp!.Where(x => x.Name == Plan.SpeedUpType.ToString()).FirstOrDefault();

            SelectedUserTypeDown = ListUserTypeDown!.Where(x => x.Name == Plan.SpeedDownType.ToString()).FirstOrDefault();
        }
    }

    private void UsertTypeUpChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e?.Value?.ToString(), out int modelo))
        {
            if (modelo == 1) { Plan.SpeedUpType = SpeedUpType.k; }
            if (modelo == 2) { Plan.SpeedUpType = SpeedUpType.M; }
            if (modelo == 3) { Plan.SpeedUpType = SpeedUpType.G; }
        }
    }

    private void UsertTypeDownChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e?.Value?.ToString(), out int modelo))
        {
            if (modelo == 1) { Plan.SpeedDownType = SpeedDownType.k; }
            if (modelo == 1) { Plan.SpeedDownType = SpeedDownType.M; }
            if (modelo == 1) { Plan.SpeedDownType = SpeedDownType.G; }
        }
    }

    private void TaxesChanged(ChangeEventArgs e)
    {
        if (e?.Value is Guid selectedId)
        {
            Plan.TaxId = selectedId;
        }
    }

    private async Task LoadTaxes()
    {
        var responseHTTP = await _repository.GetAsync<List<Tax>>($"api/v1/taxes/loadCombo");
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/products");
            return;
        }

        Taxes = responseHTTP.Response;
        if (IsEditControl == true)
        {
            SelectedTax = Taxes!.Where(x => x.TaxId == Plan.TaxId)
                .Select(x => new Tax { TaxId = x.TaxId, TaxName = x.TaxName }).FirstOrDefault();
        }
    }

    private string GetDisplayName<T>(Expression<Func<T>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            var property = memberExpression.Member as PropertyInfo;
            if (property != null)
            {
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null)
                {
                    return displayAttribute.Name!;
                }
            }
        }
        return "Texto no definido";
    }
}