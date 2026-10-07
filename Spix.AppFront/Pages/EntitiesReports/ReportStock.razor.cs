using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesInven;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesReports;

//Movimientos de inventario: que entro y que salio de cada bodega, y cuanto hay hoy.
//
//Los movimientos los DEDUCE el backend de las compras y los traslados cerrados: el sistema
//no guarda una tabla de movimientos, el Stock es un acumulado.
public partial class ReportStock
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private const string BaseUrl = "api/v3/reports-inventory";

    private ReportStockSummaryDto? Summary;
    private List<ReportStockMoveDto>? Moves;
    private List<ReportStockBalanceDto>? Balance;

    //El combo llega armado del backend, con su neutro
    private List<ProductStorage>? Storages;

    private DateTime? Desde = DateTime.Today.AddMonths(-1);
    private DateTime? Hasta = DateTime.Today;
    private Guid StorageId;

    protected override async Task OnInitializedAsync()
    {
        var bodegas = await _repository.GetAsync<List<ProductStorage>>("api/v1/combosData/ComboStorage");
        if (!await _responseHandler.HandleErrorAsync(bodegas))
        {
            Storages = bodegas.Response;
        }

        await Cargar();
    }

    private void DesdeChanged(ChangeEventArgs e)
    {
        Desde = DateTime.TryParse(e?.Value?.ToString(), out var valor) ? valor : null;
    }

    private void HastaChanged(ChangeEventArgs e)
    {
        Hasta = DateTime.TryParse(e?.Value?.ToString(), out var valor) ? valor : null;
    }

    private void StorageChanged(ChangeEventArgs e)
    {
        StorageId = Guid.TryParse(e?.Value?.ToString(), out var valor) ? valor : Guid.Empty;
    }

    private async Task Cargar()
    {
        //Las tres consultas comparten el mismo filtro
        var filtro = $"?desde={Desde:yyyy-MM-dd}&hasta={Hasta:yyyy-MM-dd}";
        if (StorageId != Guid.Empty)
        {
            filtro += $"&storageId={StorageId}";
        }

        var resumen = await _repository.GetAsync<ReportStockSummaryDto>($"{BaseUrl}/stock/summary{filtro}");
        if (!await _responseHandler.HandleErrorAsync(resumen))
        {
            Summary = resumen.Response;
        }

        var movimientos = await _repository.GetAsync<List<ReportStockMoveDto>>($"{BaseUrl}/stock/moves{filtro}");
        if (!await _responseHandler.HandleErrorAsync(movimientos))
        {
            Moves = movimientos.Response ?? new();
        }

        //Las existencias son de HOY: solo les aplica la bodega, no el periodo
        var bodega = StorageId == Guid.Empty ? string.Empty : $"?storageId={StorageId}";
        var existencias = await _repository.GetAsync<List<ReportStockBalanceDto>>($"{BaseUrl}/stock/balance{bodega}");
        if (!await _responseHandler.HandleErrorAsync(existencias))
        {
            Balance = existencias.Response ?? new();
        }

        await InvokeAsync(StateHasChanged);
    }
}
