using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.AppFront.Pages.EntitiesGen.DocumentTypePage;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.ServicePage;

public partial class IndexServiceCategory
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

    private const string baseUrl = "api/v1/servicecategories";
    public List<ServiceCategory>? ServiceCategories { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Cargar();
        }
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await Cargar(page);
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await Cargar();
    }

    private async Task ShowModalAsync(Guid? id = null, bool isEdit = false)
    {
        Type component;
        Dictionary<string, object> parameters;
        if (isEdit)
        {
            component = typeof(EditServiceCategory);
            parameters = new Dictionary<string, object>
        {
            { "Id", id! },
            { "Title", $"{Localizer[nameof(Resource.Edit_Service)]}"  }
        };
        }
        else
        {
            component = typeof(CreateServiceCategory);
            parameters = new Dictionary<string, object>
        {
            { "Title", $"{Localizer[nameof(Resource.Create_Service)]}"  }
        };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
            {
                await Cargar(CurrentPage);   // refresca la tabla
                await _sweetAlert.FireAsync(
                    Localizer[nameof(Resource.msg_SuccessTitle)],
                    Localizer[nameof(Resource.msg_SuccessMessage)],
                    SweetAlertIcon.Success
                );
            }
        });
    }

    private void ShowModalDetailsAsync(Guid? id = null)
    {
        _navigationManager.NavigateTo($"/serviceclients/details/{id}");
    }

    private async Task Cargar(int page = 1)
    {
        var url = $"{baseUrl}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }
        var responseHttp = await _repository.GetAsync<List<ServiceCategory>>(url);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        ServiceCategories = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);

        await InvokeAsync(StateHasChanged);
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
        await Cargar(CurrentPage);
    }

    // ===================== Acordeon: servicios de cada categoria =====================
    // Mismo patron que ProductCategory: se cargan bajo demanda y se cachean por categoria,
    // asi abrir y cerrar una fila no vuelve a pegarle al API.
    private const string baseUrlServiceClients = "api/v1/serviceclients";

    public Guid? ExpandedServiceCategoryId { get; set; }

    public HashSet<Guid> LoadingServiceCategoryIds { get; set; } = new();

    public Dictionary<Guid, List<ServiceClient>> ServiceClientsByCategoryId { get; set; } = new();

    private async Task ToggleExpandedServiceCategory(Guid serviceCategoryId)
    {
        if (ExpandedServiceCategoryId == serviceCategoryId)
        {
            ExpandedServiceCategoryId = null;
            return;
        }

        ExpandedServiceCategoryId = serviceCategoryId;

        if (!ServiceClientsByCategoryId.ContainsKey(serviceCategoryId))
        {
            await LoadServiceClientsAsync(serviceCategoryId);
        }
    }

    private async Task LoadServiceClientsAsync(Guid serviceCategoryId)
    {
        LoadingServiceCategoryIds.Add(serviceCategoryId);
        await InvokeAsync(StateHasChanged);

        var responseHttp = await _repository.GetAsync<List<ServiceClient>>($"{baseUrlServiceClients}?guidId={serviceCategoryId}&page=1&recordsnumber=100");

        LoadingServiceCategoryIds.Remove(serviceCategoryId);

        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            return;
        }

        ServiceClientsByCategoryId[serviceCategoryId] = responseHttp.Response ?? new List<ServiceClient>();

        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowModalServiceClientAsync(Guid serviceCategoryId, Guid? serviceClientId = null, bool isEdit = false)
    {
        Type component;
        Dictionary<string, object> parameters;

        if (isEdit)
        {
            component = typeof(EditServiceClient);
            parameters = new Dictionary<string, object>
            {
                { "Id", serviceClientId! },
                { "Title", $"{Localizer[nameof(Resource.Edit_Service)]}" }
            };
        }
        else
        {
            component = typeof(CreateServiceClient);
            parameters = new Dictionary<string, object>
            {
                { "Id", serviceCategoryId },
                { "Title", $"{Localizer[nameof(Resource.Create_Service)]}" }
            };
        }

        await _modalService.ShowAsync(component, parameters, async result =>
        {
            if (result.Succeeded)
            {
                // Se recarga el hijo y tambien el padre, porque cambia el contador de servicios
                await LoadServiceClientsAsync(serviceCategoryId);
                await Cargar(CurrentPage);

                await _sweetAlert.FireAsync(
                    Localizer[nameof(Resource.msg_SuccessTitle)],
                    Localizer[nameof(Resource.msg_SuccessMessage)],
                    SweetAlertIcon.Success
                );
            }
        });
    }

    private async Task DeleteServiceClientAsync(Guid serviceCategoryId, Guid serviceClientId)
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

        var responseHttp = await _repository.DeleteAsync($"{baseUrlServiceClients}/{serviceClientId}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);

        await LoadServiceClientsAsync(serviceCategoryId);
        await Cargar(CurrentPage);
    }
}
