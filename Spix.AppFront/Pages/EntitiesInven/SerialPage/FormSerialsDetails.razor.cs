using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.Domain.Enum;
using Spix.HttpService;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Spix.AppFront.Pages.EntitiesInven.SerialPage;

public partial class FormSerialsDetails
{
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private IntItemModel? SelectedUserType;
    private List<IntItemModel>? ListUserType;

    [Parameter, EditorRequired] public CargueDetail CargueDetail { get; set; } = null!;
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadStates();
    }

    private async Task LoadStates()
    {
        var responseHTTP = await _repository.GetAsync<List<IntItemModel>>($"api/v1/cargues/loadComboStatus");
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/usuarios");
            return;
        }
        ListUserType = responseHTTP.Response;
        if (IsEditControl)
        {
            SelectedUserType = ListUserType!.Where(x => x.Name == CargueDetail.Status.ToString())
                .Select(x => new IntItemModel { Name = x.Name, Value = x.Value }).FirstOrDefault();
        }
        else
        {
            SelectedUserType = ListUserType!.Where(x => x.Name == SerialStateType.Disponible.ToString())
                .Select(x => new IntItemModel { Name = x.Name, Value = x.Value }).FirstOrDefault();
        }
    }

    private void UsertTypeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e?.Value?.ToString(), out int modelo))
        {
            if (modelo == 1) { CargueDetail.Status = SerialStateType.Disponible; }
            if (modelo == 2) { CargueDetail.Status = SerialStateType.Averiado; }
            if (modelo == 3) { CargueDetail.Status = SerialStateType.Operativo; }
        }
    }
}