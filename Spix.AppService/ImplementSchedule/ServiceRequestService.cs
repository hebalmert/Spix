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
                .Include(x => x.ServiceRequestPic)
                .Where(x => x.CorporationId == user.CorporationId && x.Active)
                .AsQueryable();

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);
            if (loggedTechnicianId.HasValue)
            {
                queryable = queryable.Where(x => x.TechnicianId == loggedTechnicianId.Value);
            }

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

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);

            var entity = await _context.ServiceRequests
                .Include(x => x.Technician)
                .Include(x => x.ServiceRequestPic)
                .Include(x => x.ServiceRequestDetails)!.ThenInclude(x => x.ServiceCategory)
                .Include(x => x.ServiceRequestDetails)!.ThenInclude(x => x.ServiceClient)
                .Include(x => x.ServiceRequestDetails)!.ThenInclude(x => x.Tax)
                .FirstOrDefaultAsync(x => x.ServiceRequestId == id &&
                                          x.CorporationId == user.CorporationId &&
                                          (!loggedTechnicianId.HasValue || x.TechnicianId == loggedTechnicianId.Value) &&
                                          x.Active);

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

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);
            if (loggedTechnicianId.HasValue)
            {
                dto.TechnicianId = loggedTechnicianId.Value;
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

            if (!await TechnicianIsValidAsync(dto.TechnicianId, entity.CorporationId))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDto>("Debe seleccionar un tecnico activo.");
            }

            var markCompleted = entity.ScheduleStatus != ScheduleStatus.Completed &&
                                dto.ScheduleStatus == ScheduleStatus.Completed;

            entity.TechnicianId = dto.TechnicianId;
            entity.ScheduledAtUtc = dto.ScheduledAtUtc;
            entity.ScheduleStatus = dto.ScheduleStatus;
            entity.ClientReason = dto.ClientReason;
            entity.TechnicianComment = dto.TechnicianComment;
            entity.Recommendation = dto.Recommendation;

            if (markCompleted)
            {
                entity.CompletedAtUtc = DateTime.UtcNow;
                entity.UserIdCompleted = Guid.Parse(user.Id);
                entity.UsuarioOwnerCompleted = $"{user.FirstName} {user.LastName}";
            }

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

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);

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

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);

            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(x => x.ServiceRequestId == dto.ServiceRequestId &&
                                          x.CorporationId == user.CorporationId &&
                                          (!loggedTechnicianId.HasValue || x.TechnicianId == loggedTechnicianId.Value) &&
                                          x.Active);
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

            if (request.Billed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDetailDto>("La solicitud facturada no puede modificarse.");
            }

            var service = await _context.ServiceClients
                .Include(x => x.Tax)
                .FirstOrDefaultAsync(x => x.ServiceClientId == dto.ServiceClientId &&
                                          x.ServiceCategoryId == dto.ServiceCategoryId &&
                                          x.CorporationId == user.CorporationId &&
                                          x.Active);
            if (service == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDetailDto>("Debe seleccionar un servicio activo.");
            }

            var taxRate = service.Tax?.Rate ?? 0;
            var taxAmount = taxRate == 0 ? 0 : (((taxRate / 100) + 1) * service.Price) - service.Price;

            var detail = new ServiceRequestDetail
            {
                ServiceRequestId = dto.ServiceRequestId,
                ServiceCategoryId = dto.ServiceCategoryId,
                ServiceClientId = dto.ServiceClientId,
                TaxId = service.TaxId,
                TaxRate = taxRate,
                Price = service.Price,
                TaxAmount = taxAmount,
                Detail = dto.Detail
            };

            _context.ServiceRequestDetails.Add(detail);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            dto.ServiceRequestDetailId = detail.ServiceRequestDetailId;
            dto.TaxId = detail.TaxId;
            dto.TaxRate = detail.TaxRate;
            dto.Price = detail.Price;
            dto.TaxAmount = detail.TaxAmount;
            dto.Total = detail.Total;
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

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);

            var detail = await _context.ServiceRequestDetails
                .Include(x => x.ServiceRequest)
                .FirstOrDefaultAsync(x => x.ServiceRequestDetailId == id &&
                                          x.ServiceRequest!.CorporationId == user.CorporationId &&
                                          (!loggedTechnicianId.HasValue || x.ServiceRequest.TechnicianId == loggedTechnicianId.Value));
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

            if (detail.ServiceRequest.Billed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>("La solicitud facturada no puede modificarse.");
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
            ServiceRequestPicId = entity.ServiceRequestPic?.ServiceRequestPicId
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
