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

//Los servicios que se le cargan a una visita: lo que despues se le cobra al cliente.
//Va aparte de ServiceRequestService a proposito: es lo que va a crecer cuando entre la
//facturacion, y no tiene por que engordar el servicio de la solicitud.
public class ServiceRequestDetailService : IServiceRequestDetailService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public ServiceRequestDetailService(
        DataContext context,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
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

            //Mientras la visita no arranca no hay nada que cargarle: el tecnico
            //tiene que marcar que esta en sitio.
            if (request.ScheduleStatus == ScheduleStatus.Pending ||
                request.ScheduleStatus == ScheduleStatus.Requested)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ServiceRequestDetailDto>(_localizer["Visit_NotStarted"]);
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

    //Quien entra: si es tecnico, solo puede tocar lo suyo
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
