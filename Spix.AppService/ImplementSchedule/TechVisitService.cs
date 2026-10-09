using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceSchedule;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementSchedule;

//Lo que el tecnico ve en el telefono. Modulo aparte del de la web (v1) a proposito: la
//app pide poco y distinto, y asi un cambio para la oficina no rompe lo que esta en la calle.
public class TechVisitService : ITechVisitService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public TechVisitService(DataContext context, ITransactionManager transactionManager,
        IUserHelper userHelper, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    //La jornada del tecnico: lo que tiene abierto, y lo que cerro hoy para que lo pueda
    //consultar. Sin paginar porque es la lista de UN tecnico, no la de la oficina.
    public async Task<ActionResponse<IEnumerable<TechVisitDto>>> GetMineAsync(string username)
    {
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<TechVisitDto>>();
            }

            var technicianId = await GetTechnicianIdAsync(user);
            if (technicianId is null)
            {
                return Fail<IEnumerable<TechVisitDto>>("El usuario no esta registrado como tecnico.");
            }

            var desde = DateTime.UtcNow.Date;

            var lista = await Consulta()
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.Active &&
                            x.TechnicianId == technicianId.Value &&
                            (!x.ScheduleStatus.IsClosed() || x.CompletedAtUtc >= desde))
                .OrderBy(x => x.ScheduledAtUtc ?? x.CreatedAtUtc)
                .ToListAsync();

            return new ActionResponse<IEnumerable<TechVisitDto>>
            {
                WasSuccess = true,
                Result = lista.Select(ToDto).ToList()
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<TechVisitDto>>(ex);
        }
    }

    public async Task<ActionResponse<TechVisitDto>> GetAsync(Guid id, string username)
    {
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                return AuthFail<TechVisitDto>();
            }

            var technicianId = await GetTechnicianIdAsync(user);
            if (technicianId is null)
            {
                return Fail<TechVisitDto>("El usuario no esta registrado como tecnico.");
            }

            var entity = await Consulta()
                .FirstOrDefaultAsync(x => x.ServiceRequestId == id &&
                                          x.CorporationId == user.CorporationId &&
                                          x.TechnicianId == technicianId.Value &&
                                          x.Active);

            if (entity == null)
            {
                return Fail<TechVisitDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            return new ActionResponse<TechVisitDto> { WasSuccess = true, Result = ToDto(entity) };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<TechVisitDto>(ex);
        }
    }

    //Llegue y empiezo. La web lo hace guardando toda la tarjeta; desde el telefono eso no
    //tiene sentido, por eso es un paso propio.
    public async Task<ActionResponse<TechVisitDto>> StartAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<TechVisitDto>();
            }

            var technicianId = await GetTechnicianIdAsync(user);
            if (technicianId is null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<TechVisitDto>("El usuario no esta registrado como tecnico.");
            }

            var entity = await _context.ServiceRequests
                .Include(x => x.ScheduleItem)
                .FirstOrDefaultAsync(x => x.ServiceRequestId == id &&
                                          x.CorporationId == user.CorporationId &&
                                          x.TechnicianId == technicianId.Value &&
                                          x.Active);

            if (entity == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<TechVisitDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            //Solo se empieza lo que esta por hacer
            if (entity.ScheduleStatus != ScheduleStatus.Pending &&
                entity.ScheduleStatus != ScheduleStatus.OnHold)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<TechVisitDto>("Esta visita no esta pendiente.");
            }

            entity.ScheduleStatus = ScheduleStatus.InProgress;

            //La cita del calendario sigue el estado de la orden
            var schedule = entity.ScheduleItem ?? await _context.ScheduleItems
                .FirstOrDefaultAsync(x => x.ServiceRequestId == entity.ServiceRequestId);

            if (schedule != null)
            {
                schedule.ScheduleStatus = ScheduleStatus.InProgress;
                schedule.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return await GetAsync(entity.ServiceRequestId, username);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<TechVisitDto>(ex);
        }
    }

    //===== Lo de adentro =====

    //Una sola consulta con todo lo que la app necesita: en la calle no se pueden hacer
    //cinco viajes al servidor por pantalla
    private IQueryable<ServiceRequest> Consulta()
    {
        return _context.ServiceRequests.AsNoTracking()
            .Include(x => x.ContractClient!).ThenInclude(x => x.ContractMaps)
            .Include(x => x.ServiceRequestDetails)
            .Include(x => x.ServiceRequestPhotos);
    }

    private TechVisitDto ToDto(ServiceRequest x)
    {
        var tieneServicio = x.ServiceRequestDetails is { Count: > 0 };
        var tieneFoto = x.ServiceRequestPhotos?.Any(p => p.PhotoType == ServicePhotoType.After) == true;
        var tieneComentario = !string.IsNullOrWhiteSpace(x.TechnicianComment);
        var mapa = x.ContractClient?.ContractMaps?.FirstOrDefault();

        return new TechVisitDto
        {
            ServiceRequestId = x.ServiceRequestId,
            RequestNumber = x.RequestNumber,
            Status = (int)x.ScheduleStatus,
            StatusText = _localizer[$"ScheduleStatus_{x.ScheduleStatus}"],
            OriginName = _localizer[$"ServiceRequestOrigin_{x.Origin}"],
            ScheduledAtUtc = x.ScheduledAtUtc,
            ControlContrato = x.ControlContrato,
            ClientFullName = x.ClientFullName,
            Address = x.Address,
            ContactPhone = string.IsNullOrWhiteSpace(x.ContactPhone) ? x.PhoneNumber : x.ContactPhone,
            ClientReason = x.ClientReason,
            PlanName = x.PlanName,
            PlanSpeed = x.PlanSpeed,
            ServerName = x.ServerName,
            NodeName = x.NodeName,
            NodeIp = x.NodeIp,
            IpCliente = x.IpCliente,
            MacCliente = x.MacCliente,
            ContractLatitude = mapa?.Latitude,
            ContractLongitude = mapa?.Longitude,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            CapturedAtUtc = x.CapturedAtUtc,
            DistanceMeters = x.DistanceMeters,
            TechnicianComment = x.TechnicianComment,
            HasService = tieneServicio,
            HasAfterPhoto = tieneFoto,

            //La misma regla del cierre de v1, resuelta aqui para que la app no la repita
            CanClose = x.ScheduleStatus == ScheduleStatus.InProgress &&
                       tieneServicio && tieneFoto && tieneComentario
        };
    }

    private async Task<User?> GetUserAsync(string username) => await _userHelper.GetUserByUserNameAsync(username);

    //Quien entra por la app es un tecnico: si no lo es, no ve nada
    private async Task<Guid?> GetTechnicianIdAsync(User user)
    {
        var esTecnico = await _context.UserRoleDetails.AsNoTracking()
            .AnyAsync(x => x.UserId == user.Id && x.UserType == UserType.Technician);

        if (!esTecnico)
        {
            return null;
        }

        return await _context.Technicians.AsNoTracking()
            .Where(x => x.UserName == user.UserName &&
                        x.CorporationId == user.CorporationId &&
                        x.Active)
            .Select(x => (Guid?)x.TechnicianId)
            .FirstOrDefaultAsync();
    }

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };

    private static ActionResponse<T> Fail<T>(string message) => new()
    {
        WasSuccess = false,
        Message = message
    };
}
