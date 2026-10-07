using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementInven;

//El reporte de movimientos de inventario: que entro y que salio de cada bodega.
//
// IMPORTANTE: el sistema NO guarda una tabla de movimientos; el Stock es un acumulado que
// se suma y se resta. Asi que los movimientos se DEDUCEN de los dos documentos que lo
// mueven, y solo cuando ya estan cerrados, que es cuando el inventario se toco de verdad:
//
//   Compra cerrada   -> entra a la bodega de la compra
//   Traslado cerrado -> sale del origen y entra al destino
//
// La ventaja de deducirlo es que el reporte sirve hacia atras, con lo ya registrado.
//
// Lo que NO se puede listar como movimiento son los seriales (instalar en un contrato o
// marcar averiado): el sistema guarda su estado pero no CUANDO cambio. Por eso van en el
// segundo bloque, como foto del momento y no como historia.
//
// Vive aparte de los modulos de inventario, igual que ReportInventoryService.
public class ReportStockService : IReportStockService
{
    private readonly DataContext _context;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public ReportStockService(
        DataContext context,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer)
    {
        _context = context;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    //Los numeros de arriba: se calculan sobre las mismas filas del periodo
    public async Task<ActionResponse<ReportStockSummaryDto>> GetSummaryAsync(
        string username, DateTime? desde, DateTime? hasta, Guid? storageId)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return AuthFail<ReportStockSummaryDto>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var movimientos = await MovesAsync(corporationId, desde, hasta, storageId);

            var existencias = await BalanceQuery(corporationId, storageId).ToListAsync();

            var summary = new ReportStockSummaryDto
            {
                Moves = movimientos.Count,
                Entries = movimientos.Where(x => x.Quantity > 0).Sum(x => x.Quantity),
                Exits = Math.Abs(movimientos.Where(x => x.Quantity < 0).Sum(x => x.Quantity)),
                Storages = existencias.Select(x => x.StorageName).Distinct().Count(),
                TotalStock = existencias.Sum(x => x.Stock)
            };

            return new ActionResponse<ReportStockSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ReportStockSummaryDto>(ex);
        }
    }

    //Los movimientos del periodo, del mas reciente al mas viejo
    public async Task<ActionResponse<IEnumerable<ReportStockMoveDto>>> GetMovesAsync(
        string username, DateTime? desde, DateTime? hasta, Guid? storageId)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return AuthFail<IEnumerable<ReportStockMoveDto>>();

            var lista = await MovesAsync(Convert.ToInt32(user.CorporationId), desde, hasta, storageId);

            return new ActionResponse<IEnumerable<ReportStockMoveDto>>
            {
                WasSuccess = true,
                Result = lista.OrderByDescending(x => x.Date).ThenBy(x => x.Document).ToList()
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportStockMoveDto>>(ex);
        }
    }

    //Lo que hay hoy en cada bodega, con el desglose de seriales
    public async Task<ActionResponse<IEnumerable<ReportStockBalanceDto>>> GetBalanceAsync(
        string username, Guid? storageId)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null) return AuthFail<IEnumerable<ReportStockBalanceDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            var existencias = await BalanceQuery(corporationId, storageId)
                .OrderBy(x => x.StorageName)
                .ThenBy(x => x.ProductName)
                .ToListAsync();

            //El desglose de seriales, en UNA consulta agrupada para no preguntar por fila.
            //La bodega del serial es la de la compra que lo trajo: no se guarda en el serial.
            var seriales = await _context.CargueDetails
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId)
                .Where(x => x.ProductStorageId != null)
                .GroupBy(x => new
                {
                    Producto = x.Cargue!.Product!.ProductName,
                    Bodega = x.ProductStorage!.StorageName
                })
                .Select(g => new
                {
                    g.Key.Producto,
                    g.Key.Bodega,
                    Available = g.Count(x => x.Status == SerialStateType.Disponible),
                    Operative = g.Count(x => x.Status == SerialStateType.Operativo),
                    Damaged = g.Count(x => x.Status == SerialStateType.Averiado)
                })
                .ToListAsync();

            foreach (var fila in existencias)
            {
                var detalle = seriales.FirstOrDefault(x => x.Producto == fila.ProductName &&
                                                           x.Bodega == fila.StorageName);
                if (detalle == null)
                {
                    continue;
                }

                fila.Available = detalle.Available;
                fila.Operative = detalle.Operative;
                fila.Damaged = detalle.Damaged;
            }

            return new ActionResponse<IEnumerable<ReportStockBalanceDto>>
            {
                WasSuccess = true,
                Result = existencias
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ReportStockBalanceDto>>(ex);
        }
    }

    //Las tres fuentes de movimiento, en tres consultas, unidas en memoria.
    //Son tres porque salen de tablas distintas y cada una aporta pocas filas: lo que se
    //junta aqui ya viene filtrado por corporacion, periodo y bodega.
    private async Task<List<ReportStockMoveDto>> MovesAsync(
        int corporationId, DateTime? desde, DateTime? hasta, Guid? storageId)
    {
        //ENTRADAS POR COMPRA: solo las cerradas, que son las que movieron inventario
        var compras = await _context.PurchaseDetails
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        x.Purchase!.Status == PurchaseStatus.Completado &&
                        (storageId == null || x.Purchase.ProductStorageId == storageId) &&
                        (desde == null || x.Purchase.PurchaseDate >= desde) &&
                        (hasta == null || x.Purchase.PurchaseDate <= hasta))
            .Select(x => new ReportStockMoveDto
            {
                Date = x.Purchase!.PurchaseDate,
                Origin = "Compra",
                Document = x.Purchase.NroPurchase.ToString(),
                ProductName = x.Product!.ProductName,
                StorageName = x.Purchase.ProductStorage!.StorageName,
                Quantity = x.Quantity,
                Reference = x.Purchase.Supplier!.Name ?? string.Empty
            })
            .ToListAsync();

        //SALIDAS DEL ORIGEN, por traslado cerrado
        var salidas = await _context.TransferDetails
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        x.Transfer!.Status == TransferType.Completado &&
                        (storageId == null || x.Transfer.FromProductStorageId == storageId) &&
                        (desde == null || x.Transfer.DateTransfer >= desde) &&
                        (hasta == null || x.Transfer.DateTransfer <= hasta))
            .Select(x => new ReportStockMoveDto
            {
                Date = x.Transfer!.DateTransfer,
                Origin = "Traslado",
                Document = x.Transfer.NroTransfer.ToString(),
                ProductName = x.Product!.ProductName,
                StorageName = x.Transfer.FromStorageName ?? string.Empty,
                Quantity = -x.Quantity,
                Reference = x.Transfer.NombreUsuario ?? string.Empty
            })
            .ToListAsync();

        //ENTRADAS AL DESTINO, del mismo traslado
        var entradas = await _context.TransferDetails
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        x.Transfer!.Status == TransferType.Completado &&
                        (storageId == null || x.Transfer.ToProductStorageId == storageId) &&
                        (desde == null || x.Transfer.DateTransfer >= desde) &&
                        (hasta == null || x.Transfer.DateTransfer <= hasta))
            .Select(x => new ReportStockMoveDto
            {
                Date = x.Transfer!.DateTransfer,
                Origin = "Traslado",
                Document = x.Transfer.NroTransfer.ToString(),
                ProductName = x.Product!.ProductName,
                StorageName = x.Transfer.ToStorageName ?? string.Empty,
                Quantity = x.Quantity,
                Reference = x.Transfer.NombreUsuario ?? string.Empty
            })
            .ToListAsync();

        var todos = new List<ReportStockMoveDto>(compras.Count + salidas.Count + entradas.Count);
        todos.AddRange(compras);
        todos.AddRange(salidas);
        todos.AddRange(entradas);

        return todos;
    }

    //Las existencias actuales, una fila por producto y bodega
    private IQueryable<ReportStockBalanceDto> BalanceQuery(int corporationId, Guid? storageId)
    {
        return _context.ProductStocks
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        (storageId == null || x.ProductStorageId == storageId))
            .Select(x => new ReportStockBalanceDto
            {
                ProductName = x.Product!.ProductName,
                StorageName = x.ProductStorage!.StorageName,
                Stock = x.Stock,
                WithSerials = x.Product.WithSerials
            });
    }

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };
}
