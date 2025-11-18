using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModal;
using Spix.AppFront.Helper;
using Spix.AppFront.Pages.EntitiesInven.CarguePage;
using Spix.Domain.EntitiesInven;
using Spix.Domain.Resources;
using Spix.HttpService;

namespace Spix.AppFront.Pages.EntitiesInven.SerialPage;

public partial class IndexSerials
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _SweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private const string baseUrl = "/api/v1/cargueDetails/GetSerials";

    public List<CargueDetail>? CargueDetails { get; set; }

    [Parameter] public Guid Id { get; set; }  //Codigo del CargueId
    private string Filter { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await Cargar();
    }

    private async Task SetFilterValue(string value)
    {
        Filter = value;
        await Cargar();
    }

    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await Cargar(page);
    }

    private async Task Cargar(int page = 1)
    {
        var url = $"{baseUrl}?guidId={Id}&page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }

        var responseHttp = await _repository.GetAsync<List<CargueDetail>>(url);
        // Centralizamos el manejo de errores
        bool errorHandled2 = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled2)
        {
            _navigationManager.NavigateTo("/transfers");
            return;
        }

        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);

        CargueDetails = responseHttp.Response;
    }

    private async Task ShowModalAsync(Guid? id = null, bool isEdit = false)
    {
        if (isEdit)
        {
            var parameters = new Dictionary<string, object>
            {
                { "Id", id! },
                { "Title", $"{Localizer[nameof(Resource.Edit_Serial)]}"   }
            };
            await _modalService.ShowAsync<EditSerialsDetails>(parameters);
        }
        else
        {
            var parameters = new Dictionary<string, object>
            {
                { "Id", Id },
                { "Title",$"{Localizer[nameof(Resource.Create_Serial)]}"   }
            };
            await _modalService.ShowAsync<CreateCargueDetails>(parameters);
        }
    }
}