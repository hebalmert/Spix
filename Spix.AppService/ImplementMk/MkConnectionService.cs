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
    private readonly HttpErrorHandler _httpErrorHandler;

    public MkConnectionService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, HttpErrorHandler httpErrorHandler,
        IMapperService mapperService, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _mapperService = mapperService;
        _localizer = localizer;
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
            var server = await _context.Servers.Include(x => x.IpNetwork).FirstOrDefaultAsync(x => x.ServerId == serverId);
            if (server == null)
            {
                return new ActionResponse<MkConnectionResultDTO>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Server_Not_Found)]
                };
            }

            // 3. Conectar
            MK mikrotik = new MK(server.IpNetwork!.Ip!, server.ApiPort);
            if (!mikrotik.Login(server.Usuario, server.Clave))
            {
                return new ActionResponse<MkConnectionResultDTO>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Mikrotik_Connection_Error)]
                };
            }

            // 4. Obtener identidad
            mikrotik.Send("/system/identity/getall");
            mikrotik.Send("/system/identity/print", true);
            List<string> listArray = new List<string>();
            foreach (string s in mikrotik.Read())
            {
                listArray.Add(s);
            }
            var listArrayCount = listArray.Count;
            listArray.RemoveAt(listArrayCount - 1);
            var PrimerRegistro = listArray.FirstOrDefault();
            var NameServidor = PrimerRegistro!.Substring(9);

            // 5. Obtener IP Bindings
            mikrotik.Send("/ip/hotspot/ip-binding/getall");
            mikrotik.Send("/ip/hotspot/ip-binding/print");
            mikrotik.Send("=.proplist=address", true);
            List<string> list = new List<string>();
            foreach (var item in mikrotik.Read())
            {
                list.Add(item);
            }
            var bindings = list.Count;

            mikrotik.Close();

            // 7. Crear DTO de respuesta
            var dto = new MkConnectionResultDTO
            {
                Text = $"Conexión exitosa a Mikrotik {NameServidor}",
                Value = bindings,
                MikrotikName = NameServidor
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