using Spix.AppService.ImplementContratos;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceSchedule;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SettingModels;
using Spix.xLanguage.Resources;
using Spix.xFiles.FileHelper;

namespace Spix.AppService.ImplementSchedule;

public class ServiceRequestService : IServiceRequestService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IFileStorage _fileStorage;
    private readonly ImgSetting _imgOption;

    public ServiceRequestService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer, IFileStorage fileStorage, IOptions<ImgSetting> imgOption)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _fileStorage = fileStorage;
        _imgOption = imgOption.Value;
    }

    public async Task<ActionResponse<IEnumerable<ServiceRequestDto>>> GetAsync(PaginationDTO pagination, int? status, string username)
    {
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<ServiceRequestDto>>();
            }

            var queryable = _context.ServiceRequests.AsNoTracking()
                .Include(x => x.Technician)
                .Include(x => x.ServiceRequestPic)
                .Where(x => x.CorporationId == user.CorporationId && x.Active)
                .AsQueryable();

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);
            if (loggedTechnicianId.HasValue)
            {
                queryable = queryable.Where(x => x.TechnicianId == loggedTechnicianId.Value);
            }

            //Y el cliente solo ve las solicitudes de sus propios contratos
            var loggedClientId = await GetLoggedClientIdAsync(user);
            if (loggedClientId.HasValue)
            {
                queryable = queryable.Where(x => x.ContractClient!.ClientId == loggedClientId.Value);
            }

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.ClientFullName, $"%{filter}%") ||
                    EF.Functions.Like(x.ControlContrato.ToString(), $"%{filter}%") ||
                    EF.Functions.Like(x.RequestNumber.ToString(), $"%{filter}%"));
            }

            //Cero es "todas": las pildoras del tablero filtran por estatus
            if (status.HasValue && status.Value > 0)
            {
                var estado = (ScheduleStatus)status.Value;
                queryable = queryable.Where(x => x.ScheduleStatus == estado);
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderByDescending(x => x.CreatedAtUtc)
                .Paginate(pagination)
                .Select(x => ToDto(x))
                .ToListAsync();

            return new ActionResponse<IEnumerable<ServiceRequestDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ServiceRequestDto>>(ex);
        }
    }

    //Los numeros del tablero: hoy, lo que esta pendiente, lo que esta en sitio y lo que
    //se cerro este mes. Se cuentan sobre todo lo que el usuario puede ver.
    public async Task<ActionResponse<ServiceRequestSummaryDto>> GetSummaryAsync(string username)
    {
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                return AuthFail<ServiceRequestSummaryDto>();
            }

            var queryable = _context.ServiceRequests.AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId && x.Active)
                .AsQueryable();

            //El tecnico solo cuenta lo suyo, igual que en el listado
            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);
            if (loggedTechnicianId.HasValue)
            {
                queryable = queryable.Where(x => x.TechnicianId == loggedTechnicianId.Value);
            }

            var hoy = DateTime.UtcNow.Date;
            var manana = hoy.AddDays(1);
            var mes = new DateTime(hoy.Year, hoy.Month, 1);

            var summary = new ServiceRequestSummaryDto
            {
                Requested = await queryable.CountAsync(x => x.ScheduleStatus == ScheduleStatus.Requested),
                Today = await queryable.CountAsync(x => x.ScheduledAtUtc >= hoy && x.ScheduledAtUtc < manana),
                Pending = await queryable.CountAsync(x => x.ScheduleStatus == ScheduleStatus.Pending),
                InProgress = await queryable.CountAsync(x => x.ScheduleStatus == ScheduleStatus.InProgress),
                CompletedMonth = await queryable.CountAsync(x => x.ScheduleStatus == ScheduleStatus.Completed &&
                                                                 x.CompletedAtUtc != null &&
                                                                 x.CompletedAtUtc >= mes)
            };

            return new ActionResponse<ServiceRequestSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestSummaryDto>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<ServiceRequestContractDto>>> SearchContractsAsync(string filter, string username)
    {
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<ServiceRequestContractDto>>();
            }

            filter = filter?.Trim() ?? string.Empty;
            if (filter.Length < 2)
            {
                return new ActionResponse<IEnumerable<ServiceRequestContractDto>>
                {
                    WasSuccess = true,
                    Result = Enumerable.Empty<ServiceRequestContractDto>()
                };
            }

            //Se buscan TODOS los contratos, no solo los activos: un cortado o un exonerado
            //tambien puede necesitar una visita.
            var contracts = await ContractQuery()
                .Where(x => x.CorporationId == user.CorporationId &&
                            (EF.Functions.Like(x.Client!.FirstName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client!.LastName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client!.FirstName + " " + x.Client!.LastName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client.Document, $"%{filter}%") ||
                             EF.Functions.Like(x.ControlContrato.ToString(), $"%{filter}%")))
                .OrderBy(x => x.Client!.FirstName)
                .ThenBy(x => x.Client!.LastName)
                .Take(20)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ServiceRequestContractDto>>
            {
                WasSuccess = true,
                Result = contracts.Select(ToContractDto).ToList()
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ServiceRequestContractDto>>(ex);
        }
    }


    public async Task<ActionResponse<ServiceRequestDto>> GetAsync(Guid id, string username)
    {
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                return AuthFail<ServiceRequestDto>();
            }

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);

            var loggedClientId = await GetLoggedClientIdAsync(user);

            var entity = await _context.ServiceRequests.AsNoTracking()
                .Include(x => x.ContractClient)
                .Include(x => x.Technician)
                .Include(x => x.ServiceRequestPic)
                .Include(x => x.ServiceRequestPhotos)
                .Include(x => x.ServiceRequestDetails)!.ThenInclude(x => x.ServiceCategory)
                .Include(x => x.ServiceRequestDetails)!.ThenInclude(x => x.ServiceClient)
                .Include(x => x.ServiceRequestDetails)!.ThenInclude(x => x.Tax)
                .FirstOrDefaultAsync(x => x.ServiceRequestId == id &&
                                          x.CorporationId == user.CorporationId &&
                                          (!loggedTechnicianId.HasValue || x.TechnicianId == loggedTechnicianId.Value) &&
                                          (!loggedClientId.HasValue || x.ContractClient!.ClientId == loggedClientId.Value) &&
                                          x.Active);

            if (entity == null)
            {
                return new ActionResponse<ServiceRequestDto> { WasSuccess = false, Message = _localizer[nameof(Resource.Generic_IdNotFound)] };
            }

            var dto = ToDto(entity, true);

            //Los enlaces del blob se piden aqui, que es cuando se van a ver
            foreach (var photo in dto.Photos)
            {
                photo.ImageFullPath = await GetPhotoUrlAsync(
                    entity.ServiceRequestPhotos!.First(x => x.ServiceRequestPhotoId == photo.ServiceRequestPhotoId).Photo);
            }

            return new ActionResponse<ServiceRequestDto> { WasSuccess = true, Result = dto };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestDto>(ex);
        }
    }

    public async Task<ActionResponse<ServiceRequestDto>> AddAsync(ServiceRequestDto dto, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ServiceRequestDto>();
            }

            //El estado del contrato no limita la visita: un cortado puede necesitarla para
            //volver a activarse y un exonerado tambien pide servicio.
            var contract = await ContractQuery()
                .FirstOrDefaultAsync(x => x.ContractClientId == dto.ContractClientId &&
                                          x.CorporationId == user.CorporationId);
            if (contract == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            //La solicitud la levanta la oficina o el cliente desde su portal; el tecnico
            //solo atiende lo que le asignan.
            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);
            if (loggedTechnicianId.HasValue)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>(_localizer["Request_TechnicianCannotCreate"]);
            }

            //Esta es la solicitud que levanta la OFICINA: nace con tecnico y fecha.
            //Lo que pide el cliente vive en MyServiceRequestService, con sus propias reglas.
            if (!await TechnicianIsValidAsync(dto.TechnicianId!.Value, Convert.ToInt32(user.CorporationId)))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe seleccionar un tecnico activo.");
            }

            if (dto.ScheduledAtUtc == null || dto.ScheduledAtUtc == default(DateTime))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe seleccionar fecha y hora programada.");
            }

            if (string.IsNullOrWhiteSpace(dto.ClientReason))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe indicar la razon de la llamada.");
            }

            var contactPhone = string.IsNullOrWhiteSpace(dto.ContactPhone)
                ? contract.PhoneNumber
                : dto.ContactPhone.Trim();

            var snapshot = ToContractDto(contract);
            var nextNumber = await NextRequestNumberAsync(Convert.ToInt32(user.CorporationId));
            var entity = new ServiceRequest
            {
                RequestNumber = nextNumber,
                CreatedAtUtc = DateTime.UtcNow,
                ScheduledAtUtc = dto.ScheduledAtUtc,
                ContractClientId = dto.ContractClientId,
                TechnicianId = dto.TechnicianId,
                ScheduleStatus = ScheduleStatus.Pending,
                Origin = ServiceRequestOrigin.Office,
                ClientReason = dto.ClientReason.Trim(),
                CorporationId = Convert.ToInt32(user.CorporationId),
                UserId = Guid.Parse(user.Id),
                UsuarioOwner = $"{user.FirstName} {user.LastName}",
                ControlContrato = snapshot.ControlContrato,
                ClientFullName = snapshot.ClientFullName,
                PhoneNumber = snapshot.PhoneNumber,
                ContactPhone = contactPhone,
                Address = snapshot.Address,
                CityName = snapshot.CityName,
                ZoneName = snapshot.ZoneName,
                ServerName = snapshot.ServerName,
                IpServer = snapshot.IpServer,
                IpCliente = snapshot.IpCliente,
                MacCliente = snapshot.MacCliente,
                PlanName = snapshot.PlanName,
                NodeName = snapshot.NodeName,
                NodeIp = snapshot.NodeIp,
                PlanSpeed = snapshot.PlanSpeed
            };

            _context.ServiceRequests.Add(entity);
            await _transactionManager.SaveChangesAsync();

            //Sin fecha no hay nada que poner en el calendario: la cita nace al agendar
            if (entity.ScheduledAtUtc.HasValue && entity.TechnicianId.HasValue)
            {
                var schedule = BuildSchedule(entity);
                _context.ScheduleItems.Add(schedule);
                await _transactionManager.SaveChangesAsync();
            }

            await _transactionManager.CommitTransactionAsync();
            return await GetAsync(entity.ServiceRequestId, username);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestDto>(ex);
        }
    }


    public async Task<ActionResponse<ServiceRequestDto>> UpdateAsync(ServiceRequestDto dto, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ServiceRequestDto>();
            }

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);

            var entity = await _context.ServiceRequests
                .Include(x => x.ScheduleItem)
                .FirstOrDefaultAsync(x => x.ServiceRequestId == dto.ServiceRequestId &&
                                          x.CorporationId == user.CorporationId &&
                                          (!loggedTechnicianId.HasValue || x.TechnicianId == loggedTechnicianId.Value) &&
                                          x.Active);
            if (entity == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ServiceRequestDto> { WasSuccess = false, Message = _localizer[nameof(Resource.Generic_IdNotFound)] };
            }

            if (entity.ScheduleStatus == ScheduleStatus.Completed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("La solicitud completada no puede modificarse.");
            }

            if (loggedTechnicianId.HasValue)
            {
                dto.TechnicianId = loggedTechnicianId.Value;
            }

            //Mientras la solicitud no se agenda no hay tecnico que validar
            if (dto.TechnicianId.HasValue &&
                !await TechnicianIsValidAsync(dto.TechnicianId!.Value, entity.CorporationId))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe seleccionar un tecnico activo.");
            }

            //Guardar la tarjeta de la visita NO cierra la orden: cerrar tiene su propio
            //camino, con su lista de lo que falta. Y tampoco la devuelve a Solicitada.
            if (dto.ScheduleStatus == ScheduleStatus.Completed ||
                dto.ScheduleStatus == ScheduleStatus.PhoneResolved ||
                dto.ScheduleStatus == ScheduleStatus.Requested)
            {
                dto.ScheduleStatus = entity.ScheduleStatus;
            }

            var markCompleted = false;

            entity.TechnicianId = dto.TechnicianId;
            entity.ScheduledAtUtc = dto.ScheduledAtUtc;
            entity.ScheduleStatus = dto.ScheduleStatus;
            //La razon es lo que dijo el cliente cuando pidio la visita: no se reescribe.
            //Lo que el tecnico encuentre va en su comentario.
            entity.TechnicianComment = dto.TechnicianComment;
            entity.Recommendation = dto.Recommendation;

            if (markCompleted)
            {
                entity.CompletedAtUtc = DateTime.UtcNow;
                entity.UserIdCompleted = Guid.Parse(user.Id);
                entity.UsuarioOwnerCompleted = $"{user.FirstName} {user.LastName}";
            }

            var schedule = entity.ScheduleItem ?? await _context.ScheduleItems.FirstOrDefaultAsync(x => x.ServiceRequestId == entity.ServiceRequestId);
            if (schedule != null && entity.ScheduledAtUtc.HasValue && entity.TechnicianId.HasValue)
            {
                schedule.Title = $"{entity.ClientFullName} - Contrato #{entity.ControlContrato}";
                schedule.Description = entity.ClientReason;
                schedule.StartUtc = entity.ScheduledAtUtc.Value;
                schedule.EndUtc = entity.ScheduledAtUtc.Value;
                schedule.TechnicianId = entity.TechnicianId.Value;
                schedule.ScheduleStatus = entity.ScheduleStatus;
                schedule.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();
            return await GetAsync(entity.ServiceRequestId, username);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestDto>(ex);
        }
    }

    //Agendar: la oficina le pone tecnico y fecha, y ahi recien nace la cita del calendario
    public async Task<ActionResponse<ServiceRequestDto>> AssignAsync(Guid id, Guid technicianId, DateTime scheduledAtUtc, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ServiceRequestDto>();
            }

            var entity = await _context.ServiceRequests
                .Include(x => x.ScheduleItem)
                .FirstOrDefaultAsync(x => x.ServiceRequestId == id &&
                                          x.CorporationId == user.CorporationId &&
                                          x.Active);

            if (entity == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (entity.ScheduleStatus != ScheduleStatus.Requested)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>(_localizer["Request_OnlyRequested"]);
            }

            if (!await TechnicianIsValidAsync(technicianId, entity.CorporationId))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe seleccionar un tecnico activo.");
            }

            if (scheduledAtUtc == default)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe seleccionar fecha y hora programada.");
            }

            entity.TechnicianId = technicianId;
            entity.ScheduledAtUtc = scheduledAtUtc;
            entity.ScheduleStatus = ScheduleStatus.Pending;

            //La cita nace aqui: antes no habia cuando ni quien
            if (entity.ScheduleItem == null)
            {
                _context.ScheduleItems.Add(BuildSchedule(entity));
            }

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return await GetAsync(entity.ServiceRequestId, username);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestDto>(ex);
        }
    }

    //Cerrar la visita. Tiene su propio camino porque tiene sus propias reglas: un servicio
    //cargado, el comentario del tecnico y una foto del despues. El guardado normal de la
    //tarjeta no puede cerrar una orden.
    public async Task<ActionResponse<ServiceRequestDto>> CloseAsync(Guid id, string? comment, string? recommendation, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ServiceRequestDto>();
            }

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);

            var entity = await _context.ServiceRequests
                .Include(x => x.ScheduleItem)
                .Include(x => x.ServiceRequestDetails)
                .Include(x => x.ServiceRequestPhotos)
                .FirstOrDefaultAsync(x => x.ServiceRequestId == id &&
                                          x.CorporationId == user.CorporationId &&
                                          (!loggedTechnicianId.HasValue || x.TechnicianId == loggedTechnicianId.Value) &&
                                          x.Active);

            if (entity == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            //Solo se cierra lo que se empezo
            if (entity.ScheduleStatus != ScheduleStatus.InProgress)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>(_localizer["Visit_NotStarted"]);
            }

            //Lo que el tecnico escribio en pantalla se guarda al cerrar
            if (!string.IsNullOrWhiteSpace(comment))
            {
                entity.TechnicianComment = comment.Trim();
            }

            if (!string.IsNullOrWhiteSpace(recommendation))
            {
                entity.Recommendation = recommendation.Trim();
            }

            //Las tres condiciones del cierre, verificadas aqui y no en la pantalla
            var faltaServicio = entity.ServiceRequestDetails == null || entity.ServiceRequestDetails.Count == 0;
            var faltaComentario = string.IsNullOrWhiteSpace(entity.TechnicianComment);
            var faltaFoto = entity.ServiceRequestPhotos == null ||
                            !entity.ServiceRequestPhotos.Any(x => x.PhotoType == ServicePhotoType.After);

            if (faltaServicio || faltaComentario || faltaFoto)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>(_localizer["Close_Missing"]);
            }

            entity.ScheduleStatus = ScheduleStatus.Completed;
            entity.CompletedAtUtc = DateTime.UtcNow;
            entity.UserIdCompleted = Guid.Parse(user.Id);
            entity.UsuarioOwnerCompleted = $"{user.FirstName} {user.LastName}";

            //La cita del calendario sigue el estado de la orden
            var schedule = entity.ScheduleItem ?? await _context.ScheduleItems
                .FirstOrDefaultAsync(x => x.ServiceRequestId == entity.ServiceRequestId);

            if (schedule != null)
            {
                schedule.ScheduleStatus = ScheduleStatus.Completed;
                schedule.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return await GetAsync(entity.ServiceRequestId, username);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestDto>(ex);
        }
    }

    //Resuelta por telefono: se cierra sin visita, con lo que dijo quien atendio
    public async Task<ActionResponse<ServiceRequestDto>> ResolveByPhoneAsync(Guid id, string? comment, string? recommendation, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ServiceRequestDto>();
            }

            var entity = await _context.ServiceRequests
                .FirstOrDefaultAsync(x => x.ServiceRequestId == id &&
                                          x.CorporationId == user.CorporationId &&
                                          x.Active);

            if (entity == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (entity.ScheduleStatus != ScheduleStatus.Requested)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>(_localizer["Request_OnlyRequested"]);
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>(_localizer["Request_PhoneNeedsComment"]);
            }

            entity.ScheduleStatus = ScheduleStatus.PhoneResolved;
            entity.TechnicianComment = comment.Trim();
            entity.Recommendation = string.IsNullOrWhiteSpace(recommendation) ? null : recommendation.Trim();
            entity.CompletedAtUtc = DateTime.UtcNow;
            entity.UserIdCompleted = Guid.Parse(user.Id);
            entity.UsuarioOwnerCompleted = $"{user.FirstName} {user.LastName}";

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return await GetAsync(entity.ServiceRequestId, username);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestDto>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<bool>();
            }

            //El tecnico recibe la orden para resolverla: no la borra
            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);
            if (loggedTechnicianId.HasValue)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer["Request_TechnicianCannotDelete"]);
            }

            var entity = await _context.ServiceRequests
                .Include(x => x.ScheduleItem)
                .Include(x => x.ServiceRequestPic)
                .Include(x => x.ServiceRequestDetails)
                .FirstOrDefaultAsync(x => x.ServiceRequestId == id &&
                                          x.CorporationId == user.CorporationId &&
                                          (!loggedTechnicianId.HasValue || x.TechnicianId == loggedTechnicianId.Value) &&
                                          x.Active);
            if (entity == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = _localizer[nameof(Resource.Generic_IdNotFound)] };
            }

            if (entity.ScheduleStatus == ScheduleStatus.Completed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>("La solicitud completada no puede eliminarse.");
            }

            var schedule = entity.ScheduleItem ?? await _context.ScheduleItems.FirstOrDefaultAsync(x => x.ServiceRequestId == entity.ServiceRequestId);
            if (schedule != null)
            {
                _context.ScheduleItems.Remove(schedule);
            }

            if (entity.ServiceRequestDetails?.Any() == true)
            {
                _context.ServiceRequestDetails.RemoveRange(entity.ServiceRequestDetails);
            }

            if (entity.ServiceRequestPic != null)
            {
                DeletePicImage(entity.ServiceRequestPic.PhotoBefore1);
                DeletePicImage(entity.ServiceRequestPic.PhotoBefore2);
                DeletePicImage(entity.ServiceRequestPic.PhotoAfter1);
                DeletePicImage(entity.ServiceRequestPic.PhotoAfter2);
                _context.ServiceRequestPics.Remove(entity.ServiceRequestPic);
            }

            _context.ServiceRequests.Remove(entity);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool> { WasSuccess = true, Result = true };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }




    //El enlace del blob dura poco: se firma cada vez que se abre la orden.
    //Subir y borrar fotos vive en ServiceRequestPhotoService.
    private async Task<string?> GetPhotoUrlAsync(string? photo)
    {
        if (string.IsNullOrWhiteSpace(photo))
            return _imgOption.ImgNoImage;

        return await _fileStorage.GetBlobSasUrlAsync(photo, _imgOption.RequiereServicePicture, TimeSpan.FromMinutes(5));
    }

    private IQueryable<ContractClient> ContractQuery()
    {
        return _context.ContractClients
            .Include(x => x.Client)
            .Include(x => x.Zone)!.ThenInclude(x => x.City)
            .Include(x => x.ContractIps)!.ThenInclude(x => x.IpNet)
            .Include(x => x.ContractMacs)!.ThenInclude(x => x.CargueDetail)
            .Include(x => x.ContractServers)!.ThenInclude(x => x.Server)!.ThenInclude(x => x.IpNetwork)
            .Include(x => x.ContractPlans)!.ThenInclude(x => x.Plan)!.ThenInclude(x => x.Tax)
            .Include(x => x.ContractNodes)!.ThenInclude(x => x.Node)!.ThenInclude(x => x!.IpNetwork);
    }

    private static ServiceRequestContractDto ToContractDto(ContractClient contract)
    {
        var ip = contract.ContractIps?.FirstOrDefault()?.IpNet;
        var mac = contract.ContractMacs?.FirstOrDefault()?.CargueDetail;
        var server = contract.ContractServers?.FirstOrDefault()?.Server;
        var plan = contract.ContractPlans?.FirstOrDefault()?.Plan;
        var node = contract.ContractNodes?.FirstOrDefault()?.Node;

        return new ServiceRequestContractDto
        {
            ContractClientId = contract.ContractClientId,
            ControlContrato = contract.ControlContrato,
            ClientFullName = $"{contract.Client?.FirstName} {contract.Client?.LastName}".Trim(),
            PhoneNumber = contract.PhoneNumber,
            Address = contract.Address,
            CityName = contract.Zone?.City?.Name,
            ZoneName = contract.Zone?.ZoneName,
            ServerName = server?.ServerName,
            IpServer = server?.IpNetwork?.Ip,
            IpCliente = ip?.Ip,
            MacCliente = mac?.MacWlan,
            PlanName = plan?.PlanName,
            PlanSpeed = plan?.VelocidadTotal,
            NodeName = node?.NodesName,
            NodeIp = node?.IpNetwork?.Ip,
            ContractState = contract.ContractState
        };
    }

    private static ServiceRequestDto ToDto(ServiceRequest entity, bool includeDetails = false)
    {
        var dto = new ServiceRequestDto
        {
            ServiceRequestId = entity.ServiceRequestId,
            RequestNumber = entity.RequestNumber,
            CreatedAtUtc = entity.CreatedAtUtc,
            ScheduledAtUtc = entity.ScheduledAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            UserIdCompleted = entity.UserIdCompleted,
            UsuarioOwnerCompleted = entity.UsuarioOwnerCompleted,
            ContractClientId = entity.ContractClientId,
            TechnicianId = entity.TechnicianId,
            TechnicianName = entity.Technician == null ? null : $"{entity.Technician.FirstName} {entity.Technician.LastName}",
            ScheduleStatus = entity.ScheduleStatus,
            ClientReason = entity.ClientReason,
            TechnicianComment = entity.TechnicianComment,
            Recommendation = entity.Recommendation,
            ControlContrato = entity.ControlContrato,
            ClientFullName = entity.ClientFullName,
            PhoneNumber = entity.PhoneNumber,
            Address = entity.Address,
            CityName = entity.CityName,
            ZoneName = entity.ZoneName,
            ServerName = entity.ServerName,
            IpServer = entity.IpServer,
            IpCliente = entity.IpCliente,
            MacCliente = entity.MacCliente,
            PlanName = entity.PlanName,
            PlanSpeed = entity.PlanSpeed,
            Billed = entity.Billed,
            SellId = entity.SellId,
            SubTotal = entity.SubTotal,
            TotalTax = entity.TotalTax,
            Total = entity.Total,
            ServiceRequestPicId = entity.ServiceRequestPic?.ServiceRequestPicId,
            ContactPhone = entity.ContactPhone,
            NodeName = entity.NodeName,
            NodeIp = entity.NodeIp,
            HasPhotoBefore = entity.ServiceRequestPhotos != null &&
                             entity.ServiceRequestPhotos.Any(x => x.PhotoType == ServicePhotoType.Before),
            HasPhotoAfter = entity.ServiceRequestPhotos != null &&
                            entity.ServiceRequestPhotos.Any(x => x.PhotoType == ServicePhotoType.After)
        };

        if (entity.ServiceRequestPhotos != null)
        {
            dto.Photos = entity.ServiceRequestPhotos
                .OrderBy(x => x.PhotoType)
                .ThenBy(x => x.DateCreated)
                .Select(x => new ServiceRequestPhotoDto
                {
                    ServiceRequestPhotoId = x.ServiceRequestPhotoId,
                    ServiceRequestId = x.ServiceRequestId,
                    PhotoType = x.PhotoType,
                    DateCreated = x.DateCreated,
                    UserByName = x.UserByName
                })
                .ToList();
        }

        if (includeDetails && entity.ServiceRequestDetails != null)
        {
            dto.Details = entity.ServiceRequestDetails.Select(x => new ServiceRequestDetailDto
            {
                ServiceRequestDetailId = x.ServiceRequestDetailId,
                ServiceRequestId = x.ServiceRequestId,
                ServiceCategoryId = x.ServiceCategoryId,
                ServiceCategoryName = x.ServiceCategory?.Name,
                ServiceClientId = x.ServiceClientId,
                ServiceClientName = x.ServiceClient?.ServiceName,
                TaxId = x.TaxId,
                TaxRate = x.TaxRate,
                Price = x.Price,
                TaxAmount = x.TaxAmount,
                Total = x.Total,
                SellDetailId = x.SellDetailId,
                Detail = x.Detail
            }).ToList();
        }

        return dto;
    }

    private static ScheduleItem BuildSchedule(ServiceRequest entity)
    {
        return new ScheduleItem
        {
            Title = $"{entity.ClientFullName} - Contrato #{entity.ControlContrato}",
            Description = entity.ClientReason,
            StartUtc = entity.ScheduledAtUtc!.Value,
            EndUtc = entity.ScheduledAtUtc!.Value,
            TechnicianId = entity.TechnicianId!.Value,
            CreatedAtUtc = DateTime.UtcNow,
            Active = true,
            ScheduleStatus = entity.ScheduleStatus,
            Origin = ScheduleOrigin.ServiceRequest,
            ServiceRequestId = entity.ServiceRequestId,
            CorporationId = entity.CorporationId,
            UserId = entity.UserId,
            UsuarioOwner = entity.UsuarioOwner
        };
    }

    private async Task<User?> GetUserAsync(string username) => await _userHelper.GetUserByUserNameAsync(username);

    //Si quien entra es un cliente, devuelve su id; si no, null. Igual que con el tecnico,
    //esto es lo que limita lo que puede ver y hacer.
    private async Task<Guid?> GetLoggedClientIdAsync(User user)
    {
        var isClient = await _context.UserRoleDetails
            .AnyAsync(x => x.UserId == user.Id && x.UserType == UserType.Client);

        if (!isClient)
            return null;

        var clientId = await _context.Clients
            .Where(x => x.UserName == user.UserName &&
                        x.CorporationId == user.CorporationId &&
                        x.Active)
            .Select(x => (Guid?)x.ClientId)
            .FirstOrDefaultAsync();

        return clientId ?? Guid.Empty;
    }

    private async Task<Guid?> GetLoggedTechnicianIdAsync(User user)
    {
        var isTechnician = await _context.UserRoleDetails
            .AnyAsync(x => x.UserId == user.Id && x.UserType == UserType.Technician);

        if (!isTechnician)
            return null;

        var technicianId = await _context.Technicians
            .Where(x => x.UserName == user.UserName &&
                        x.CorporationId == user.CorporationId &&
                        x.Active)
            .Select(x => (Guid?)x.TechnicianId)
            .FirstOrDefaultAsync();

        return technicianId ?? Guid.Empty;
    }

    //Las fotos VIEJAS (ServiceRequestPic) quedaron en el contenedor de las cedulas:
    //se borran de alli, no del contenedor nuevo de las visitas.
    private void DeletePicImage(string? photo)
    {
        if (!string.IsNullOrWhiteSpace(photo))
            _fileStorage.DeleteImage(_imgOption.ImgContractIDPic!, photo);
    }

    private async Task<bool> TechnicianIsValidAsync(Guid technicianId, int corporationId)
    {
        return technicianId != Guid.Empty && await _context.Technicians.AnyAsync(x => x.TechnicianId == technicianId && x.CorporationId == corporationId && x.Active);
    }

    private async Task<long> NextRequestNumberAsync(int corporationId)
    {
        var max = await _context.ServiceRequests
            .Where(x => x.CorporationId == corporationId)
            .Select(x => (long?)x.RequestNumber)
            .MaxAsync();

        return (max ?? 0) + 1;
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
