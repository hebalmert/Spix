using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.AppFront.Pages.Entities.SoftSecPage;
using Spix.Domain.EntitiesData;
using Spix.Domain.Resources;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesData.FrecuencyTypePage;

public partial class DetailFrecuency
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    [Parameter] public int Id { get; set; }

    private string Filter { get; set; } = string.Empty;

    private int CurrentPage = 1;  //Pagina seleccionada
    private int TotalPages;      //Cantidad total de paginas
    private int PageSize = 20;  //Cantidad de registros por pagina

    private const string baseUrl = "api/v1/frecuencies";
    public FrecuencyType? FrecuencyType { get; set; }
    public List<Frecuency>? Frecuencies { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await Cargar();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await Cargar(page);
    }

    private async Task ShowModalAsync(int id = 0, bool isEdit = false)
    {
        if (isEdit)
        {
            var parameters = new Dictionary<string, object>
            {
                { "Id", id },
                { "Title", $"{Localizer[nameof(Resource.Edit_Frecuency)]}"   }
            };
            await _modalService.ShowAsync<EditFrecuencyType>(parameters);
        }
        else
        {
            var parameters = new Dictionary<string, object>
            {
                { "Title",$"{Localizer[nameof(Resource.Create_Frecuency)]}"   }
            };
            await _modalService.ShowAsync<CreateFrecuencyType>(parameters);
        }
    }

    private async Task Cargar(int page = 1)
    {
        var url = $"{baseUrl}?id={Id}&page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }
        var responseHttp = await _repository.GetAsync<List<Frecuency>>(url);
        // Centralizamos el manejo de errores
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/usuarios");
            return;
        }

        Frecuencies = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);

        await LoadFrecuencyType();
    }

    private async Task LoadFrecuencyType()
    {
        var responseHTTP = await _repository.GetAsync<FrecuencyType>($"/api/v1/frecuencytypes/{Id}");
        bool errorHandler = await _responseHandler.HandleErrorAsync(responseHTTP);
        if (errorHandler)
        {
            _navigationManager.NavigateTo($"/usuarios");
            return;
        }
        FrecuencyType = responseHTTP.Response;
    }

    private async Task DeleteAsync(int id)
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
        await Cargar();
    }
}