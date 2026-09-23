using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos;

public class RunSuspendedService : IRunSuspendedService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;
    private readonly IContractActivationIntegrityService _contractActivationIntegrityService;

    public RunSuspendedService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer,
        IEnumMultilLanguageService enumMultilLanguageService,
        IContractActivationIntegrityService contractActivationIntegrityService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _enumMultilLanguageService = enumMultilLanguageService;
        _contractActivationIntegrityService = contractActivationIntegrityService;
    }

    public async Task<ActionResponse<IEnumerable<RunSuspended>>> GetAsync(
        PaginationDTO pagination,
        string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<RunSuspended>>();
            }

            var queryable = _context.RunSuspendeds
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.YearNumber.ToString(), $"%{filter}%") ||
                    EF.Functions.Like(x.MonthType.ToString(), $"%{filter}%") ||
                    EF.Functions.Like(x.UserByName!, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderByDescending(x => x.YearNumber)
                .ThenByDescending(x => x.MonthType)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<RunSuspended>>
            {
                WasSuccess = true,
                Result = list
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<RunSuspended>>(ex);
        }
    }

    public async Task<ActionResponse<RunSuspended>> GetByIdAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<RunSuspended>();
            }

            //Solo el encabezado: el detalle se pide aparte y paginado
            var model = await _context.RunSuspendeds
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RunSuspendedId == id &&
                                          x.CorporationId == user.CorporationId);
            if (model == null)
            {
                return Fail<RunSuspended>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            return new ActionResponse<RunSuspended>
            {
                WasSuccess = true,
                Result = model
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<RunSuspended>(ex);
        }
    }

    public async Task<ActionResponse<RunSuspended>> AddAsync(RunSuspended model, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<RunSuspended>();
            }

            if (!IsValidPeriod(model.YearNumber, model.MonthType))
            {
                return Fail<RunSuspended>("Debe seleccionar un mes y a\u00f1o validos.");
            }

            var corporationId = Convert.ToInt32(user.CorporationId);
            var exists = await PeriodExistsAsync(Guid.Empty, corporationId, model.YearNumber, model.MonthType);
            if (exists)
            {
                return Fail<RunSuspended>("Ya existe un corte general para ese mes y a\u00f1o.");
            }

            model.CorporationId = corporationId;
            model.Executed = false;
            model.DateUtc = null;
            model.UserId = null;
            model.UserByName = null;

            _context.RunSuspendeds.Add(model);
            await _context.SaveChangesAsync();

            return new ActionResponse<RunSuspended>
            {
                WasSuccess = true,
                Result = model
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<RunSuspended>(ex);
        }
    }

    public async Task<ActionResponse<RunSuspended>> UpdateAsync(RunSuspended model, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<RunSuspended>();
            }

            var current = await _context.RunSuspendeds
                .FirstOrDefaultAsync(x => x.RunSuspendedId == model.RunSuspendedId &&
                                          x.CorporationId == user.CorporationId);
            if (current == null)
            {
                return Fail<RunSuspended>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (current.Executed)
            {
                return Fail<RunSuspended>(_localizer["Corte_ExecutedNoEdit"]);
            }

            if (!IsValidPeriod(model.YearNumber, model.MonthType))
            {
                return Fail<RunSuspended>("Debe seleccionar un mes y a\u00f1o validos.");
            }

            var exists = await PeriodExistsAsync(
                current.RunSuspendedId,
                current.CorporationId,
                model.YearNumber,
                model.MonthType);
            if (exists)
            {
                return Fail<RunSuspended>("Ya existe un corte general para ese mes y a\u00f1o.");
            }

            current.YearNumber = model.YearNumber;
            current.MonthType = model.MonthType;
            await _context.SaveChangesAsync();

            return new ActionResponse<RunSuspended>
            {
                WasSuccess = true,
                Result = current
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<RunSuspended>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<bool>();
            }

            var model = await _context.RunSuspendeds
                .FirstOrDefaultAsync(x => x.RunSuspendedId == id &&
                                          x.CorporationId == user.CorporationId);
            if (model == null)
            {
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (model.Executed)
            {
                return Fail<bool>(_localizer["Corte_ExecutedNoDelete"]);
            }

            _context.RunSuspendeds.Remove(model);
            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    //Revision previa: cuantos deben, a cuantos se les corta y como quedan repartidos por
    //equipo. Los numeros los cuenta la base: aqui NO se traen los contratos, porque con
    //mil clientes seria cargar mil filas para mostrar cuatro numeros.
    public async Task<ActionResponse<CorteCheckDto>> CheckAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<CorteCheckDto>();
            }

            var corporationId = Convert.ToInt32(user.CorporationId);
            var run = await _context.RunSuspendeds
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RunSuspendedId == id && x.CorporationId == corporationId);

            if (run == null)
            {
                return Fail<CorteCheckDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            //Todo sale de las notas vivas: las que tienen saldo y no estan anuladas
            var pendientes = _context.CxCBills
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && !x.Cancelled && x.Balance > 0);

            //1. Cuantos contratos deben y cuanto suman, en una sola pasada
            var totales = await pendientes
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Contracts = g.Select(x => x.ContractClientId).Distinct().Count(),
                    Debt = g.Sum(x => x.Balance)
                })
                .FirstOrDefaultAsync();

            //2. De esos, los que ya estan suspendidos: se dejan como estan
            var yaSuspendidos = await pendientes
                .Where(x => x.ContractClient!.ContractState == ContractState.Suspended ||
                            _context.ContractSuspendeds.Any(s => s.ContractClientId == x.ContractClientId &&
                                                                 s.DateReactivated == null))
                .Select(x => x.ContractClientId)
                .Distinct()
                .CountAsync();

            //3. Los que si se van a cortar, agrupados por equipo. Devuelve una fila por
            //   servidor, no una por cliente.
            var servers = await pendientes
                .Where(x => x.ContractClient!.ContractState != ContractState.Suspended &&
                            !_context.ContractSuspendeds.Any(s => s.ContractClientId == x.ContractClientId &&
                                                                  s.DateReactivated == null))
                .Select(x => new
                {
                    x.ContractClientId,
                    x.Balance,
                    ServerId = x.ContractClient!.ContractBinds!.Select(b => (Guid?)b.ServerId).FirstOrDefault(),
                    ServerName = x.ContractClient!.ContractBinds!.Select(b => b.Server!.ServerName).FirstOrDefault()
                })
                .GroupBy(x => new { x.ServerId, x.ServerName })
                .Select(g => new CorteCheckServerDto
                {
                    ServerId = g.Key.ServerId ?? Guid.Empty,
                    ServerName = g.Key.ServerName,
                    Contracts = g.Select(x => x.ContractClientId).Distinct().Count(),
                    Debt = g.Sum(x => x.Balance)
                })
                .ToListAsync();

            //Los que no tienen IpBinding no viven en ningun equipo
            foreach (var server in servers.Where(x => string.IsNullOrWhiteSpace(x.ServerName)))
            {
                server.ServerName = _localizer["Corte_NoServer"];
            }

            var dto = new CorteCheckDto
            {
                Executed = run.Executed,
                Debtors = totales?.Contracts ?? 0,
                AlreadySuspended = yaSuspendidos,
                Servers = servers.OrderBy(x => x.ServerName).ToList()
            };

            dto.ToSuspend = dto.Servers.Sum(x => x.Contracts);
            dto.DebtTotal = dto.Servers.Sum(x => x.Debt);
            dto.NoServer = dto.Servers.FirstOrDefault(x => x.ServerId == Guid.Empty)?.Contracts ?? 0;

            return new ActionResponse<CorteCheckDto> { WasSuccess = true, Result = dto };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<CorteCheckDto>(ex);
        }
    }

    //Lo que quedo cortado, pagina por pagina: un corte de mil contratos no se trae de una
    public async Task<ActionResponse<IEnumerable<CorteDetailDto>>> GetDetailsAsync(Guid id, PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<CorteDetailDto>>();
            }

            var corporationId = Convert.ToInt32(user.CorporationId);
            var queryable = _context.RunSuspendedDetails
                .AsNoTracking()
                .Where(x => x.RunSuspendedId == id && x.RunSuspended!.CorporationId == corporationId)
                .Select(x => new CorteDetailDto
                {
                    ControlContrato = x.ContractClient!.ControlContrato,
                    ClientFullName = x.Client!.FirstName + " " + x.Client.LastName,
                    CollectionNote = x.CxCBill!.CollectionNote,
                    Debt = x.CxCBill.Balance,
                    PlanAmount = x.PlanAmount
                });

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderBy(x => x.ControlContrato)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<CorteDetailDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<CorteDetailDto>>(ex);
        }
    }

    //Corta TODOS los contratos deudores de UN servidor: se abre la conexion del equipo una
    //sola vez, se les quita el acceso a todos y se cierra. Cada equipo va en su propia
    //transaccion: si uno no responde, lo de los demas ya quedo hecho y el corte sigue
    //abierto para reintentarlo sin repetir a nadie.
    public async Task<ActionResponse<CorteRunResultDto>> RunServerAsync(Guid id, Guid serverId, string username)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await transaction.RollbackAsync();
                return AuthFail<CorteRunResultDto>();
            }

            var corporationId = Convert.ToInt32(user.CorporationId);
            var run = await _context.RunSuspendeds
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RunSuspendedId == id && x.CorporationId == corporationId);

            if (run == null)
            {
                await transaction.RollbackAsync();
                return Fail<CorteRunResultDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            if (run.Executed)
            {
                await transaction.RollbackAsync();
                return Fail<CorteRunResultDto>(_localizer["Corte_AlreadyExecuted"]);
            }

            //Los contratos que viven en ese equipo. El vacio es para los que no tienen
            //IpBinding, que no viven en ningun equipo.
            var delServidor = serverId == Guid.Empty
                ? await _context.ContractClients
                    .AsNoTracking()
                    .Where(x => x.CorporationId == corporationId && !x.ContractBinds!.Any())
                    .Select(x => x.ContractClientId)
                    .ToListAsync()
                : await _context.ContractBinds
                    .AsNoTracking()
                    .Where(x => x.ServerId == serverId && x.ContractClient!.CorporationId == corporationId)
                    .Select(x => x.ContractClientId)
                    .Distinct()
                    .ToListAsync();

            //La deuda se vuelve a mirar aqui: entre la revision y el corte alguien pudo pagar.
            //Solo se miran los contratos de este equipo.
            var deudas = await DebtorsAsync(corporationId, delServidor);

            var idsDelLote = delServidor.Where(deudas.ContainsKey).ToList();

            var result = new CorteRunResultDto { Contracts = idsDelLote.Count };

            if (idsDelLote.Count == 0)
            {
                await transaction.CommitAsync();
                return new ActionResponse<CorteRunResultDto> { WasSuccess = true, Result = result };
            }

            //Los que ya estan suspendidos se quedan como estan
            var suspendidos = await SuspendedContractsAsync(corporationId);
            idsDelLote = idsDelLote.Where(x => !suspendidos.Contains(x)).ToList();
            result.Skipped = result.Contracts - idsDelLote.Count;

            if (idsDelLote.Count == 0)
            {
                await transaction.CommitAsync();
                return new ActionResponse<CorteRunResultDto> { WasSuccess = true, Result = result };
            }

            //Sin Includes de coleccion: el plan va aparte
            var contracts = await _context.ContractClients
                .Include(x => x.Client)
                .Where(x => x.CorporationId == corporationId &&
                            idsDelLote.Contains(x.ContractClientId))
                .ToListAsync();

            result.Skipped += idsDelLote.Count - contracts.Count;

            if (contracts.Count == 0)
            {
                await transaction.CommitAsync();
                return new ActionResponse<CorteRunResultDto> { WasSuccess = true, Result = result };
            }

            //Si la corporacion controla el acceso por HotSpot, primero se quita en el equipo
            var usesHotSpotControl = await _contractActivationIntegrityService
                .UsesHotSpotControlAsync(corporationId);

            if (usesHotSpotControl)
            {
                if (serverId == Guid.Empty)
                {
                    //Sin IpBinding no hay como quitarle el acceso: se reportan y no se tocan
                    foreach (var contract in contracts)
                    {
                        result.Issues.Add(NewIssue(contract, _localizer["Corte_NoBinding"]));
                    }

                    await transaction.CommitAsync();
                    return new ActionResponse<CorteRunResultDto> { WasSuccess = true, Result = result };
                }

                var connectionResponse = await _contractActivationIntegrityService
                    .VerifyHotSpotServersConnectionAsync(contracts.Select(x => x.ContractClientId).ToList());

                if (!connectionResponse.WasSuccess)
                {
                    await transaction.RollbackAsync();
                    return Fail<CorteRunResultDto>(connectionResponse.Message!);
                }

                //Una sola conexion al equipo para todos sus contratos
                var suspendResponse = await _contractActivationIntegrityService
                    .SuspendHotSpotBindingsAsync(contracts);

                if (!suspendResponse.WasSuccess)
                {
                    await transaction.RollbackAsync();
                    return Fail<CorteRunResultDto>(suspendResponse.Message!);
                }
            }

            //El valor del plan al momento del corte, en una sola consulta
            var planes = (await _context.ContractPlans
                .AsNoTracking()
                .Where(x => idsDelLote.Contains(x.ContractClientId))
                .Select(x => new { x.ContractClientId, x.Plan!.Price })
                .ToListAsync())
                .GroupBy(x => x.ContractClientId)
                .ToDictionary(x => x.Key, x => x.First().Price);

            var utcNow = DateTime.UtcNow;
            var userName = $"{user.FirstName} {user.LastName}";
            var userId = Guid.Parse(user.Id);

            foreach (var contract in contracts)
            {
                var deuda = deudas[contract.ContractClientId];
                planes.TryGetValue(contract.ContractClientId, out var planAmount);

                contract.ContractState = ContractState.Suspended;

                _context.RunSuspendedDetails.Add(new RunSuspendedDetail
                {
                    RunSuspendedDetailId = Guid.NewGuid(),
                    RunSuspendedId = run.RunSuspendedId,
                    ContractClientId = contract.ContractClientId,
                    ClientId = contract.ClientId,
                    CxCBillId = deuda.CxCBillId,
                    DateUtc = utcNow,
                    PlanAmount = planAmount
                });
            }

            //Queda el registro de la suspension, igual que cuando se hace a mano
            await ContractSuspendedRegistry.OpenManyAsync(_context, contracts, SuspendedOrigin.Corte,
                null, run.RunSuspendedId, userName, userId);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            result.Suspended = contracts.Count;
            return new ActionResponse<CorteRunResultDto> { WasSuccess = true, Result = result };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return await _httpErrorHandler.HandleErrorAsync<CorteRunResultDto>(ex);
        }
    }

    //Los contratos que ya estan suspendidos: no se vuelven a cortar. Se miran los dos
    //lados, el registro de suspension abierto y el estado del contrato, que dicen lo mismo.
    private async Task<HashSet<Guid>> SuspendedContractsAsync(int corporationId)
    {
        var conRegistro = await _context.ContractSuspendeds
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId && x.DateReactivated == null)
            .Select(x => x.ContractClientId)
            .ToListAsync();

        var porEstado = await _context.ContractClients
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId && x.ContractState == ContractState.Suspended)
            .Select(x => x.ContractClientId)
            .ToListAsync();

        return conRegistro.Concat(porEstado).ToHashSet();
    }

    //Cierra el corte. Se reclama de forma atomica para que no lo cierren dos veces.
    public async Task<ActionResponse<RunSuspended>> FinishRunAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<RunSuspended>();
            }

            var corporationId = Convert.ToInt32(user.CorporationId);
            var utcNow = DateTime.UtcNow;
            var userName = $"{user.FirstName} {user.LastName}";
            var userId = Guid.Parse(user.Id);

            var claimed = await _context.RunSuspendeds
                .Where(x => x.RunSuspendedId == id && x.CorporationId == corporationId && !x.Executed)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Executed, true)
                    .SetProperty(x => x.DateUtc, utcNow)
                    .SetProperty(x => x.UserId, userId)
                    .SetProperty(x => x.UserByName, userName));

            if (claimed == 0)
            {
                return Fail<RunSuspended>(_localizer["Corte_AlreadyExecuted"]);
            }

            var run = await _context.RunSuspendeds
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RunSuspendedId == id && x.CorporationId == corporationId);

            return new ActionResponse<RunSuspended> { WasSuccess = true, Result = run };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<RunSuspended>(ex);
        }
    }

    //Todo lo que un contrato debe: CUALQUIER nota viva con saldo, del periodo que sea.
    //El corte no mira meses: si quedo debiendo, se corta. Asi nadie se queda consumiendo
    //internet sin pagar porque se le reactivo y se le paso pagar.
    private async Task<Dictionary<Guid, CorteDeuda>> DebtorsAsync(int corporationId, List<Guid>? soloEstos = null)
    {
        var queryable = _context.CxCBills
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        !x.Cancelled &&
                        x.Balance > 0);

        if (soloEstos != null)
        {
            queryable = queryable.Where(x => soloEstos.Contains(x.ContractClientId));
        }

        //Solo las columnas que se necesitan: ni el cliente ni el contrato hacen falta aqui
        var notas = await queryable
            .Select(x => new
            {
                x.CxCBillId,
                x.ContractClientId,
                x.CollectionNote,
                x.DateNote,
                x.Balance
            })
            .ToListAsync();

        return notas
            .GroupBy(x => x.ContractClientId)
            .ToDictionary(x => x.Key, x =>
            {
                var vieja = x.OrderBy(n => n.DateNote).First();
                return new CorteDeuda
                {
                    CxCBillId = vieja.CxCBillId,
                    CollectionNote = vieja.CollectionNote,
                    DateNote = vieja.DateNote,
                    Debt = x.Sum(n => n.Balance),
                    Notes = x.Count()
                };
            });
    }

    private static CorteRunIssueDto NewIssue(ContractClient contract, string reason) => new()
    {
        ContractClientId = contract.ContractClientId,
        ControlContrato = contract.ControlContrato,
        ClientFullName = $"{contract.Client?.FirstName} {contract.Client?.LastName}".Trim(),
        Reason = reason
    };

    //Lo que debe un contrato, resumido
    private class CorteDeuda
    {
        public Guid CxCBillId { get; set; }

        public string? CollectionNote { get; set; }

        public DateTime DateNote { get; set; }

        public decimal Debt { get; set; }

        public int Notes { get; set; }
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<IntItemModel>>();
            }

            var list = _enumMultilLanguageService.GetEnumSelectList<MonthType>("Select_Month");
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

    private async Task<ActionResponse<bool>> ValidateHotSpotConfigurationAsync(
        List<ContractClient> activeContracts)
    {
        var contractIds = activeContracts.Select(x => x.ContractClientId).ToList();
        var bindContractIds = await _context.ContractBinds
            .Where(x => contractIds.Contains(x.ContractClientId))
            .Select(x => x.ContractClientId)
            .Distinct()
            .ToListAsync();
        var queueContractIds = await _context.ContractQues
            .Where(x => contractIds.Contains(x.ContractClientId))
            .Select(x => x.ContractClientId)
            .Distinct()
            .ToListAsync();

        var bindIds = bindContractIds.ToHashSet();
        var queueIds = queueContractIds.ToHashSet();
        var contractsWithoutConfiguration = activeContracts
            .Where(x => !bindIds.Contains(x.ContractClientId) || !queueIds.Contains(x.ContractClientId))
            .Select(x => x.ControlContrato)
            .ToList();

        if (contractsWithoutConfiguration.Count == 0)
        {
            return Success<bool>();
        }

        var contractsText = string.Join(", ", contractsWithoutConfiguration.Take(10));
        var message = $"No se puede correr el corte general porque {contractsWithoutConfiguration.Count} contrato(s) activo(s) no tienen Contract Queue e IpBinding configurados. Contratos: {contractsText}.";
        return Fail<bool>(message);
    }

    private async Task<bool> PeriodExistsAsync(
        Guid runSuspendedId,
        int corporationId,
        int yearNumber,
        MonthType monthType)
    {
        return await _context.RunSuspendeds.AnyAsync(x =>
            x.RunSuspendedId != runSuspendedId &&
            x.CorporationId == corporationId &&
            x.YearNumber == yearNumber &&
            x.MonthType == monthType);
    }

    private static bool IsValidPeriod(int yearNumber, MonthType monthType)
    {
        return yearNumber >= 2000 && Enum.IsDefined(monthType);
    }

    private ActionResponse<T> AuthFail<T>()
    {
        return new ActionResponse<T>
        {
            WasSuccess = false,
            Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
        };
    }

    private static ActionResponse<T> Success<T>()
    {
        return new ActionResponse<T>
        {
            WasSuccess = true,
            Result = default
        };
    }

    private static ActionResponse<T> Fail<T>(string message)
    {
        return new ActionResponse<T>
        {
            WasSuccess = false,
            Message = message
        };
    }
}
