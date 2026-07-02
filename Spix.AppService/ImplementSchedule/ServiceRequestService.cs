using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
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
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementSchedule;

public class ServiceRequestService : IServiceRequestService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public ServiceRequestService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<ServiceRequestDto>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<ServiceRequestDto>>();
            }

            var queryable = _context.ServiceRequests
                .Include(x => x.Technician)
                .Where(x => x.CorporationId == user.CorporationId && x.Active)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.ClientFullName, $"%{filter}%") ||
                    EF.Functions.Like(x.ControlContrato.ToString(), $"%{filter}%") ||
                    EF.Functions.Like(x.RequestNumber.ToString(), $"%{filter}%"));
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

            var contracts = await ContractQuery()
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.ContractState == ContractState.Active &&
                            (EF.Functions.Like(x.Client!.FirstName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client!.LastName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client!.FirstName + " " + x.Client!.LastName, $"%{filter}%") ||
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

            var entity = await _context.ServiceRequests
                .Include(x => x.Technician)
                .Include(x => x.ServiceRequestDetails)!.ThenInclude(x => x.ServiceCategory)
                .Include(x => x.ServiceRequestDetails)!.ThenInclude(x => x.ServiceClient)
                .FirstOrDefaultAsync(x => x.ServiceRequestId == id && x.CorporationId == user.CorporationId && x.Active);

            if (entity == null)
            {
                return new ActionResponse<ServiceRequestDto> { WasSuccess = false, Message = _localizer[nameof(Resource.Generic_IdNotFound)] };
            }

            return new ActionResponse<ServiceRequestDto> { WasSuccess = true, Result = ToDto(entity, true) };
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

            var contract = await ContractQuery()
                .FirstOrDefaultAsync(x => x.ContractClientId == dto.ContractClientId &&
                                          x.CorporationId == user.CorporationId &&
                                          x.ContractState == ContractState.Active);
            if (contract == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe seleccionar un contrato activo.");
            }

            if (!await TechnicianIsValidAsync(dto.TechnicianId, Convert.ToInt32(user.CorporationId)))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe seleccionar un tecnico activo.");
            }

            if (dto.ScheduledAtUtc == default)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe seleccionar fecha y hora programada.");
            }

            if (string.IsNullOrWhiteSpace(dto.ClientReason))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe indicar la razon de la llamada.");
            }

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
                ClientReason = dto.ClientReason.Trim(),
                CorporationId = Convert.ToInt32(user.CorporationId),
                UserId = Guid.Parse(user.Id),
                UsuarioOwner = $"{user.FirstName} {user.LastName}",
                ControlContrato = snapshot.ControlContrato,
                ClientFullName = snapshot.ClientFullName,
                PhoneNumber = snapshot.PhoneNumber,
                Address = snapshot.Address,
                CityName = snapshot.CityName,
                ZoneName = snapshot.ZoneName,
                ServerName = snapshot.ServerName,
                IpServer = snapshot.IpServer,
                IpCliente = snapshot.IpCliente,
                MacCliente = snapshot.MacCliente,
                PlanName = snapshot.PlanName,
                PlanSpeed = snapshot.PlanSpeed
            };

            _context.ServiceRequests.Add(entity);
            await _transactionManager.SaveChangesAsync();

            var schedule = BuildSchedule(entity);
            _context.ScheduleItems.Add(schedule);
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

            var entity = await _context.ServiceRequests
                .Include(x => x.ScheduleItem)
                .FirstOrDefaultAsync(x => x.ServiceRequestId == dto.ServiceRequestId && x.CorporationId == user.CorporationId && x.Active);
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

            if (!await TechnicianIsValidAsync(dto.TechnicianId, entity.CorporationId))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe seleccionar un tecnico activo.");
            }

            entity.TechnicianId = dto.TechnicianId;
            entity.ScheduledAtUtc = dto.ScheduledAtUtc;
            entity.ScheduleStatus = dto.ScheduleStatus;
            entity.ClientReason = dto.ClientReason;
            entity.TechnicianComment = dto.TechnicianComment;
            entity.Recommendation = dto.Recommendation;
            entity.CompletedAtUtc = dto.ScheduleStatus == ScheduleStatus.Completed ? DateTime.UtcNow : null;

            var schedule = entity.ScheduleItem ?? await _context.ScheduleItems.FirstOrDefaultAsync(x => x.ServiceRequestId == entity.ServiceRequestId);
            if (schedule != null)
            {
                schedule.Title = $"{entity.ClientFullName} - Contrato #{entity.ControlContrato}";
                schedule.Description = entity.ClientReason;
                schedule.StartUtc = entity.ScheduledAtUtc;
                schedule.EndUtc = entity.ScheduledAtUtc;
                schedule.TechnicianId = entity.TechnicianId;
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

            var entity = await _context.ServiceRequests
                .Include(x => x.ScheduleItem)
                .Include(x => x.ServiceRequestDetails)
                .FirstOrDefaultAsync(x => x.ServiceRequestId == id && x.CorporationId == user.CorporationId && x.Active);
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

    public async Task<ActionResponse<ServiceRequestDetailDto>> AddDetailAsync(ServiceRequestDetailDto dto, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ServiceRequestDetailDto>();
            }

            var request = await _context.ServiceRequests.FirstOrDefaultAsync(x => x.ServiceRequestId == dto.ServiceRequestId && x.CorporationId == user.CorporationId && x.Active);
            if (request == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDetailDto>("Solicitud de servicio no encontrada.");
            }

            if (request.ScheduleStatus == ScheduleStatus.Completed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDetailDto>("La solicitud completada no puede modificarse.");
            }

            var detail = new ServiceRequestDetail
            {
                ServiceRequestId = dto.ServiceRequestId,
                ServiceCategoryId = dto.ServiceCategoryId,
                ServiceClientId = dto.ServiceClientId,
                Detail = dto.Detail
            };

            _context.ServiceRequestDetails.Add(detail);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            dto.ServiceRequestDetailId = detail.ServiceRequestDetailId;
            return new ActionResponse<ServiceRequestDetailDto> { WasSuccess = true, Result = dto };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ServiceRequestDetailDto>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteDetailAsync(Guid id, string username)
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

            var detail = await _context.ServiceRequestDetails
                .Include(x => x.ServiceRequest)
                .FirstOrDefaultAsync(x => x.ServiceRequestDetailId == id && x.ServiceRequest!.CorporationId == user.CorporationId);
            if (detail == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = _localizer[nameof(Resource.Generic_IdNotFound)] };
            }

            if (detail.ServiceRequest!.ScheduleStatus == ScheduleStatus.Completed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>("La solicitud completada no puede modificarse.");
            }

            _context.ServiceRequestDetails.Remove(detail);
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

    private IQueryable<ContractClient> ContractQuery()
    {
        return _context.ContractClients
            .Include(x => x.Client)
            .Include(x => x.Zone)!.ThenInclude(x => x.City)
            .Include(x => x.ContractIps)!.ThenInclude(x => x.IpNet)
            .Include(x => x.ContractMacs)!.ThenInclude(x => x.CargueDetail)
            .Include(x => x.ContractServers)!.ThenInclude(x => x.Server)!.ThenInclude(x => x.IpNetwork)
            .Include(x => x.ContractPlans)!.ThenInclude(x => x.Plan)!.ThenInclude(x => x.Tax);
    }

    private static ServiceRequestContractDto ToContractDto(ContractClient contract)
    {
        var ip = contract.ContractIps?.FirstOrDefault()?.IpNet;
        var mac = contract.ContractMacs?.FirstOrDefault()?.CargueDetail;
        var server = contract.ContractServers?.FirstOrDefault()?.Server;
        var plan = contract.ContractPlans?.FirstOrDefault()?.Plan;

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
            PlanSpeed = plan?.VelocidadTotal
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
            PlanSpeed = entity.PlanSpeed
        };

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
            StartUtc = entity.ScheduledAtUtc,
            EndUtc = entity.ScheduledAtUtc,
            TechnicianId = entity.TechnicianId,
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
