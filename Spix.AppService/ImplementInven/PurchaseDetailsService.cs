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
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.Services.ImplementInven;

//Los renglones de la compra y su cierre. Todo filtrado por la corporacion del usuario,
//y solo se tocan renglones de una compra abierta (Pendiente).
public class PurchaseDetailsService : IPurchaseDetailsService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IUserHelper _userHelper;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;
    private readonly IStringLocalizer _localizer;

    public PurchaseDetailsService(
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

    public async Task<ActionResponse<IEnumerable<PurchaseDetail>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<PurchaseDetail>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var queryable = _context.PurchaseDetails
                .AsNoTracking()
                .Include(x => x.Product)
                .ThenInclude(x => x!.ProductCategory)
                .Where(x => x.CorporationId == corporationId && x.PurchaseId == pagination.GuidId);

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var list = await queryable
                .OrderBy(x => x.NameProduct)
                .Paginate(pagination)
                .ToListAsync();

            return Success<IEnumerable<PurchaseDetail>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<PurchaseDetail>>(ex);
        }
    }

    public async Task<ActionResponse<PurchaseDetail>> GetAsync(Guid id, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<PurchaseDetail>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var modelo = await _context.PurchaseDetails
                .AsNoTracking()
                .Include(x => x.Product)
                .ThenInclude(x => x!.ProductCategory)
                .FirstOrDefaultAsync(x => x.PurchaseDetailId == id && x.CorporationId == corporationId);
            if (modelo == null) return Fail<PurchaseDetail>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            return Success(modelo);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<PurchaseDetail>(ex);
        }
    }

    //Un producto va una sola vez por compra: si ya esta, se edita su renglon
    public async Task<ActionResponse<PurchaseDetail>> AddAsync(PurchaseDetail modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<PurchaseDetail>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var purchaseError = await ValidateOpenPurchaseAsync(modelo.PurchaseId, corporationId.Value);
            if (purchaseError != null) return await FailRollbackAsync<PurchaseDetail>(purchaseError);

            var product = await GetProductAsync(modelo.ProductId, corporationId.Value);
            if (product == null) return await FailRollbackAsync<PurchaseDetail>(_localizer["Purchase_InvalidProduct"]);

            var lineError = await ValidateLineAsync(modelo, product, null);
            if (lineError != null) return await FailRollbackAsync<PurchaseDetail>(lineError);

            //Lo que decide el servidor: la tasa y el nombre salen del producto
            var nuevo = new PurchaseDetail
            {
                PurchaseId = modelo.PurchaseId,
                ProductId = product.ProductId,
                NameProduct = product.ProductName,
                RateTax = product.Tax?.Rate ?? 0,
                Quantity = modelo.Quantity,
                UnitCost = modelo.UnitCost,
                CorporationId = corporationId.Value
            };

            //Persistencia
            _context.PurchaseDetails.Add(nuevo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(nuevo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<PurchaseDetail>(ex);
        }
    }

    public async Task<ActionResponse<PurchaseDetail>> UpdateAsync(PurchaseDetail modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<PurchaseDetail>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.PurchaseDetails.FirstOrDefaultAsync(x =>
                x.PurchaseDetailId == modelo.PurchaseDetailId &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<PurchaseDetail>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var purchaseError = await ValidateOpenPurchaseAsync(current.PurchaseId, corporationId.Value);
            if (purchaseError != null) return await FailRollbackAsync<PurchaseDetail>(purchaseError);

            var product = await GetProductAsync(modelo.ProductId, corporationId.Value);
            if (product == null) return await FailRollbackAsync<PurchaseDetail>(_localizer["Purchase_InvalidProduct"]);

            //El renglon sigue en su compra aunque el cliente mande otra
            modelo.PurchaseId = current.PurchaseId;
            var lineError = await ValidateLineAsync(modelo, product, current.PurchaseDetailId);
            if (lineError != null) return await FailRollbackAsync<PurchaseDetail>(lineError);

            //Mapeo campo por campo
            current.ProductId = product.ProductId;
            current.NameProduct = product.ProductName;
            current.RateTax = product.Tax?.Rate ?? 0;
            current.Quantity = modelo.Quantity;
            current.UnitCost = modelo.UnitCost;

            //Persistencia
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(current);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<PurchaseDetail>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.PurchaseDetails.FirstOrDefaultAsync(x =>
                x.PurchaseDetailId == id &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var purchaseError = await ValidateOpenPurchaseAsync(current.PurchaseId, corporationId.Value);
            if (purchaseError != null) return await FailRollbackAsync<bool>(purchaseError);

            //Persistencia
            _context.PurchaseDetails.Remove(current);
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

    //Cierra la compra: sube el stock a la bodega de la compra, actualiza el costo del producto
    //y abre un cargue por cada producto con seriales.
    //Del modelo que llega solo se usa el PurchaseId: la bodega y la corporacion salen de la base.
    public async Task<ActionResponse<Purchase>> ClosePurchaseSync(Purchase modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<Purchase>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var purchase = await _context.Purchases.FirstOrDefaultAsync(x =>
                x.PurchaseId == modelo.PurchaseId &&
                x.CorporationId == corporationId);
            if (purchase == null) return await FailRollbackAsync<Purchase>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var details = await _context.PurchaseDetails
                .Include(x => x.Product)
                .Where(x => x.PurchaseId == purchase.PurchaseId && x.CorporationId == corporationId)
                .ToListAsync();
            if (details.Count == 0) return await FailRollbackAsync<Purchase>(_localizer["Purchase_NoItems"]);

            //Renglones viejos que quedaron con cantidad partida en productos con seriales
            var partida = details.FirstOrDefault(x => x.Product!.WithSerials && x.Quantity != decimal.Truncate(x.Quantity));
            if (partida != null) return await FailRollbackAsync<Purchase>(_localizer["Purchase_QuantityWhole", partida.NameProduct ?? string.Empty]);

            //El cierre se reclama en UNA sentencia: si dos cierres llegan a la vez (doble clic,
            //reintento), solo uno encuentra la compra Pendiente y el otro no suma nada.
            var claimed = await _context.Purchases
                .Where(x => x.PurchaseId == purchase.PurchaseId && x.Status == PurchaseStatus.Pendiente)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, PurchaseStatus.Completado));
            if (claimed == 0) return await FailRollbackAsync<Purchase>(_localizer["Purchase_NotPending"]);

            foreach (var item in details)
            {
                //Stock en la bodega de la compra
                var stock = await _context.ProductStocks.FirstOrDefaultAsync(x =>
                    x.ProductId == item.ProductId &&
                    x.ProductStorageId == purchase.ProductStorageId &&
                    x.CorporationId == corporationId);
                if (stock == null)
                {
                    _context.ProductStocks.Add(new ProductStock
                    {
                        ProductId = item.ProductId,
                        ProductStorageId = purchase.ProductStorageId,
                        Stock = item.Quantity,
                        CorporationId = corporationId.Value
                    });
                }
                else
                {
                    stock.AddStock(item.Quantity);
                }

                //Costo del producto: el ultimo costo unitario con su impuesto
                item.Product!.Costo = ((item.RateTax / 100) + 1) * item.UnitCost;

                //Un producto con seriales abre su cargue para subir las MAC
                if (item.Product.WithSerials)
                {
                    //El consecutivo del cargue lo entrega la base
                    var nroCargue = await NumberSequence.NextAsync(_context, corporationId.Value, NumberKind.Cargue);
                    _context.Cargues.Add(new Cargue
                    {
                        DateCargue = DateTime.UtcNow,
                        ControlCargue = Convert.ToString(nroCargue),
                        PurchaseDetailId = item.PurchaseDetailId,
                        ProductId = item.ProductId,
                        CantToUp = item.Quantity,
                        Status = CargueType.Pendiente,
                        CorporationId = corporationId.Value
                    });
                }
            }

            //Persistencia
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            purchase.Status = PurchaseStatus.Completado;
            return Success(purchase);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Purchase>(ex);
        }
    }

    //La compra existe, es de la corporacion y sigue abierta
    private async Task<string?> ValidateOpenPurchaseAsync(Guid purchaseId, int corporationId)
    {
        var status = await _context.Purchases
            .Where(x => x.PurchaseId == purchaseId && x.CorporationId == corporationId)
            .Select(x => (PurchaseStatus?)x.Status)
            .FirstOrDefaultAsync();

        if (status == null) return _localizer[nameof(Resource.Generic_IdNotFound)];
        if (status != PurchaseStatus.Pendiente) return _localizer["Purchase_NotPending"];
        return null;
    }

    private async Task<Product?> GetProductAsync(Guid productId, int corporationId)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(x => x.Tax)
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.CorporationId == corporationId);
    }

    //Cantidad y costo validos, entera si el producto lleva seriales, y sin repetir producto
    private async Task<string?> ValidateLineAsync(PurchaseDetail modelo, Product product, Guid? purchaseDetailId)
    {
        if (modelo.Quantity <= 0) return _localizer["Purchase_QuantityInvalid"];
        if (modelo.UnitCost < 0) return _localizer["Purchase_CostInvalid"];
        if (product.WithSerials && modelo.Quantity != decimal.Truncate(modelo.Quantity)) return _localizer["Purchase_QuantityWhole", product.ProductName];

        var repeated = await _context.PurchaseDetails.AnyAsync(x =>
            x.PurchaseId == modelo.PurchaseId &&
            x.ProductId == product.ProductId &&
            x.PurchaseDetailId != purchaseDetailId);
        if (repeated) return _localizer["Purchase_ProductRepeated", product.ProductName];

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
