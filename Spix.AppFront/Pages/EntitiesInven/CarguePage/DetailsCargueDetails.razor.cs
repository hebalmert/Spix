using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using Spix.xLanguage.Resources;
using System.Net;
using System.Text.RegularExpressions;

namespace Spix.AppFront.Pages.EntitiesInven.CarguePage;

public partial class DetailsCargueDetails
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    //Escribir sigue siendo del controlador de seriales; leer, del tablero
    private const string BaseUrl = "/api/v1/cargueDetails";
    private const string BoardUrl = "/api/v1/cargueboard";

    //El mismo formato que valida la entidad: 00:1A:2B:3C:4D:5E, con : o - o sin separador
    private static readonly Regex MacFormat = new(@"^([0-9A-Fa-f]{2}[:-]?){5}[0-9A-Fa-f]{2}$");

    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;
    private string Filter { get; set; } = string.Empty;

    [Parameter] public Guid Id { get; set; }  //Codigo del CargueId

    public CargueProgressDto? Progress { get; set; }
    public List<CargueSerialDto>? Serials { get; set; }

    //El escaner
    private ElementReference ScanInput;
    private string ScanMac = string.Empty;
    private string? ScanMessage;
    private bool ScanOk;
    private bool IsSaving;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadProgressAsync();
            await Cargar();
            await FocusScanAsync();
        }
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

    private async Task LoadProgressAsync()
    {
        var responseHttp = await _repository.GetAsync<CargueProgressDto>($"{BoardUrl}/{Id}/progress");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            _navigationManager.NavigateTo("/cargues");
            return;
        }

        Progress = responseHttp.Response;
        await InvokeAsync(StateHasChanged);
    }

    private async Task Cargar(int page = 1)
    {
        var url = $"{BoardUrl}/{Id}/serials?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }

        var responseHttp = await _repository.GetAsync<List<CargueSerialDto>>(url);
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        Serials = responseHttp.Response;

        await InvokeAsync(StateHasChanged);
    }

    private void ScanChanged(ChangeEventArgs e)
    {
        ScanMac = e.Value?.ToString() ?? string.Empty;
    }

    //Enter del lector: guarda, limpia el campo y lo deja listo para la siguiente MAC.
    //Los avisos van en linea y no en SweetAlert, para no cortar el ritmo del escaneo.
    private async Task ScanAsync()
    {
        var mac = ScanMac.Trim();
        if (string.IsNullOrEmpty(mac) || IsSaving) return;

        //Formato antes de ir al servidor
        if (!MacFormat.IsMatch(mac))
        {
            ShowScan(false, Localizer["Cargue_MacFormat", mac]);
            await FocusScanAsync();
            return;
        }

        //Guardar
        IsSaving = true;
        var modelo = new CargueDetail
        {
            CargueId = Id,
            MacWlan = mac
        };
        var responseHttp = await _repository.PostAsync(BaseUrl, modelo);
        IsSaving = false;

        //Lo que rechaza el negocio (MAC repetida, cargue lleno) se muestra en linea;
        //lo demas (sesion, permisos, servidor) sigue el manejo central.
        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                ShowScan(false, message?.Trim('"') ?? mac);
            }
            else
            {
                await _responseHandler.HandleErrorAsync(responseHttp);
            }

            await FocusScanAsync();
            return;
        }

        //Refrescar avance y lista
        ScanMac = string.Empty;
        ShowScan(true, Localizer["Cargue_MacAdded", modelo.MacWlan!]);
        await LoadProgressAsync();
        CurrentPage = 1;
        await Cargar();
        await FocusScanAsync();
    }

    private void ShowScan(bool ok, string message)
    {
        ScanOk = ok;
        ScanMessage = message;
        StateHasChanged();
    }

    //El campo del escaner solo existe mientras el cargue esta abierto y le faltan seriales
    private async Task FocusScanAsync()
    {
        if (Progress?.Status != CargueType.Pendiente || Missing == 0) return;

        StateHasChanged();
        await Task.Yield();
        try
        {
            await ScanInput.FocusAsync();
        }
        catch (InvalidOperationException)
        {
        }
    }

    private async Task ShowEditAsync(Guid id)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", id },
            { "Title", $"{Localizer[nameof(Resource.Edit_Mac)]}" }
        };

        await _modalService.ShowAsync(typeof(EditCargueDetails), parameters, async result =>
        {
            if (!result.Succeeded) return;

            await LoadProgressAsync();
            await Cargar(CurrentPage);
        });
    }

    //Solo para cargues que quedaron completos sin cerrar: hoy el ultimo serial los cierra solos
    private async Task CloseCargueAsync()
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer["Cargue_CloseTitle"],
            Text = Localizer["Cargue_CloseText"],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer["Cargue_Close"],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.GetAsync($"{BaseUrl}/CerrarTrans/{Id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await LoadProgressAsync();
        await Cargar(CurrentPage);
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

        var responseHttp = await _repository.DeleteAsync($"{BaseUrl}/{id}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _sweetAlert.FireAsync(Localizer[nameof(Resource.msg_DeleteConfirmationTitle)], Localizer[nameof(Resource.msg_DeleteConfirmationText)], SweetAlertIcon.Success);
        await LoadProgressAsync();
        await Cargar(CurrentPage);
    }

    //Lo que falta y el porcentaje, para la barra de avance
    private int Missing => Progress is null ? 0 : Math.Max(0, (int)Progress.CantToUp - Progress.Uploaded);

    private int Percent => Progress is null || Progress.CantToUp <= 0
        ? 0
        : Math.Min(100, (int)(Progress.Uploaded * 100 / Progress.CantToUp));

    private static string StatusCss(SerialStateType status) => status switch
    {
        SerialStateType.Disponible => "is-free",
        SerialStateType.Operativo => "is-used",
        _ => "is-bad"
    };
}
