using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Sequences;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesInven;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.ReportsDTO;
using Spix.xLanguage.Resources;

namespace Spix.Services.ImplementInven;

//La cabecera de la compra. Toda consulta va filtrada por la corporacion del usuario,
//y una compra cerrada (Completado) ya no se edita ni se borra: su stock y sus cargues
//ya se generaron.
public class PurchaseService : IPurchaseService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IUserHelper _userHelper;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;
    private readonly IStringLocalizer _localizer;

    public PurchaseService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IEnumMultilLanguageService enumMultilLanguageService,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _enumMultilLanguageService = enumMultilLanguageService;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus()
    {
        try
        {
            List<IntItemModel> list = _enumMultilLanguageService.GetEnumSelectList<PurchaseStatus>();

            return Success<IEnumerable<IntItemModel>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<Purchase>>> GetReporteSellDates(ReportDataDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<Purchase>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            DateTime dateInicio = Convert.ToDateTime(pagination.DateStart);
            DateTime dateFin = Convert.ToDateTime(pagination.DateEnd);

            var list = await _context.Purchases
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId &&
                            x.Status == PurchaseStatus.Completado &&
                            x.PurchaseDate >= dateInicio &&
                            x.PurchaseDate <= dateFin)
                .Include(x => x.Supplier)
                .Include(x => x.ProductStorage)
                .Include(x => x.PurchaseDetails)
                .AsSplitQuery()
                .ToListAsync();

            return Success<IEnumerable<Purchase>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Purchase>>(ex);
        }
    }

    //El tablero: compras abiertas, y lo cerrado en el mes en curso
    public async Task<ActionResponse<PurchaseSummaryDto>> GetSummaryAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<PurchaseSummaryDto>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var nextMonth = monthStart.AddMonths(1);

            var compras = _context.Purchases
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId);

            var delMes = compras.Where(x =>
                x.Status == PurchaseStatus.Completado &&
                x.PurchaseDate >= monthStart &&
                x.PurchaseDate < nextMonth);

            //El total se calcula en SQL con los renglones: costo por cantidad, mas su impuesto
            var monthTotal = await _context.PurchaseDetails
                .AsNoTracking()
                .Where(d => delMes.Any(p => p.PurchaseId == d.PurchaseId))
                .SumAsync(d => (decimal?)(d.Quantity * d.UnitCost * (1 + d.RateTax / 100))) ?? 0;

            var summary = new PurchaseSummaryDto
            {
                OpenPurchases = await compras.CountAsync(x => x.Status == PurchaseStatus.Pendiente),
                MonthPurchases = await delMes.CountAsync(),
                MonthTotal = monthTotal
            };

            return Success(summary);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<PurchaseSummaryDto>(ex);
        }
    }

    //Las compras de la corporacion, de la mas nueva a la mas vieja
    public async Task<ActionResponse<IEnumerable<Purchase>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<Purchase>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var queryable = _context.Purchases
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Include(x => x.ProductStorage)
                .Include(x => x.PurchaseDetails)
                .Where(x => x.CorporationId == corporationId);

            //Se busca por proveedor o por numero de factura
            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.Supplier!.Name!, $"%{filter}%") ||
                    EF.Functions.Like(x.NroFactura, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var list = await queryable
                .OrderByDescending(x => x.NroPurchase)
                .Paginate(pagination)
                .AsSplitQuery()
                .ToListAsync();

            return Success<IEnumerable<Purchase>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Purchase>>(ex);
        }
    }

    public async Task<ActionResponse<Purchase>> GetAsync(Guid id, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<Purchase>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var modelo = await _context.Purchases
                .AsNoTracking()
                .Include(x => x.PurchaseDetails)
                .Include(x => x.Supplier)
                .Include(x => x.ProductStorage)
                .FirstOrDefaultAsync(x => x.PurchaseId == id && x.CorporationId == corporationId);
            if (modelo == null) return Fail<Purchase>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            return Success(modelo);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Purchase>(ex);
        }
    }

    public async Task<ActionResponse<Purchase>> AddAsync(Purchase modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<Purchase>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var error = await ValidateHeaderAsync(modelo, corporationId.Value, null);
            if (error != null) return await FailRollbackAsync<Purchase>(error);

            //Consecutivo de compra: lo entrega la base, no la memoria
            var nroPurchase = await NumberSequence.NextAsync(_context, corporationId.Value, NumberKind.Purchase);

            //Lo que decide el servidor, no el cliente
            modelo.PurchaseId = Guid.Empty;
            modelo.CorporationId = corporationId.Value;
            modelo.NroPurchase = nroPurchase;
            modelo.Status = PurchaseStatus.Pendiente;
            modelo.NroFactura = modelo.NroFactura.Trim();
            modelo.Supplier = null;
            modelo.ProductStorage = null;
            modelo.PurchaseDetails = null;

            //Persistencia
            _context.Purchases.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(modelo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Purchase>(ex);
        }
    }

    //Solo cambia lo que el usuario puede cambiar: proveedor, bodega, factura y fechas
    public async Task<ActionResponse<Purchase>> UpdateAsync(Purchase modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<Purchase>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.Purchases.FirstOrDefaultAsync(x =>
                x.PurchaseId == modelo.PurchaseId &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<Purchase>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            if (current.Status != PurchaseStatus.Pendiente) return await FailRollbackAsync<Purchase>(_localizer["Purchase_NotPending"]);

            var error = await ValidateHeaderAsync(modelo, corporationId.Value, current.PurchaseId);
            if (error != null) return await FailRollbackAsync<Purchase>(error);

            //Mapeo campo por campo
            current.SupplierId = modelo.SupplierId;
            current.ProductStorageId = modelo.ProductStorageId;
            current.NroFactura = modelo.NroFactura.Trim();
            current.FacuraDate = modelo.FacuraDate;
            current.PurchaseDate = modelo.PurchaseDate;

            //Persistencia
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(current);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Purchase>(ex);
        }
    }

    //Una compra abierta se borra con sus renglones; una cerrada ya movio inventario y no se toca
    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.Purchases
                .Include(x => x.PurchaseDetails)
                .FirstOrDefaultAsync(x => x.PurchaseId == id && x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            if (current.Status != PurchaseStatus.Pendiente) return await FailRollbackAsync<bool>(_localizer["Purchase_NotPending"]);

            //Persistencia
            _context.PurchaseDetails.RemoveRange(current.PurchaseDetails!);
            _context.Purchases.Remove(current);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    //Proveedor y bodega de la misma corporacion, y la factura no repetida para ese proveedor
    private async Task<string?> ValidateHeaderAsync(Purchase modelo, int corporationId, Guid? purchaseId)
    {
        if (string.IsNullOrWhiteSpace(modelo.NroFactura)) return _localizer["Purchase_InvoiceRequired"];

        var supplierOk = await _context.Suppliers.AnyAsync(x =>
            x.SupplierId == modelo.SupplierId &&
            x.CorporationId == corporationId);
        if (!supplierOk) return _localizer["Purchase_InvalidSupplier"];

        var storageOk = await _context.ProductStorages.AnyAsync(x =>
            x.ProductStorageId == modelo.ProductStorageId &&
            x.CorporationId == corporationId);
        if (!storageOk) return _localizer["Purchase_InvalidStorage"];

        var factura = modelo.NroFactura.Trim();
        var invoiceRepeated = await _context.Purchases.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.SupplierId == modelo.SupplierId &&
            x.NroFactura == factura &&
            x.PurchaseId != purchaseId);
        if (invoiceRepeated) return _localizer["Purchase_InvoiceRepeated", factura];

        return null;
    }

    private async Task<int?> GetCorporationIdAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        return user?.CorporationId;
    }

    private async Task<ActionResponse<T>> FailRollbackAsync<T>(string message)
    {
        await _transactionManager.RollbackTransactionAsync();
        return Fail<T>(message);
    }

    private static ActionResponse<T> Success<T>(T result) => new() { WasSuccess = true, Result = result };

    private static ActionResponse<T> Fail<T>(string message) => new() { WasSuccess = false, Message = message };
}
