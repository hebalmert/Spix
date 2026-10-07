using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//El DETALLE de los seriales: uno por fila, con su estado y de quien es si esta instalado.
//El reporte de totales sigue estando aparte; este es el que se baja a Excel.
public partial class ReportSerialsDetail
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private IJSRuntime _js { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports-inventory";

    private List<ReportSerialDetailDto>? Serials;

    //Las dos listas llegan armadas del backend, con su neutro
    private List<ProductStorage>? Storages;
    private List<IntItemModel>? States;

    private int StateValue;
    private Guid StorageId;

    protected override async Task OnInitializedAsync()
    {
        var bodegas = await _repository.GetAsync<List<ProductStorage>>("api/v1/combosData/ComboStorage");
        if (!await _responseHandler.HandleErrorAsync(bodegas))
        {
            Storages = bodegas.Response;
        }

        var estados = await _repository.GetAsync<List<IntItemModel>>($"{BaseUrl}/serials/states");
        if (!await _responseHandler.HandleErrorAsync(estados))
        {
            States = estados.Response;
        }

        await Cargar();
    }

    private void StateChanged(ChangeEventArgs e)
    {
        StateValue = int.TryParse(e?.Value?.ToString(), out var valor) ? valor : 0;
    }

    private void StorageChanged(ChangeEventArgs e)
    {
        StorageId = Guid.TryParse(e?.Value?.ToString(), out var valor) ? valor : Guid.Empty;
    }

    //El neutro es 0: sin estado elegido salen todos
    private string Filtro()
    {
        var filtro = string.Empty;

        if (StateValue > 0)
        {
            filtro += $"estado={StateValue}";
        }

        if (StorageId != Guid.Empty)
        {
            filtro += (filtro.Length == 0 ? string.Empty : "&") + $"storageId={StorageId}";
        }

        return filtro.Length == 0 ? string.Empty : "?" + filtro;
    }

    private async Task Cargar()
    {
        var responseHttp = await _repository.GetAsync<List<ReportSerialDetailDto>>($"{BaseUrl}/serials/detail{Filtro()}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            Serials = new();
            await InvokeAsync(StateHasChanged);
            return;
        }

        Serials = responseHttp.Response ?? new();
        await InvokeAsync(StateHasChanged);
    }

    //La hoja la arma el backend con los [Display] del DTO; aqui solo se baja el archivo
    private async Task DescargarExcel()
    {
        var responseHttp = await _repository.GetFileAsync($"{BaseUrl}/serials/detail/excel{Filtro()}");
        if (await _responseHandler.HandleErrorAsync(responseHttp))
        {
            return;
        }

        var archivo = responseHttp.Response;
        if (archivo is null || archivo.Length == 0)
        {
            return;
        }

        await _js.InvokeVoidAsync("downloadFile",
            $"seriales-{DateTime.Now:yyyyMMdd-HHmm}.xlsx",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            Convert.ToBase64String(archivo));
    }

    //Verde disponible, azul instalado, rojo averiado: los mismos colores del sistema
    private static string ColorEstado(string estado) => estado switch
    {
        "Disponible" => "#198754",
        "Instalado" => "#2c5fad",
        _ => "#dc3545"
    };
}
