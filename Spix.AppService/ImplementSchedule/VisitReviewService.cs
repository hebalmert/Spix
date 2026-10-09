using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.ImplementContratos;
using Spix.AppService.InterfaceSchedule;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementSchedule;

//Bandeja de revision: visitas que esperan una decision de la oficina. Dos listas,
//porque son dos problemas: Ubicacion (marco lejos) y Sin cliente (hay que reagendar).
//Cada visita cae en UNA sola. La ubicacion nueva no pasa por aqui: se aplico sola.
public class VisitReviewService : IVisitReviewService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public VisitReviewService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    //Los dos numeros del menu, en una sola pasada a SQL (no traer la lista para contarla)
    public async Task<ActionResponse<VisitReviewCountersDto>> GetCountersAsync(string username)
    {
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                return AuthFail<VisitReviewCountersDto>();
            }

            var corporationId = Convert.ToInt32(user.CorporationId);

            var resultado = await _context.ServiceRequests.AsNoTracking()
                .Where(x => x.CorporationId == corporationId && x.Active && !x.LocationReviewed)
                .GroupBy(x => 1)
                .Select(g => new VisitReviewCountersDto
                {
                    Location = g.Count(x => !x.ClientAbsent &&
                                            x.DistanceMeters != null &&
                                            x.DistanceMeters > GeoHelper.MargenMetros),
                    Absent = g.Count(x => x.ClientAbsent)
                })
                .FirstOrDefaultAsync();

            return new ActionResponse<VisitReviewCountersDto>
            {
                WasSuccess = true,
                Result = resultado ?? new VisitReviewCountersDto()
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<VisitReviewCountersDto>(ex);
        }
    }

    //Pestaña Ubicacion: marco lejos y el cliente SI estaba
    public async Task<ActionResponse<IEnumerable<VisitReviewDto>>> GetLocationAsync(PaginationDTO pagination, string username)
    {
        return await ListarAsync(pagination, username, soloAusentes: false);
    }

    //Pestaña Sin cliente: salen TODAS. La que si fue al sitio igual hay que reagendarla
    public async Task<ActionResponse<IEnumerable<VisitReviewDto>>> GetAbsentAsync(PaginationDTO pagination, string username)
    {
        return await ListarAsync(pagination, username, soloAusentes: true);
    }

    //Aplica la coordenada del tecnico al contrato: evita transcribir numeros a mano
    //en DetailContractControl. Queda en la bitacora con quien lo aplico.
    public async Task<ActionResponse<bool>> ApplyLocationAsync(Guid serviceRequestId, string username)
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

            var corporationId = Convert.ToInt32(user.CorporationId);

            var visita = await _context.ServiceRequests
                .FirstOrDefaultAsync(x => x.ServiceRequestId == serviceRequestId &&
                                          x.CorporationId == corporationId &&
                                          x.Active);

            if (visita == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (visita.Latitude is null || visita.Longitude is null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>("La visita no tiene coordenada que aplicar.");
            }

            var mapa = await _context.ContractMaps
                .FirstOrDefaultAsync(x => x.ContractClientId == visita.ContractClientId);

            if (mapa == null)
            {
                mapa = new ContractMap { ContractClientId = visita.ContractClientId };
                _context.ContractMaps.Add(mapa);
            }

            mapa.Latitude = visita.Latitude;
            mapa.Longitude = visita.Longitude;

            //Ya quedan en el mismo sitio: la diferencia desaparece
            visita.DistanceMeters = 0;
            visita.LocationReviewed = true;

            await ContractAuditLog.AddAsync(_context, visita.ContractClientId,
                ContractEventType.LocationUpdated,
                $"{visita.Latitude}, {visita.Longitude} (visita #{visita.RequestNumber})",
                $"{user.FirstName} {user.LastName}".Trim(),
                Guid.TryParse(user.Id, out var uid) ? uid : (Guid?)null,
                referenceId: visita.ServiceRequestId,
                corporationId: corporationId);

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

    //Descartar: el contrato queda como esta y la visita sale de la bandeja. No borra la
    //diferencia (sigue en la visita y en la bitacora) porque sirve para revisar al tecnico.
    public async Task<ActionResponse<bool>> DismissAsync(Guid serviceRequestId, string username)
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

            var corporationId = Convert.ToInt32(user.CorporationId);

            var visita = await _context.ServiceRequests
                .FirstOrDefaultAsync(x => x.ServiceRequestId == serviceRequestId &&
                                          x.CorporationId == corporationId &&
                                          x.Active);

            if (visita == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            visita.LocationReviewed = true;

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

    //===== Lo de adentro =====

    //Las dos pestañas leen lo mismo y cambian una condicion, por eso una sola consulta
    private async Task<ActionResponse<IEnumerable<VisitReviewDto>>> ListarAsync(PaginationDTO pagination, string username, bool soloAusentes)
    {
        try
        {
            var user = await GetUserAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<VisitReviewDto>>();
            }

            var corporationId = Convert.ToInt32(user.CorporationId);

            var queryable = _context.ServiceRequests.AsNoTracking()
                .Where(x => x.CorporationId == corporationId &&
                            x.Active &&
                            !x.LocationReviewed &&
                            x.ClientAbsent == soloAusentes)
                .AsQueryable();

            //En la pestaña de ubicacion solo tiene sentido lo que quedo lejos
            if (!soloAusentes)
            {
                queryable = queryable.Where(x => x.DistanceMeters != null &&
                                                 x.DistanceMeters > GeoHelper.MargenMetros);
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

            //Lo mas viejo primero: lleva mas tiempo sin que nadie lo atienda
            var crudo = await queryable
                .OrderBy(x => x.CompletedAtUtc ?? x.CreatedAtUtc)
                .Paginate(pagination)
                .Select(x => new
                {
                    x.ServiceRequestId,
                    x.ContractClientId,
                    x.RequestNumber,
                    x.ControlContrato,
                    x.ClientFullName,
                    x.Address,
                    x.Origin,
                    x.CompletedAtUtc,
                    x.CreatedAtUtc,
                    x.TechnicianComment,
                    x.Latitude,
                    x.Longitude,
                    x.DistanceMeters,
                    x.ClientAbsent,
                    TechnicianName = x.Technician == null
                        ? null
                        : x.Technician.FirstName + " " + x.Technician.LastName
                })
                .ToListAsync();

            if (crudo.Count == 0)
            {
                return new ActionResponse<IEnumerable<VisitReviewDto>>
                {
                    WasSuccess = true,
                    Result = new List<VisitReviewDto>()
                };
            }

            //Lo que falta se trae de una pasada y solo para los contratos de ESTA pagina:
            //una consulta por fila no aguanta miles de visitas
            var contratos = crudo.Select(x => x.ContractClientId).Distinct().ToList();
            var visitas = crudo.Select(x => x.ServiceRequestId).Distinct().ToList();

            var mapas = await _context.ContractMaps.AsNoTracking()
                .Where(x => contratos.Contains(x.ContractClientId))
                .Select(x => new { x.ContractClientId, x.Latitude, x.Longitude })
                .ToListAsync();

            //Los intentos: para saber si esta es la primera, la segunda o la tercera
            var intentos = await _context.ServiceRequests.AsNoTracking()
                .Where(x => contratos.Contains(x.ContractClientId) && x.Active && x.ClientAbsent)
                .Select(x => new { x.ContractClientId, x.ServiceRequestId, x.CreatedAtUtc })
                .ToListAsync();

            //Las que ya tienen una visita nueva colgada: a esas no se les vuelve a ofrecer
            var reagendadas = await _context.ServiceRequests.AsNoTracking()
                .Where(x => x.ServiceRequestParentId != null &&
                            visitas.Contains(x.ServiceRequestParentId.Value) &&
                            x.Active)
                .Select(x => x.ServiceRequestParentId!.Value)
                .ToListAsync();

            var lista = new List<VisitReviewDto>();

            foreach (var item in crudo)
            {
                var mapa = mapas.FirstOrDefault(x => x.ContractClientId == item.ContractClientId);

                var cadena = intentos
                    .Where(x => x.ContractClientId == item.ContractClientId)
                    .OrderBy(x => x.CreatedAtUtc)
                    .ToList();

                var posicion = cadena.FindIndex(x => x.ServiceRequestId == item.ServiceRequestId);

                lista.Add(new VisitReviewDto
                {
                    ServiceRequestId = item.ServiceRequestId,
                    ContractClientId = item.ContractClientId,
                    RequestNumber = item.RequestNumber,
                    ControlContrato = item.ControlContrato,
                    ClientFullName = item.ClientFullName,
                    Address = item.Address,
                    TechnicianName = item.TechnicianName,
                    OriginName = _localizer[$"ServiceRequestOrigin_{item.Origin}"],
                    CompletedAtUtc = item.CompletedAtUtc ?? item.CreatedAtUtc,
                    TechnicianComment = item.TechnicianComment,
                    ContractLatitude = mapa?.Latitude,
                    ContractLongitude = mapa?.Longitude,
                    VisitLatitude = item.Latitude,
                    VisitLongitude = item.Longitude,
                    DistanceMeters = item.DistanceMeters,
                    SameSite = GeoHelper.MismoSitio(item.DistanceMeters),
                    ClientAbsent = item.ClientAbsent,
                    Attempt = posicion < 0 ? 1 : posicion + 1,
                    AlreadyRescheduled = reagendadas.Contains(item.ServiceRequestId)
                });
            }

            return new ActionResponse<IEnumerable<VisitReviewDto>> { WasSuccess = true, Result = lista };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<VisitReviewDto>>(ex);
        }
    }

    private async Task<User?> GetUserAsync(string username) => await _userHelper.GetUserByUserNameAsync(username);

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
