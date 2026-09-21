using Spix.xFiles.FileHelper;
using Spix.DomainLogic.SettingModels;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.ImplementContratos;
using Spix.AppService.InterfaceSchedule;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementSchedule;

//El portal del CLIENTE, aparte del servicio de la oficina a proposito.
//
//Aqui solo hay tres cosas: cuales son sus contratos, que solicitudes tiene y como pedir
//una nueva. Cada consulta trae SOLO las columnas que la pantalla muestra: ni montos, ni
//detalles de lo cobrado, ni fotos. Lo que no se consulta no se puede filtrar al portal,
//y ademas no se paga en cada peticion.
public class MyServiceRequestService : IMyServiceRequestService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IFileStorage _fileStorage;
    private readonly ImgSetting _imgOption;

    public MyServiceRequestService(
        DataContext context,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer,
        IFileStorage fileStorage,
        IOptions<ImgSetting> imgOption)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _fileStorage = fileStorage;
        _imgOption = imgOption.Value;
    }

    //Las solicitudes del cliente que esta entrando, de la mas nueva a la mas vieja
    public async Task<ActionResponse<IEnumerable<MyServiceRequestItemDto>>> GetAsync(string username)
    {
        try
        {
            var client = await GetLoggedClientAsync(username);
            if (client == null)
            {
                return Fail<IEnumerable<MyServiceRequestItemDto>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            var list = await _context.ServiceRequests
                .AsNoTracking()
                .Where(x => x.CorporationId == client.CorporationId &&
                            x.ContractClient!.ClientId == client.ClientId &&
                            x.Active)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(100)
                .Select(x => new MyServiceRequestItemDto
                {
                    ServiceRequestId = x.ServiceRequestId,
                    RequestNumber = x.RequestNumber,
                    CreatedAtUtc = x.CreatedAtUtc,
                    ScheduledAtUtc = x.ScheduledAtUtc,
                    CompletedAtUtc = x.CompletedAtUtc,
                    ScheduleStatus = x.ScheduleStatus,
                    ControlContrato = x.ControlContrato,
                    Address = x.Address,
                    ZoneName = x.ZoneName,
                    ClientReason = x.ClientReason,
                    TechnicianName = x.Technician == null
                        ? null
                        : x.Technician.FirstName + " " + x.Technician.LastName,
                    TechnicianComment = x.TechnicianComment,
                    Recommendation = x.Recommendation,

                    //Solo las del despues: la prueba del trabajo hecho
                    Photos = x.ServiceRequestPhotos!
                        .Where(f => f.PhotoType == ServicePhotoType.After)
                        .OrderBy(f => f.DateCreated)
                        .Select(f => new MyServiceRequestPhotoDto
                        {
                            ServiceRequestPhotoId = f.ServiceRequestPhotoId,
                            DateCreated = f.DateCreated,

                            //El nombre del blob viaja aqui y se cambia por el enlace abajo
                            ImageFullPath = f.Photo
                        })
                        .ToList()
                })
                .ToListAsync();

            //Los enlaces del blob duran poco: se firman al momento de mostrarlos
            foreach (var photo in list.SelectMany(x => x.Photos))
            {
                photo.ImageFullPath = await GetPhotoUrlAsync(photo.ImageFullPath);
            }

            return new ActionResponse<IEnumerable<MyServiceRequestItemDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<MyServiceRequestItemDto>>(ex);
        }
    }

    //Sus contratos, para elegir sobre cual pide la visita. Con el neutro que arma el backend.
    public async Task<ActionResponse<IEnumerable<MyContractItemDto>>> GetMyContractsAsync(string username)
    {
        try
        {
            var client = await GetLoggedClientAsync(username);
            if (client == null)
            {
                return Fail<IEnumerable<MyContractItemDto>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            //Solo los contratos que tienen o tuvieron internet: un borrador o uno anulado
            //no tiene datos de conexion, asi que no hay visita que pedirle.
            var list = await _context.ContractClients
                .AsNoTracking()
                .Where(x => x.CorporationId == client.CorporationId &&
                            x.ClientId == client.ClientId &&
                            (x.ContractState == ContractState.Active ||
                             x.ContractState == ContractState.Suspended ||
                             x.ContractState == ContractState.Exempt))
                .OrderBy(x => x.ControlContrato)
                .Select(x => new MyContractItemDto
                {
                    ContractClientId = x.ContractClientId,
                    ControlContrato = x.ControlContrato,
                    Address = x.Address,
                    CityName = x.Zone!.City!.Name,
                    ZoneName = x.Zone.ZoneName,
                    PhoneNumber = x.PhoneNumber,
                    PlanName = x.ContractPlans!.Select(p => p.Plan!.PlanName).FirstOrDefault()
                })
                .ToListAsync();

            list.Insert(0, new MyContractItemDto
            {
                ContractClientId = Guid.Empty,
                Address = _localizer[nameof(Resource.Select_Contract)]
            });

            return new ActionResponse<IEnumerable<MyContractItemDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<MyContractItemDto>>(ex);
        }
    }

    //Pedir la visita: sin tecnico, sin fecha y sin cita en el calendario.
    //Nace en Requested y la oficina decide despues si va alguien o se resuelve llamando.
    public async Task<ActionResponse<bool>> AddAsync(MyServiceRequestDto dto, string username)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            var client = await GetLoggedClientAsync(username);
            if (client == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);
            }

            if (string.IsNullOrWhiteSpace(dto.ClientReason))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer["MyRequest_NeedProblem"]);
            }

            //Solo sobre un contrato suyo
            var contract = await _context.ContractClients
                .Include(x => x.Zone)
                    .ThenInclude(x => x!.City)
                .Include(x => x.ContractPlans!)
                    .ThenInclude(x => x.Plan)
                .Include(x => x.ContractServers!)
                    .ThenInclude(x => x.Server)
                        .ThenInclude(x => x!.IpNetwork)
                .Include(x => x.ContractIps!)
                    .ThenInclude(x => x.IpNet)
                .Include(x => x.ContractMacs!)
                    .ThenInclude(x => x.CargueDetail)
                .Include(x => x.ContractNodes!)
                    .ThenInclude(x => x.Node)
                        .ThenInclude(x => x!.IpNetwork)
                .FirstOrDefaultAsync(x => x.ContractClientId == dto.ContractClientId &&
                                          x.CorporationId == client.CorporationId &&
                                          x.ClientId == client.ClientId &&
                                          (x.ContractState == ContractState.Active ||
                                           x.ContractState == ContractState.Suspended ||
                                           x.ContractState == ContractState.Exempt));

            if (contract == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            //Con que numero llamarlo: el que dio ahora, o el del contrato
            var contactPhone = string.IsNullOrWhiteSpace(dto.ContactPhone)
                ? contract.PhoneNumber
                : dto.ContactPhone.Trim();

            //Si pidio dejarlo en su contrato, se cambia y queda en la bitacora del contrato
            if (dto.UpdateContractPhone &&
                !string.IsNullOrWhiteSpace(dto.ContactPhone) &&
                contactPhone != contract.PhoneNumber)
            {
                var anterior = contract.PhoneNumber;
                contract.PhoneNumber = contactPhone;

                await ContractAuditLog.AddAsync(_context, contract.ContractClientId, ContractEventType.Updated,
                    $"Telefono: {anterior} -> {contactPhone}", client.FullName, client.UserId,
                    clientId: contract.ClientId, corporationId: contract.CorporationId);
            }

            //La foto del momento: lo mismo que guarda la oficina al registrar una visita
            var plan = contract.ContractPlans?.FirstOrDefault()?.Plan;
            var server = contract.ContractServers?.FirstOrDefault()?.Server;
            var ip = contract.ContractIps?.FirstOrDefault()?.IpNet;
            var mac = contract.ContractMacs?.FirstOrDefault()?.CargueDetail;
            var node = contract.ContractNodes?.FirstOrDefault()?.Node;

            var nextNumber = await _context.ServiceRequests
                .Where(x => x.CorporationId == contract.CorporationId)
                .MaxAsync(x => (long?)x.RequestNumber) ?? 0;

            _context.ServiceRequests.Add(new ServiceRequest
            {
                RequestNumber = nextNumber + 1,
                CreatedAtUtc = DateTime.UtcNow,
                ContractClientId = contract.ContractClientId,
                ScheduleStatus = ScheduleStatus.Requested,
                Origin = ServiceRequestOrigin.Client,
                ClientReason = dto.ClientReason.Trim(),
                ContactPhone = contactPhone,
                CorporationId = contract.CorporationId,
                UserId = client.UserId,
                UsuarioOwner = client.FullName,

                //Foto del contrato al momento de pedirla
                ControlContrato = contract.ControlContrato,
                ClientFullName = client.FullName,
                PhoneNumber = contract.PhoneNumber,
                Address = contract.Address,
                CityName = contract.Zone?.City?.Name,
                ZoneName = contract.Zone?.ZoneName,
                PlanName = plan?.PlanName,
                PlanSpeed = plan?.VelocidadTotal,
                ServerName = server?.ServerName,
                IpServer = server?.IpNetwork?.Ip,
                IpCliente = ip?.Ip,
                MacCliente = mac?.MacWlan,
                NodeName = node?.NodesName,
                NodeIp = node?.IpNetwork?.Ip,
                Active = true
            });

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

    //El enlace firmado de la foto, valido por unos minutos
    private async Task<string?> GetPhotoUrlAsync(string? photo)
    {
        if (string.IsNullOrWhiteSpace(photo))
            return _imgOption.ImgNoImage;

        return await _fileStorage.GetBlobSasUrlAsync(photo, _imgOption.RequiereServicePicture, TimeSpan.FromMinutes(5));
    }

    //Quien esta entrando: tiene que ser un cliente activo de la corporacion
    private async Task<LoggedClient?> GetLoggedClientAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        if (user == null)
            return null;

        var isClient = await _context.UserRoleDetails
            .AnyAsync(x => x.UserId == user.Id && x.UserType == UserType.Client);

        if (!isClient)
            return null;

        return await _context.Clients
            .AsNoTracking()
            .Where(x => x.UserName == user.UserName &&
                        x.CorporationId == user.CorporationId &&
                        x.Active)
            .Select(x => new LoggedClient
            {
                ClientId = x.ClientId,
                CorporationId = x.CorporationId,
                FullName = x.FirstName + " " + x.LastName,
                UserId = Guid.Parse(user.Id)
            })
            .FirstOrDefaultAsync();
    }

    private static ActionResponse<T> Fail<T>(string message)
    {
        return new ActionResponse<T> { WasSuccess = false, Message = message };
    }

    //Los cuatro datos del cliente que este servicio necesita
    private class LoggedClient
    {
        public Guid ClientId { get; set; }

        public int CorporationId { get; set; }

        public string FullName { get; set; } = null!;

        public Guid UserId { get; set; }
    }
}
