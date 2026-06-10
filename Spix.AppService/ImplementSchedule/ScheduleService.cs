using Microsoft.EntityFrameworkCore;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceSchedule;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.ImplementSchedule;

public class ScheduleService : IScheduleService
{
    private readonly DataContext _context;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;

    public ScheduleService(DataContext context, HttpErrorHandler httpErrorHandle, ITransactionManager transactionManager,
        IUserHelper userHelper)
    {
        _context = context;
        _httpErrorHandler = httpErrorHandle;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
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
                RecurrenceRule = x.RecurrenceRule
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
                RecurrenceRule = entity.RecurrenceRule
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
                UsuarioOwner = $"{user.FirstName!} {user.LastName!}"
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
