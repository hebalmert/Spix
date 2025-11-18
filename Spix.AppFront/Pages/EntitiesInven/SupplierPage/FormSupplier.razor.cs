using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Spix.AppFront.Helper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Spix.AppFront.Pages.EntitiesInven.SupplierPage;

public partial class FormSupplier
{
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private DocumentType? SelectedDocument;
    private List<DocumentType>? DocumentTypes;
    private State? SelectedSate;
    private List<State>? States;
    private City? SelectedCity = new();
    private List<City>? Cities = new();
    private string? ImageUrl;
    private string BaseComboState = "/api/v1/states/loadCombo";
    private string BaseComboCity = "/api/v1/cities/loadCombo";
    private string BaseView = "/suppliers";
    private string BaseComboDocument = "/api/v1/documenttypes/loadCombo";

    [Parameter, EditorRequired] public Supplier Supplier { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadDocument();
        await LoadState();
        if (IsEditControl)
        {
            ImageUrl = Supplier.ImageFullPath;
        }
    }

    private async Task LoadDocument()
    {
        var responseHttp = await _repository.GetAsync<List<DocumentType>>($"{BaseComboDocument}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"{BaseView}");
            return;
        }
        DocumentTypes = responseHttp.Response;
        if (IsEditControl)
        {
            SelectedDocument = DocumentTypes!.Where(x => x.CorporationId == Supplier.CorporationId)
                .Select(x => new DocumentType { DocumentTypeId = x.DocumentTypeId, DocumentName = x.DocumentName })
                .FirstOrDefault();
        }
    }

    private void DocumentChanged(ChangeEventArgs e)
    {
        if (Guid.TryParse(e?.Value?.ToString(), out Guid selectedId))
        {
            Supplier.DocumentTypeId = selectedId;
        }
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
            SelectedSate = States!.Where(x => x.StateId == Supplier.StateId)
                .Select(x => new State { StateId = x.StateId, Name = x.Name })
                .FirstOrDefault();

            await LoadCity(SelectedSate!.StateId);
        }
    }

    private async Task StateChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e?.Value?.ToString(), out int selectedId))
        {
            Supplier.StateId = selectedId;
        }
        SelectedCity = new();
        Cities = new();
        await LoadCity(selectedId);
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
            SelectedCity = Cities!.Where(x => x.CityId == Supplier.CityId)
                .Select(x => new City { CityId = x.CityId, Name = x.Name })
                .FirstOrDefault();
        }
    }

    private void CityChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e?.Value?.ToString(), out int selectedId))
        {
            Supplier.CityId = selectedId;
        }
    }

    private void ImageSelected(string imagenBase64)
    {
        Supplier.ImgBase64 = imagenBase64;
        ImageUrl = null;
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