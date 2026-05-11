using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesMk;
using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;
using Spix.xNetwork.MkHelper;

namespace Spix.AppService.ImplementMk;

public class MkConnectionService : IMkConnectionService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IMapperService _mapperService;
    private readonly IStringLocalizer _localizer;
    private readonly IMikrotikControl _mikrotik;
    private readonly HttpErrorHandler _httpErrorHandler;

    public MkConnectionService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, HttpErrorHandler httpErrorHandler,
        IMapperService mapperService, IStringLocalizer localizer, IMikrotikControl mikrotik)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _mapperService = mapperService;
        _localizer = localizer;
        _mikrotik = mikrotik;
        _httpErrorHandler = httpErrorHandler;
    }

    public async Task<ActionResponse<MkConnectionResultDTO>> CheckConnectionAsync(Guid serverId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<MkConnectionResultDTO>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            // 1. Obtener datos del servidor desde la BD (como antes)
            var server = await _context.Servers.Include(x=> x.IpNetwork).FirstOrDefaultAsync(x => x.ServerId == serverId);
            if (server == null)
            {
                return new ActionResponse<MkConnectionResultDTO>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Server_Not_Found)]
                };
            }

            // 2. Armar modelo de conexión
            var mk = new MkConnectionInfo
            {
                Host = server.IpNetwork!.Ip!,
                Port = server.ApiPort,
                Username = server.Usuario,
                Password = server.Clave
            };

            // 3. Conectar
            bool ok = await _mikrotik.ConnectAsync(mk);
            if (!ok)
            {
                return new ActionResponse<MkConnectionResultDTO>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Mikrotik_Connection_Error)]
                };
            }

            // 4. Obtener identidad
            var identity = await _mikrotik.SendCommandAsync("/system/identity/print");

            // 5. Obtener IP Bindings
            var bindings = await _mikrotik.SendCommandAsync("/ip/hotspot/ip-binding/print", "=.proplist=address");

            _mikrotik.Disconnect();

            // 6. Procesar identidad
            string name = identity.FirstOrDefault(x => x.Contains("name="))?.Replace("!re=name=", "") ?? "Desconocido";

            // 7. Crear DTO de respuesta
            var dto = new MkConnectionResultDTO
            {
                Text = $"Conexión exitosa a Mikrotik {name}",
                Value = bindings.Count - 1,
                MikrotikName = name
            };

            return new ActionResponse<MkConnectionResultDTO>
            {
                WasSuccess = true,
                Result = dto
            };

        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<MkConnectionResultDTO>(ex); // ✅ Manejo de errores automático
        }
    }
}