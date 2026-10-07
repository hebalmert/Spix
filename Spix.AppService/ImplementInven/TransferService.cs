using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Spix.xLanguage.Resources;
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
    private readonly IStringLocalizer _localizer;

    public TransferService(DataContext context, IHttpContextAccessor httpContextAccessor, IMapperService mapperService,
        ITransactionManager transactionManager, IMemoryCache cache, HttpErrorHandler httpErrorHandle,
        IUserHelper userHelper, IStringLocalizer localizer)
    {
        _localizer = localizer;
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

    //La lista de quien puede recibir los equipos: tecnicos y usuarios del sistema en
    //UNA sola lista, armada aqui y lista para pintar. El front no filtra ni ordena.
    public async Task<ActionResponse<IEnumerable<TextItemModel>>> ReceiversComboAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);

            //Las etiquetas se traducen ACA, no dentro del Select: adentro quedan a
            //merced de como EF decida evaluar la proyeccion
            var etiquetaTecnico = _localizer[nameof(Resource.Audit_Technician)].Value;
            var etiquetaUsuario = _localizer[nameof(Resource.User)].Value;

            var tecnicos = await _context.Technicians.AsNoTracking()
                .Where(x => x.CorporationId == corporationId)
                .OrderBy(x => x.FirstName)
                .Select(x => new TextItemModel
                {
                    Value = "T:" + x.TechnicianId,
                    Name = etiquetaTecnico + " - " + x.FirstName + " " + x.LastName
                })
                .ToListAsync();

            var usuarios = await _context.Usuarios.AsNoTracking()
                .Where(x => x.CorporationId == corporationId)
                .OrderBy(x => x.FirstName)
                .Select(x => new TextItemModel
                {
                    Value = "U:" + x.UsuarioId,
                    Name = etiquetaUsuario + " - " + x.FirstName + " " + x.LastName
                })
                .ToListAsync();

            var lista = tecnicos.Concat(usuarios).ToList();

            //El elemento neutro traducido va en la posicion 0
            lista.Insert(0, new TextItemModel
            {
                Value = string.Empty,
                Name = _localizer[nameof(Resource.Select_Receiver)].Value
            });

            return new ActionResponse<IEnumerable<TextItemModel>>
            {
                WasSuccess = true,
                Result = lista
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<TextItemModel>>(ex);
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

            //NombreUsuario es [NotMapped]: no esta en la base, hay que resolverlo. Se
            //piden los creadores de la pagina en UNA consulta, no uno por fila.
            var creadores = modelo.Where(x => x.UserId != null).Select(x => x.UserId!).Distinct().ToList();

            var nombres = await _context.Users.AsNoTracking()
                .Where(x => creadores.Contains(x.Id))
                .Select(x => new { x.Id, Nombre = x.FirstName + " " + x.LastName })
                .ToDictionaryAsync(x => x.Id, x => x.Nombre);

            foreach (var item in modelo)
            {
                item.NombreUsuario = item.UserId is not null && nombres.TryGetValue(item.UserId, out var nombre)
                    ? nombre
                    : string.Empty;
            }

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

            //La llave del combo se arma de vuelta para que el formulario lo preseleccione
            modelo.ReceiverKey = modelo.ReceivedByTechnicianId is not null
                ? "T:" + modelo.ReceivedByTechnicianId
                : modelo.ReceivedByUsuarioId is not null
                    ? "U:" + modelo.ReceivedByUsuarioId
                    : string.Empty;

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
            var actual = await _context.Transfers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TransferId == modelo.TransferId && x.CorporationId == corporationId);

            if (actual is null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<Transfer>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            //La auditoria no se edita: se conserva la que ya tenia el registro, porque
            //Update pisa toda la fila con lo que llego del formulario
            modelo.DateCreated = actual.DateCreated;
            modelo.UserIdClosed = actual.UserIdClosed;
            modelo.NombreUsuarioCierre = actual.NombreUsuarioCierre;
            modelo.DateClosed = actual.DateClosed;
            modelo.NroTransfer = actual.NroTransfer;

            //Si cambiaron a quien recibe, se vuelve a congelar el nombre
            modelo.ReceivedByName = await ResolverRecibeAsync(modelo, corporationId ?? 0);

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

            //Auditoria: la fecha del registro la pone el servidor (DateTransfer es la del
            //movimiento y esa si la elige el operador)
            modelo.DateCreated = DateTime.Now;
            modelo.ReceivedByName = await ResolverRecibeAsync(modelo, modelo.CorporationId);

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
    //Quien recibe los equipos es UNO de los dos: un tecnico o un usuario del sistema.
    //El nombre se guarda congelado para que el traslado viejo siga diciendo quien recibio
    //aunque esa persona ya no exista.
    private async Task<string?> ResolverRecibeAsync(Transfer modelo, int corporationId)
    {
        //El formulario manda UNA llave; aqui se reparte en la columna que le toca
        RepartirLlave(modelo);

        if (modelo.ReceivedByTechnicianId is not null && modelo.ReceivedByTechnicianId != Guid.Empty)
        {
            modelo.ReceivedByUsuarioId = null;

            var tecnico = await _context.Technicians.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TechnicianId == modelo.ReceivedByTechnicianId &&
                                          x.CorporationId == corporationId);

            return tecnico is null ? null : $"{tecnico.FirstName} {tecnico.LastName}".Trim();
        }

        if (modelo.ReceivedByUsuarioId is not null && modelo.ReceivedByUsuarioId != Guid.Empty)
        {
            var usuario = await _context.Usuarios.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UsuarioId == modelo.ReceivedByUsuarioId &&
                                          x.CorporationId == corporationId);

            return usuario is null ? null : $"{usuario.FirstName} {usuario.LastName}".Trim();
        }

        //Sin destinatario: se limpian los dos
        modelo.ReceivedByTechnicianId = null;
        modelo.ReceivedByUsuarioId = null;
        return null;
    }

    //"T:<guid>" es un tecnico, "U:<guid>" es un usuario, vacio es nadie
    private static void RepartirLlave(Transfer modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo.ReceiverKey))
        {
            return;
        }

        var partes = modelo.ReceiverKey.Split(':');
        if (partes.Length != 2 || !Guid.TryParse(partes[1], out var id))
        {
            return;
        }

        modelo.ReceivedByTechnicianId = partes[0] == "T" ? id : null;
        modelo.ReceivedByUsuarioId = partes[0] == "U" ? id : null;
    }

    private async Task<int?> GetCorporationIdAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        return user?.CorporationId;
    }
}
