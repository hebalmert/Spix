using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.Domain.EntitiesInven;
using Spix.Domain.Enum;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesInven;

namespace Spix.Services.ImplementInven;

public class PurchaseDetailsService : IPurchaseDetailsService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapperService _mapperService;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IUserHelper _userHelper;

    public PurchaseDetailsService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapperService mapperService,
        ITransactionManager transactionManager, IMemoryCache cache,
        IUserHelper userHelper, HttpErrorHandler httpErrorHandle)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _mapperService = mapperService;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandle;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatus()
    {
        try
        {
            List<IntItemModel> list = Enum.GetValues(typeof(PurchaseStatus)).Cast<PurchaseStatus>().Select(c => new IntItemModel()
            {
                Name = c.ToString(),
                Value = (int)c
            }).ToList();

            return new ActionResponse<IEnumerable<IntItemModel>>
            {
                WasSuccess = true,
                Result = list
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<PurchaseDetail>>> GetAsync(PaginationDTO pagination, string email)
    {
        try
        {
            var user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<PurchaseDetail>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var queryable = _context.PurchaseDetails
                .Include(x => x.Product).Include(x => x.ProductCategory)
                .Where(x => x.CorporationId == user.CorporationId && x.PurchaseId == pagination.GuidId).AsQueryable();

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.PurchaseDetailId).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<PurchaseDetail>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<PurchaseDetail>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<PurchaseDetail>> GetAsync(Guid id)
    {
        try
        {
            var modelo = await _context.PurchaseDetails
                .FirstOrDefaultAsync(x => x.PurchaseDetailId == id);
            if (modelo == null)
            {
                return new ActionResponse<PurchaseDetail>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            return new ActionResponse<PurchaseDetail>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<PurchaseDetail>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<PurchaseDetail>> UpdateAsync(PurchaseDetail modelo)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            PurchaseDetail NewModelo = _mapperService.Map<PurchaseDetail, PurchaseDetail>(modelo);

            _context.PurchaseDetails.Update(NewModelo);
            await _transactionManager.SaveChangesAsync();

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<PurchaseDetail>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<PurchaseDetail>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<PurchaseDetail>> AddAsync(PurchaseDetail modelo, string email)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                return new ActionResponse<PurchaseDetail>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }
            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            //Guardar el nombre del producto para el Historial
            var nombreProduct = await _context.Products.Where(x => x.ProductId == modelo.ProductId)
                .Select(x => x.ProductName).FirstOrDefaultAsync();
            modelo.NameProduct = nombreProduct;

            var BuscarItem = await _context.PurchaseDetails.FirstOrDefaultAsync(x => x.PurchaseId == modelo.PurchaseId && x.ProductId == modelo.ProductId);
            if (BuscarItem == null)
            {
                _context.PurchaseDetails.Add(modelo);
            }
            else
            {
                BuscarItem.Quantity += modelo.Quantity;
                BuscarItem.UnitCost = modelo.UnitCost;
                _context.PurchaseDetails.Update(BuscarItem);
            }

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<PurchaseDetail>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<PurchaseDetail>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Purchase>> ClosePurchaseSync(Purchase modelo, string email)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                return new ActionResponse<Purchase>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }
            //Vemos cuantos Items hay en la compra por PurchaseDetails
            var PurchaseDetails = await _context.PurchaseDetails.Where(x => x.PurchaseId == modelo.PurchaseId).ToListAsync();
            if (PurchaseDetails.Count == 0)
                return new ActionResponse<Purchase>
                {
                    WasSuccess = false,
                    Message = "No Existe ningun Item para poder hacer un Cierre de Compra, Agregue Item o Elimine la Compra"
                };
            foreach (var item in PurchaseDetails)
            {
                //Actualizamos los inventarios segun la bodega venga en el Modelo
                var ProductStocks = await _context.ProductStocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId && x.ProductStorageId == modelo.ProductStorageId);
                if (ProductStocks == null)
                {
                    ProductStock Inventario = new()
                    {
                        ProductId = item.ProductId,
                        ProductStorageId = modelo.ProductStorageId,
                        Stock = item.Quantity,
                        CorporationId = modelo.CorporationId
                    };
                    _context.ProductStocks.Add(Inventario);
                }
                else
                {
                    decimal NuevoStock = (decimal)(ProductStocks.Stock + item.Quantity);
                    ProductStocks.Stock = NuevoStock;
                    _context.ProductStocks.Update(ProductStocks);
                }
                //Actualizamos el producto con su nuevo valor de Costo
                var UpdateProduct = await _context.Products.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);
                var tasa = (item.RateTax / 100) + 1;
                var costoUnitario = item.UnitCost;
                var NewUniCost = (tasa * costoUnitario);
                UpdateProduct!.Costo = NewUniCost;

                _context.Products.Update(UpdateProduct);

                //Preguntamos si el equipo va a manejar seriales
                if (UpdateProduct.WithSerials)
                {
                    var IdPurchaseDetails = await _context.PurchaseDetails.FirstOrDefaultAsync(x => x.PurchaseId == modelo.PurchaseId);
                    var CheckRegister = await _context.Registers.FirstOrDefaultAsync(x => x.CorporationId == modelo.CorporationId);
                    CheckRegister!.Cargue += 1;
                    _context.Registers.Update(CheckRegister);
                    Cargue cargue = new()
                    {
                        DateCargue = DateTime.Now,
                        ControlCargue = Convert.ToString(CheckRegister.Cargue),
                        PurchaseId = modelo.PurchaseId,
                        PurchaseDetailId = IdPurchaseDetails!.PurchaseDetailId,
                        ProductId = IdPurchaseDetails.ProductId,
                        CantToUp = IdPurchaseDetails.Quantity,
                        Status = CargueType.Pendiente,
                        CorporationId = modelo.CorporationId
                    };
                    _context.Cargues.Add(cargue);
                }
            }
            //Cambiamos el estatus del Purchas para ya no se pueda editar o borrar.
            var UpdatePurchase = await _context.Purchases.FirstOrDefaultAsync(x => x.PurchaseId == modelo.PurchaseId);
            if (UpdatePurchase == null)
                return new ActionResponse<Purchase>
                {
                    WasSuccess = false,
                    Message = "Error en la Actualizacion del Estado de Compra, no se pudo Guradar Nada"
                };
            UpdatePurchase.Status = PurchaseStatus.Completado;
            _context.Purchases.Update(UpdatePurchase);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Purchase>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Purchase>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.PurchaseDetails.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            _context.PurchaseDetails.Remove(DataRemove);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex); // ✅ Manejo de errores automático
        }
    }
}