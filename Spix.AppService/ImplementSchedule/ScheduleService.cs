using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementSchedule;

public class ScheduleService : IScheduleService
{
    private readonly DataContext _context;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;

    public ScheduleService(DataContext context, HttpErrorHandler httpErrorHandle, ITransactionManager transactionManager,
        IUserHelper userHelper, IStringLocalizer localizer, IEnumMultilLanguageService enumMultilLanguageService)
    {
        _context = context;
        _httpErrorHandler = httpErrorHandle;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _localizer = localizer;
        _enumMultilLanguageService = enumMultilLanguageService;
    }
    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboStatusAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<IntItemModel>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var list = _enumMultilLanguageService.GetEnumSelectList<ScheduleStatus>();

            list.Insert(0, new IntItemModel
            {
                Value = 0,
                Name = _localizer[nameof(Resource.Select_Status)]
            });


            return new ActionResponse<IEnumerable<IntItemModel>>
            {
                WasSuccess = true,
                Result = list
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
        }
    }


    public async Task<ActionResponse<IEnumerable<ScheduleItemDto>>> GetAsync(DateTime fromUtc, DateTime toUtc, Guid? technicianId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<ScheduleItemDto>>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);

            var query = _context.ScheduleItems
                .Include(x => x.Technician)
                .Where(x => x.Active &&
                            x.CorporationId == user.CorporationId &&
                            x.StartUtc < toUtc &&
                            x.EndUtc > fromUtc);

            if (loggedTechnicianId.HasValue)
                query = query.Where(x => x.TechnicianId == loggedTechnicianId.Value);
            else if (technicianId.HasValue)
                query = query.Where(x => x.TechnicianId == technicianId.Value);

            var items = await query.ToListAsync();

            var dtoList = items.Select(x => new ScheduleItemDto
            {
                Id = x.ScheduleItemId,
                Title = x.Title,
                Description = x.Description,
                StartUtc = x.StartUtc,
                EndUtc = x.EndUtc,
                IsAllDay = x.IsAllDay,
                TechnicianId = x.TechnicianId,
                TechnicianName = $"{x.Technician!.FirstName} {x.Technician.LastName}",
                IsRecurring = x.IsRecurring,
                RecurrenceRule = x.RecurrenceRule,
                ScheduleStatus = x.ScheduleStatus,
                Origin = x.Origin,
                ServiceRequestId = x.ServiceRequestId
            }).ToList();

            return new ActionResponse<IEnumerable<ScheduleItemDto>>
            {
                WasSuccess = true,
                Result = dtoList
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ScheduleItemDto>>(ex);
        }
    }

    public async Task<ActionResponse<ScheduleItemDto>> GetByIdAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<ScheduleItemDto>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);

            var entity = await _context.ScheduleItems
                .Include(x => x.Technician)
                .FirstOrDefaultAsync(x => x.ScheduleItemId == id &&
                                          x.CorporationId == user.CorporationId &&
                                          (!loggedTechnicianId.HasValue || x.TechnicianId == loggedTechnicianId.Value) &&
                                          x.Active);

            if (entity == null)
            {
                return new ActionResponse<ScheduleItemDto>
                {
                    WasSuccess = false,
                    Message = "No se encontró el registro indicado"
                };
            }

            var dto = new ScheduleItemDto
            {
                Id = entity.ScheduleItemId,
                Title = entity.Title,
                Description = entity.Description,
                StartUtc = entity.StartUtc,
                EndUtc = entity.EndUtc,
                IsAllDay = entity.IsAllDay,
                TechnicianId = entity.TechnicianId,
                TechnicianName = $"{entity.Technician!.FirstName} {entity.Technician.LastName}",
                IsRecurring = entity.IsRecurring,
                RecurrenceRule = entity.RecurrenceRule,
                ScheduleStatus = entity.ScheduleStatus,
                Origin = entity.Origin,
                ServiceRequestId = entity.ServiceRequestId
            };

            return new ActionResponse<ScheduleItemDto>
            {
                WasSuccess = true,
                Result = dto
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ScheduleItemDto>(ex);
        }
    }

    public async Task<ActionResponse<ScheduleItemDto>> CreateAsync(ScheduleItemDto dto, string UserName)
    {
        try
        {
            await _transactionManager.BeginTransactionAsync();

            var user = await _userHelper.GetUserByUserNameAsync(UserName);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ScheduleItemDto>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var technicianExists = await _context.Technicians
                .AnyAsync(x => x.TechnicianId == dto.TechnicianId && x.CorporationId == user.CorporationId && x.Active);
            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);
            if (loggedTechnicianId.HasValue)
            {
                dto.TechnicianId = loggedTechnicianId.Value;
                technicianExists = true;
            }

            if (!technicianExists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ScheduleItemDto>
                {
                    WasSuccess = false,
                    Result = dto,
                    Message = "Debe seleccionar un tecnico valido."
                };
            }

            var entity = new ScheduleItem
            {
                Title = dto.Title!,
                Description = dto.Description,
                StartUtc = dto.StartUtc,
                EndUtc = dto.EndUtc,
                IsAllDay = dto.IsAllDay,
                TechnicianId = dto.TechnicianId,
                IsRecurring = dto.IsRecurring,
                RecurrenceRule = dto.RecurrenceRule,
                CreatedAtUtc = DateTime.UtcNow,
                CorporationId = Convert.ToInt32(user.CorporationId),
                UserId = Guid.Parse(user.Id),
                UsuarioOwner = $"{user.FirstName!} {user.LastName!}",
                ScheduleStatus = dto.ScheduleStatus,
                Origin = ScheduleOrigin.Schedule
            };

            _context.ScheduleItems.Add(entity);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            dto.Id = entity.ScheduleItemId;

            return new ActionResponse<ScheduleItemDto>
            {
                WasSuccess = true,
                Result = dto
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ScheduleItemDto>(ex);
        }
    }

    public async Task<ActionResponse<ScheduleItemDto>> UpdateAsync(Guid id, ScheduleItemDto dto, string username)
    {
        try
        {
            await _transactionManager.BeginTransactionAsync();

            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ScheduleItemDto>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);

            var entity = await _context.ScheduleItems
                .FirstOrDefaultAsync(x => x.ScheduleItemId == id &&
                                          x.CorporationId == user.CorporationId &&
                                          (!loggedTechnicianId.HasValue || x.TechnicianId == loggedTechnicianId.Value));
            if (entity == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ScheduleItemDto>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            var technicianExists = await _context.Technicians
                .AnyAsync(x => x.TechnicianId == dto.TechnicianId && x.CorporationId == entity.CorporationId && x.Active);
            if (loggedTechnicianId.HasValue)
            {
                dto.TechnicianId = loggedTechnicianId.Value;
                technicianExists = true;
            }

            if (!technicianExists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ScheduleItemDto>
                {
                    WasSuccess = false,
                    Result = dto,
                    Message = "Debe seleccionar un tecnico valido."
                };
            }

            if (entity.Origin == Spix.DomainLogic.EnumTypes.ScheduleOrigin.ServiceRequest)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ScheduleItemDto>
                {
                    WasSuccess = false,
                    Result = dto,
                    Message = "Esta agenda fue creada desde una Solicitud de Servicio. Debe modificarse desde Solicitudes de Servicio."
                };
            }

            entity.Title = dto.Title!;
            entity.Description = dto.Description;
            entity.StartUtc = dto.StartUtc;
            entity.EndUtc = dto.EndUtc;
            entity.IsAllDay = dto.IsAllDay;
            entity.TechnicianId = dto.TechnicianId;
            entity.IsRecurring = dto.IsRecurring;
            entity.RecurrenceRule = dto.RecurrenceRule;
            entity.UpdatedAtUtc = DateTime.UtcNow;
            entity.ScheduleStatus = dto.ScheduleStatus;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ScheduleItemDto>
            {
                WasSuccess = true,
                Result = dto
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ScheduleItemDto>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        try
        {
            await _transactionManager.BeginTransactionAsync();

            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            var loggedTechnicianId = await GetLoggedTechnicianIdAsync(user);

            var entity = await _context.ScheduleItems
                .FirstOrDefaultAsync(x => x.ScheduleItemId == id &&
                                          x.CorporationId == user.CorporationId &&
                                          (!loggedTechnicianId.HasValue || x.TechnicianId == loggedTechnicianId.Value));
            if (entity == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            if (entity.Origin == Spix.DomainLogic.EnumTypes.ScheduleOrigin.ServiceRequest)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Esta agenda fue creada desde una Solicitud de Servicio. Debe modificarse desde Solicitudes de Servicio."
                };
            }

            entity.Active = false;

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
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    private async Task<Guid?> GetLoggedTechnicianIdAsync(Spix.Domain.Entities.User user)
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
}
