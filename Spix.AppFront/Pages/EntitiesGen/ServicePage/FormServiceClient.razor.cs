using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Spix.AppFront.Pages.EntitiesGen.ServicePage;

public partial class FormServiceClient
{
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private Tax? SelectedTax;
    private List<Tax>? Taxes;

    [Parameter, EditorRequired] public ServiceClient ServiceClient { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadTaxes();
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
            SelectedTax = Taxes!.Where(x => x.TaxId == ServiceClient.TaxId)
                .Select(x => new Tax { TaxId = x.TaxId, TaxName = x.TaxName }).FirstOrDefault();
        }
    }

    private void TaxesChanged(Tax modelo)
    {
        ServiceClient.TaxId = modelo.TaxId;
        SelectedTax = modelo;
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