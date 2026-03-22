using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.Services.ImplementInven;

public class TransferService : ITransferService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapperService _mapperService;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IUserHelper _userHelper;

    public TransferService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapperService mapperService,
        ITransactionManager transactionManager, IMemoryCache cache, HttpErrorHandler httpErrorHandle,
        IUserHelper userHelper)
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
            List<IntItemModel> list = Enum.GetValues(typeof(TransferType)).Cast<TransferType>().Select(c => new IntItemModel()
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

    public async Task<ActionResponse<IEnumerable<Transfer>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<Transfer>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var queryable = _context.Transfers.Where(x => x.CorporationId == user.CorporationId).AsQueryable();

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.DateTransfer).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<Transfer>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Transfer>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Transfer>> GetAsync(Guid id)
    {
        try
        {
            var modelo = await _context.Transfers
            .FirstOrDefaultAsync(x => x.TransferId == id);
            if (modelo == null)
            {
                return new ActionResponse<Transfer>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == modelo!.UserId);
            modelo!.NombreUsuario = $"{modelo.User!.FirstName} {modelo.User!.LastName}" ;
            return new ActionResponse<Transfer>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Transfer>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Transfer>> UpdateAsync(Transfer modelo)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            Transfer NewModelo = _mapperService.Map<Transfer, Transfer>(modelo);

            _context.Transfers.Update(NewModelo);
            await _transactionManager.SaveChangesAsync();

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Transfer>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Transfer>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<Transfer>> AddAsync(Transfer modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<Transfer>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var Bodegas = await _context.ProductStorages.Where(x => x.CorporationId == user.CorporationId).ToListAsync();
            if (Bodegas == null)
            {
                return new ActionResponse<Transfer>
                {
                    WasSuccess = false,
                    Message = "Problemas para Cargar Las Bodegas"
                };
            }

            modelo.UserId = user.Id;

            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            modelo.FromStorageName = Bodegas.Where(x => x.ProductStorageId == modelo.FromProductStorageId).Select(x => x.StorageName).FirstOrDefault();
            modelo.ToStorageName = Bodegas.Where(x => x.ProductStorageId == modelo.ToProductStorageId).Select(x => x.StorageName).FirstOrDefault();
            modelo.Status = TransferType.Pendiente;
            //Para LLevar el control de Consecutivos de Compra
            int ControlTranfer = 0;
            var CheckRegister = await _context.Registers.FirstOrDefaultAsync(x => x.CorporationId == modelo.CorporationId);
            if (CheckRegister == null)
            {
                return new ActionResponse<Transfer>
                {
                    WasSuccess = false,
                    Message = "Problemas para Correlativo de Transferencia"
                };
            }

            CheckRegister.RegTransfer += 1;
            ControlTranfer = CheckRegister.RegTransfer;
            _context.Registers.Update(CheckRegister);

            //Fin...
            modelo.NroTransfer = ControlTranfer;
            _context.Transfers.Add(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<Transfer>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Transfer>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.Transfers.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            _context.Transfers.Remove(DataRemove);

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