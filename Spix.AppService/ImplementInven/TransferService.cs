using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Spix.AppInfra;
using Spix.AppInfra.Sequences;
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

            //AsNoTracking como en Compras: sin el, EF rastrea las entidades y les rellena
            //las navegaciones con lo que haya cargado en la misma peticion, y eso termina
            //en un ciclo al serializar.
            var queryable = _context.Transfers
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId);

            //Se busca por bodega de origen, de destino o por el numero del traslado
            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.FromStorageName!, $"%{filter}%") ||
                    EF.Functions.Like(x.ToStorageName!, $"%{filter}%") ||
                    EF.Functions.Like(x.NroTransfer.ToString(), $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var modelo = await queryable
                .OrderByDescending(x => x.NroTransfer)
                .Paginate(pagination)
                .ToListAsync();

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

    public async Task<ActionResponse<Transfer>> GetAsync(Guid id, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);

            //Por id SOLO no alcanza: tiene que ser de su corporacion
            var modelo = await _context.Transfers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TransferId == id && x.CorporationId == corporationId);
            if (modelo == null)
            {
                return new ActionResponse<Transfer>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }
            //El nombre sale del usuario que se acaba de consultar. Antes leia modelo.User,
            //que NO viene cargado, y reventaba con null al abrir el registro.
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == modelo.UserId);
            modelo.NombreUsuario = user == null ? string.Empty : $"{user.FirstName} {user.LastName}";
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

    public async Task<ActionResponse<Transfer>> UpdateAsync(Transfer modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var corporationId = await GetCorporationIdAsync(username);

            //Por id SOLO no alcanza: tiene que ser de su corporacion
            var existe = await _context.Transfers.AsNoTracking()
                .AnyAsync(x => x.TransferId == modelo.TransferId && x.CorporationId == corporationId);

            if (!existe)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<Transfer>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

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
            //El consecutivo de la transferencia lo entrega la base, no la memoria
            var ControlTranfer = await NumberSequence.NextAsync(_context, modelo.CorporationId, NumberKind.Transfer);
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

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var corporationId = await GetCorporationIdAsync(username);

            //Por id SOLO no alcanza: tiene que ser de su corporacion
            var DataRemove = await _context.Transfers
                .FirstOrDefaultAsync(x => x.TransferId == id && x.CorporationId == corporationId);
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

    //El CorporationId sale del JWT: el controlador baja el username y aqui se resuelve.
    //Mismo patron que Compras y el resto de los modulos.
    private async Task<int?> GetCorporationIdAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        return user?.CorporationId;
    }
}
