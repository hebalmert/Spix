using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Spix.AppFront.Pages.EntitiesGen.ZonePage;

public partial class FormZone
{
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter, EditorRequired] public Zone Zone { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }

    private State? SelectedSate;
    private List<State>? States;
    private City? SelectedCity = new();
    private List<City>? Cities = new();
    private string BaseView = "/corporations";
    private string BaseComboState = "/api/v1/states/loadCombo";
    private string BaseComboCity = "/api/v1/cities/loadCombo";

    protected override async Task OnInitializedAsync()
    {
        await LoadState();
    }

    private async Task LoadState()
    {
        var responseHttp = await _repository.GetAsync<List<State>>($"{BaseComboState}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        States = responseHttp.Response;
        if (IsEditControl)
        {
            SelectedSate = States!.Where(x => x.StateId == Zone.StateId)
                .Select(x => new State { StateId = x.StateId, Name = x.Name })
                .FirstOrDefault();

            await LoadCity(SelectedSate!.StateId);
        }
    }

    private async Task StateChanged(State modelo)
    {
        Zone.StateId = modelo.StateId;
        SelectedSate = modelo;
        SelectedCity = new();
        await LoadCity(modelo.StateId);
    }

    private async Task LoadCity(int id)
    {
        var responseHttp = await _repository.GetAsync<List<City>>($"{BaseComboCity}/{id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        Cities = responseHttp.Response;
        if (IsEditControl)
        {
            SelectedCity = Cities!.Where(x => x.CityId == Zone.CityId)
                .Select(x => new City { CityId = x.CityId, Name = x.Name })
                .FirstOrDefault();
        }
    }

    private void CityChanged(City modelo)
    {
        Zone.CityId = modelo.CityId;
        SelectedCity = modelo;
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