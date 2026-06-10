using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
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

    public ScheduleService(DataContext context, HttpErrorHandler httpErrorHandle, ITransactionManager transactionManager,
        IUserHelper userHelper, IStringLocalizer localizer)
    {
        _context = context;
        _httpErrorHandler = httpErrorHandle;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _localizer = localizer;
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

            List<IntItemModel> list = Enum.GetValues(typeof(ScheduleStatus)).Cast<ScheduleStatus>().Select(c => new IntItemModel()
            {
                Name = c.ToString(),
                Value = (int)c
            }).ToList();

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


    public async Task<ActionResponse<IEnumerable<ScheduleItemDto>>> GetAsync(DateTime fromUtc, DateTime toUtc, Guid? usuarioId)
    {
        try
        {
            var query = _context.ScheduleItems
                .Include(x => x.Usuario)
                .Where(x => x.Active &&
                            x.StartUtc < toUtc &&
                            x.EndUtc > fromUtc);

            if (usuarioId.HasValue)
                query = query.Where(x => x.UsuarioId == usuarioId.Value);

            var items = await query.ToListAsync();

            var dtoList = items.Select(x => new ScheduleItemDto
            {
                Id = x.ScheduleItemId,
                Title = x.Title,
                Description = x.Description,
                StartUtc = x.StartUtc,
                EndUtc = x.EndUtc,
                IsAllDay = x.IsAllDay,
                UsuarioId = x.UsuarioId,
                UsuarioNombreCompleto = $"{x.Usuario!.FirstName} {x.Usuario.LastName}",
                IsRecurring = x.IsRecurring,
                RecurrenceRule = x.RecurrenceRule,
                ScheduleStatus = x.ScheduleStatus
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

    public async Task<ActionResponse<ScheduleItemDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var entity = await _context.ScheduleItems
                .Include(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.ScheduleItemId == id && x.Active);

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
                UsuarioId = entity.UsuarioId,
                UsuarioNombreCompleto = $"{entity.Usuario!.FirstName} {entity.Usuario.LastName}",
                IsRecurring = entity.IsRecurring,
                RecurrenceRule = entity.RecurrenceRule,
                ScheduleStatus = entity.ScheduleStatus
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
                return new ActionResponse<ScheduleItemDto>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }
            var entity = new ScheduleItem
            {
                Title = dto.Title!,
                Description = dto.Description,
                StartUtc = dto.StartUtc,
                EndUtc = dto.EndUtc,
                IsAllDay = dto.IsAllDay,
                UsuarioId = dto.UsuarioId,
                IsRecurring = dto.IsRecurring,
                RecurrenceRule = dto.RecurrenceRule,
                CreatedAtUtc = DateTime.UtcNow,
                CorporationId = Convert.ToInt32(user.CorporationId),
                UserId = Guid.Parse(user.Id),
                UsuarioOwner = $"{user.FirstName!} {user.LastName!}",
                ScheduleStatus = dto.ScheduleStatus
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

    public async Task<ActionResponse<ScheduleItemDto>> UpdateAsync(Guid id, ScheduleItemDto dto)
    {
        try
        {
            await _transactionManager.BeginTransactionAsync();

            var entity = await _context.ScheduleItems.FindAsync(id);
            if (entity == null)
            {
                return new ActionResponse<ScheduleItemDto>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }

            entity.Title = dto.Title!;
            entity.Description = dto.Description;
            entity.StartUtc = dto.StartUtc;
            entity.EndUtc = dto.EndUtc;
            entity.IsAllDay = dto.IsAllDay;
            entity.UsuarioId = dto.UsuarioId;
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

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            await _transactionManager.BeginTransactionAsync();

            var entity = await _context.ScheduleItems.FindAsync(id);
            if (entity == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
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
}
