using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
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
    private const string baseUrlServiceClients = "api/v1/serviceclients";

    public List<ServiceCategory>? ServiceCategories { get; set; }
    public Dictionary<Guid, List<ServiceClient>> ServiceClientsByCategoryId { get; set; } = new();
    public Guid? SelectedServiceCategoryId { get; set; }
    public HashSet<Guid> LoadingServiceCategoryIds { get; set; } = new();

    //Filtros del panel de servicios
    private string ServiceChip { get; set; } = "all";
    private string ServiceFilter { get; set; } = string.Empty;

    private ServiceCategory? SelectedCategory =>
        ServiceCategories?.FirstOrDefault(x => x.ServiceCategoryId == SelectedServiceCategoryId);

    private List<ServiceClient> SelectedServices =>
        SelectedServiceCategoryId is not null && ServiceClientsByCategoryId.TryGetValue(SelectedServiceCategoryId.Value, out var services)
            ? services
            : new List<ServiceClient>();

    private int ActiveServicesCount => SelectedServices.Count(x => x.Active);

    private int InactiveServicesCount => SelectedServices.Count(x => !x.Active);

    private List<ServiceClient> FilteredServices =>
        ServiceChip switch
        {
            "active" => SelectedServices.Where(x => x.Active).ToList(),
            "inactive" => SelectedServices.Where(x => !x.Active).ToList(),
            _ => SelectedServices
        };

    private void SetServiceChip(string chip) => ServiceChip = chip;

    //El texto va al servidor: asi busca en TODOS los servicios de la categoria, no solo en los cargados
    private async Task SetServiceFilter(string value)
    {
        ServiceFilter = value;

        if (SelectedServiceCategoryId is not null)
        {
            await LoadServiceClientsAsync(SelectedServiceCategoryId.Value);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Cargar();
        }
    }

    private async Task SelectCategoryAsync(Guid serviceCategoryId)
    {
        SelectedServiceCategoryId = serviceCategoryId;
        ServiceChip = "all";
        ServiceFilter = string.Empty;

        if (!ServiceClientsByCategoryId.ContainsKey(serviceCategoryId))
        {
            await LoadServiceClientsAsync(serviceCategoryId);
        }
    }

    private async Task LoadServiceClientsAsync(Guid serviceCategoryId)
    {
        LoadingServiceCategoryIds.Add(serviceCategoryId);
        await InvokeAsync(StateHasChanged);

        var url = $"{baseUrlServiceClients}?guidId={serviceCategoryId}&page=1&recordsnumber=100";
        if (!string.IsNullOrWhiteSpace(ServiceFilter))
        {
            url += $"&filter={Uri.EscapeDataString(ServiceFilter)}";
        }

        var responseHttp = await _repository.GetAsync<List<ServiceClient>>(url);

        LoadingServiceCategoryIds.Remove(serviceCategoryId);

        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            return;
        }

        ServiceClientsByCategoryId[serviceCategoryId] = responseHttp.Response ?? new List<ServiceClient>();

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
                await Cargar(CurrentPage);   //solo refresca si hubo cambios
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

        ServiceClientsByCategoryId.Clear();
        LoadingServiceCategoryIds.Clear();

        //Se conserva la categoria elegida si sigue en la lista; si no, se toma la primera
        var previous = SelectedServiceCategoryId;
        SelectedServiceCategoryId = ServiceCategories?.Any(x => x.ServiceCategoryId == previous) == true
            ? previous
            : ServiceCategories?.FirstOrDefault()?.ServiceCategoryId;

        await InvokeAsync(StateHasChanged);

        if (SelectedServiceCategoryId is not null)
        {
            await LoadServiceClientsAsync(SelectedServiceCategoryId.Value);
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

        if (SelectedServiceCategoryId == id)
        {
            SelectedServiceCategoryId = null;
        }

        await Cargar(CurrentPage);
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
                SelectedServiceCategoryId = serviceCategoryId;
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

        SelectedServiceCategoryId = serviceCategoryId;
        await Cargar(CurrentPage);
    }
}
