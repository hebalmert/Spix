using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.Sequences;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesBilling;
using Spix.AppService.InterfacesPayment;
using Spix.AppService.ImplementPayment;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesPayment;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementBilling;

public class BillingService : IBillingService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;
    private readonly IContractorPaymentService _contractorPaymentService;

    public BillingService(DataContext context, IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer,
        IEnumMultilLanguageService enumMultilLanguageService,
        IContractorPaymentService contractorPaymentService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _enumMultilLanguageService = enumMultilLanguageService;
        _contractorPaymentService = contractorPaymentService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<IntItemModel>>();

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

    //El tablero de notas generales: como va el ano y cuantos contratos entrarian hoy
    public async Task<ActionResponse<BillingNoteSummaryDto>> GetBillingNoteSummaryAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingNoteSummaryDto>();

            var year = DateTime.Today.Year;

            //Las notas del ano, contadas de una sola pasada
            var notas = await _context.BillingNotes
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId && x.YearNumber == year)
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Notes = g.Count(),
                    Launched = g.Count(x => x.Created)
                })
                .FirstOrDefaultAsync();

            var summary = new BillingNoteSummaryDto
            {
                YearNumber = year,
                Notes = notas?.Notes ?? 0,
                Launched = notas?.Launched ?? 0,
                ActiveContracts = await _context.ContractClients
                    .AsNoTracking()
                    .CountAsync(x => x.CorporationId == user.CorporationId &&
                                     x.ContractState == ContractState.Active)
            };

            summary.Pending = summary.Notes - summary.Launched;

            return new ActionResponse<BillingNoteSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<BillingNoteSummaryDto>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<BillingNote>>> GetBillingNotesAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<BillingNote>>();

            var queryable = _context.BillingNotes
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.YearNumber.ToString(), $"%{filter}%") ||
                    EF.Functions.Like(x.MonthType.ToString(), $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderByDescending(x => x.YearNumber)
                .ThenByDescending(x => x.MonthType)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<BillingNote>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<BillingNote>>(ex);
        }
    }

    //El tablero de notas individuales: como va el ano y cuanto se ha facturado por ellas
    public async Task<ActionResponse<BillingNoteSummaryDto>> GetBillingNoteOneSummaryAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingNoteSummaryDto>();

            var year = DateTime.Today.Year;

            var notas = await _context.BillingNoteOnes
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId && x.YearNumber == year)
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Notes = g.Count(),
                    Launched = g.Count(x => x.Created)
                })
                .FirstOrDefaultAsync();

            var summary = new BillingNoteSummaryDto
            {
                YearNumber = year,
                Notes = notas?.Notes ?? 0,
                Launched = notas?.Launched ?? 0,

                //Lo facturado por notas individuales, por el indice del periodo
                Billed = await _context.CxCBills
                    .AsNoTracking()
                    .Where(x => x.CorporationId == user.CorporationId &&
                                x.YearNumber == year &&
                                !x.Cancelled &&
                                x.BillingNoteOneId != null)
                    .SumAsync(x => (decimal?)x.Total) ?? 0
            };

            summary.Pending = summary.Notes - summary.Launched;

            return new ActionResponse<BillingNoteSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<BillingNoteSummaryDto>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<BillingNoteOne>>> GetBillingNoteOnesAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<BillingNoteOne>>();

            var queryable = _context.BillingNoteOnes.AsNoTracking()
                .Include(x => x.Client)
                .Include(x => x.ContractClient)
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.Client!.FirstName, $"%{filter}%") ||
                    EF.Functions.Like(x.Client!.LastName, $"%{filter}%") ||
                    EF.Functions.Like(x.ContractClient!.ControlContrato.ToString(), $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderByDescending(x => x.DateBill)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<BillingNoteOne>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<BillingNoteOne>>(ex);
        }
    }

    public async Task<ActionResponse<BillingNoteOne>> GetBillingNoteOneAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingNoteOne>();

            var model = await _context.BillingNoteOnes
                .Include(x => x.Client)
                .Include(x => x.ContractClient!)
                    .ThenInclude(x => x.Zone!)
                        .ThenInclude(x => x.City)
                .Include(x => x.ContractClient!)
                    .ThenInclude(x => x.ContractPlans!)
                        .ThenInclude(x => x.Plan)
                .FirstOrDefaultAsync(x => x.BillingNoteOneId == id &&
                                          x.CorporationId == user.CorporationId);

            if (model == null)
                return Fail<BillingNoteOne>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            return new ActionResponse<BillingNoteOne> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<BillingNoteOne>(ex);
        }
    }

    public async Task<ActionResponse<BillingNote>> GetBillingNoteAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingNote>();

            var model = await _context.BillingNotes
                .FirstOrDefaultAsync(x => x.BillingNoteId == id && x.CorporationId == user.CorporationId);

            if (model == null)
                return Fail<BillingNote>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            return new ActionResponse<BillingNote> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<BillingNote>(ex);
        }
    }

    public async Task<ActionResponse<BillingNote>> AddBillingNoteAsync(BillingNote model, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingNote>();

            if (model.DateBill == default)
                return Fail<BillingNote>("Debe seleccionar la fecha.");

            if (model.YearNumber <= 0)
                model.YearNumber = model.DateBill.Year;

            if (!Enum.IsDefined(model.MonthType))
                model.MonthType = (MonthType)model.DateBill.Month;
            model.Created = false;
            model.DateCreated = null;
            model.CorporationId = Convert.ToInt32(user.CorporationId);
            model.UserId = Guid.Parse(user.Id);
            model.UsuarioOwner = $"{user.FirstName} {user.LastName}";

            var exists = await _context.BillingNotes.AnyAsync(x =>
                x.CorporationId == model.CorporationId &&
                x.YearNumber == model.YearNumber &&
                x.MonthType == model.MonthType);

            if (exists)
                return Fail<BillingNote>("Ya existe una nota general para ese mes y año.");

            _context.BillingNotes.Add(model);
            await _context.SaveChangesAsync();

            return new ActionResponse<BillingNote> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<BillingNote>(ex);
        }
    }

    public async Task<ActionResponse<BillingNote>> UpdateBillingNoteAsync(BillingNote model, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingNote>();

            var current = await _context.BillingNotes
                .FirstOrDefaultAsync(x => x.BillingNoteId == model.BillingNoteId &&
                                          x.CorporationId == user.CorporationId);

            if (current == null)
                return Fail<BillingNote>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (current.Created)
                return Fail<BillingNote>("La nota general ya fue lanzada y no puede modificarse.");

            if (model.DateBill == default)
                return Fail<BillingNote>("Debe seleccionar la fecha.");

            if (model.YearNumber <= 0)
                model.YearNumber = model.DateBill.Year;

            if (!Enum.IsDefined(model.MonthType))
                model.MonthType = (MonthType)model.DateBill.Month;

            var exists = await _context.BillingNotes.AnyAsync(x =>
                x.BillingNoteId != current.BillingNoteId &&
                x.CorporationId == current.CorporationId &&
                x.YearNumber == model.YearNumber &&
                x.MonthType == model.MonthType);

            if (exists)
                return Fail<BillingNote>("Ya existe una nota general para ese mes y año.");

            current.DateBill = model.DateBill;
            current.YearNumber = model.YearNumber;
            current.MonthType = model.MonthType;

            await _context.SaveChangesAsync();

            return new ActionResponse<BillingNote> { WasSuccess = true, Result = current };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<BillingNote>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteBillingNoteAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<bool>();

            var model = await _context.BillingNotes
                .FirstOrDefaultAsync(x => x.BillingNoteId == id && x.CorporationId == user.CorporationId);

            if (model == null)
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (model.Created)
                return Fail<bool>("La nota general ya fue lanzada y no puede eliminarse.");

            _context.BillingNotes.Remove(model);
            await _context.SaveChangesAsync();

            return new ActionResponse<bool> { WasSuccess = true, Result = true };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    public async Task<ActionResponse<BillingNoteOne>> AddBillingNoteOneAsync(BillingNoteOne model, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingNoteOne>();

            if (model.ContractClientId == Guid.Empty)
                return Fail<BillingNoteOne>(_localizer["BillingOne_ContractNotActive"]);

            var contract = await _context.ContractClients
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ContractClientId == model.ContractClientId &&
                                          x.CorporationId == user.CorporationId &&
                                          x.ContractState == ContractState.Active);

            if (contract == null)
                return Fail<BillingNoteOne>(_localizer["BillingOne_ContractNotActive"]);

            model.ClientId = contract.ClientId;
            if (model.YearNumber <= 0)
                model.YearNumber = model.DateBill.Year;

            if (!Enum.IsDefined(model.MonthType))
                model.MonthType = (MonthType)model.DateBill.Month;
            model.Created = false;
            model.DateCreated = null;
            model.CorporationId = Convert.ToInt32(user.CorporationId);
            model.UserId = Guid.Parse(user.Id);
            model.UsuarioOwner = $"{user.FirstName} {user.LastName}";

            if (await HasBillingForPeriodAsync(
                model.ContractClientId,
                model.CorporationId,
                model.YearNumber,
                model.MonthType))
            {
                return Fail<BillingNoteOne>(_localizer["BillingOne_PeriodTaken"]);
            }

            _context.BillingNoteOnes.Add(model);
            await _context.SaveChangesAsync();

            return new ActionResponse<BillingNoteOne> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<BillingNoteOne>(ex);
        }
    }

    public async Task<ActionResponse<BillingNoteOne>> UpdateBillingNoteOneAsync(BillingNoteOne model, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingNoteOne>();

            var current = await _context.BillingNoteOnes
                .FirstOrDefaultAsync(x => x.BillingNoteOneId == model.BillingNoteOneId &&
                                          x.CorporationId == user.CorporationId);

            if (current == null)
                return Fail<BillingNoteOne>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (current.Created)
                return Fail<BillingNoteOne>(_localizer["BillingOne_LaunchedNoEdit"]);

            if (model.ContractClientId == Guid.Empty)
                return Fail<BillingNoteOne>(_localizer["BillingOne_ContractNotActive"]);

            var contract = await _context.ContractClients
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ContractClientId == model.ContractClientId &&
                                          x.CorporationId == user.CorporationId &&
                                          x.ContractState == ContractState.Active);

            if (contract == null)
                return Fail<BillingNoteOne>(_localizer["BillingOne_ContractNotActive"]);

            if (model.DateBill == default)
                return Fail<BillingNoteOne>(_localizer["BillingOne_DateRequired"]);

            if (model.YearNumber <= 0)
                model.YearNumber = model.DateBill.Year;

            if (!Enum.IsDefined(model.MonthType))
                model.MonthType = (MonthType)model.DateBill.Month;

            if (await HasBillingForPeriodAsync(
                model.ContractClientId,
                current.CorporationId,
                model.YearNumber,
                model.MonthType))
            {
                return Fail<BillingNoteOne>(_localizer["BillingOne_PeriodTaken"]);
            }

            current.DateBill = model.DateBill;
            current.ContractClientId = model.ContractClientId;
            current.ClientId = contract.ClientId;
            current.YearNumber = model.YearNumber;
            current.MonthType = model.MonthType;

            await _context.SaveChangesAsync();

            return new ActionResponse<BillingNoteOne> { WasSuccess = true, Result = current };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<BillingNoteOne>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteBillingNoteOneAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<bool>();

            var model = await _context.BillingNoteOnes
                .FirstOrDefaultAsync(x => x.BillingNoteOneId == id &&
                                          x.CorporationId == user.CorporationId);

            if (model == null)
                return Fail<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (model.Created)
                return Fail<bool>("La nota individual ya fue lanzada y no puede eliminarse.");

            _context.BillingNoteOnes.Remove(model);
            await _context.SaveChangesAsync();

            return new ActionResponse<bool> { WasSuccess = true, Result = true };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    //Revision previa al lanzamiento: recorre los contratos activos y dice cuales estan
    //incompletos y que les falta, para arreglarlos ANTES y que el lanzamiento no se interrumpa.
    //Todo sale de UNA consulta: con 200 contratos sigue siendo una sola ida a la base.
    public async Task<ActionResponse<IEnumerable<BillingCheckDto>>> CheckContractsAsync(int yearNumber, MonthType monthType, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<BillingCheckDto>>();

            var corporationId = Convert.ToInt32(user.CorporationId);

            var list = await _context.ContractClients
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && x.ContractState == ContractState.Active)
                .OrderBy(x => x.ControlContrato)
                .Select(x => new BillingCheckDto
                {
                    ContractClientId = x.ContractClientId,
                    ControlContrato = x.ControlContrato,
                    ClientFullName = x.Client!.FirstName + " " + x.Client.LastName,
                    ZoneName = x.Zone!.ZoneName,
                    HasPlan = x.ContractPlans!.Any(),
                    HasServer = x.ContractServers!.Any(),
                    HasIp = x.ContractIps!.Any(),
                    HasMac = x.ContractMacs!.Any(),
                    HasNode = x.ContractNodes!.Any(),
                    HasQueue = _context.ContractQues.Any(q => q.ContractClientId == x.ContractClientId),
                    HasBinding = _context.ContractBinds.Any(b => b.ContractClientId == x.ContractClientId),

                    //Ya facturado en el periodo que se va a lanzar
                    AlreadyBilled = _context.CxCBills.Any(c =>
                        c.CorporationId == corporationId &&
                        c.ContractClientId == x.ContractClientId &&
                        c.YearNumber == yearNumber &&
                        c.MonthType == monthType &&
                        !c.Cancelled)
                })
                .ToListAsync();

            return new ActionResponse<IEnumerable<BillingCheckDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<BillingCheckDto>>(ex);
        }
    }

    //Lanza las notas del periodo: recorre los contratos activos y, contrato por contrato,
    //arma su venta (plan + solicitudes sin facturar), aplica el pago adelantado o la exoneracion
    //y crea su nota de cobro.
    //
    //Un contrato con problema (sin plan) NO detiene el lote: se salta y se reporta al final,
    //para eso esta ademas la revision previa (CheckContractsAsync).
    public async Task<ActionResponse<BillingLaunchResultDto>> LaunchBillingNoteAsync(Guid id, string username)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingLaunchResultDto>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var note = await _context.BillingNotes
                .FirstOrDefaultAsync(x => x.BillingNoteId == id && x.CorporationId == corporationId);

            if (note == null)
                return Fail<BillingLaunchResultDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //Se reclama la nota en UNA sentencia: si dos personas la lanzan a la vez, solo una entra
            var claimed = await _context.BillingNotes
                .Where(x => x.BillingNoteId == id && !x.Created)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Created, true)
                    .SetProperty(x => x.DateCreated, DateTime.UtcNow.Date));

            if (claimed == 0)
                return Fail<BillingLaunchResultDto>(_localizer["Billing_AlreadyLaunched"]);

            var contracts = await GetBillableContractsQuery(corporationId)
                .OrderBy(x => x.ControlContrato)
                .AsSplitQuery()
                .ToListAsync();

            if (contracts.Count == 0)
            {
                await transaction.RollbackAsync();
                return Fail<BillingLaunchResultDto>(_localizer["Billing_NoContracts"]);
            }

            //Lo ya facturado del periodo, en UNA consulta: antes se preguntaba contrato por contrato
            var yaFacturados = await BilledContractsForPeriodAsync(corporationId, note.YearNumber, note.MonthType);

            var result = new BillingLaunchResultDto { Contracts = contracts.Count };

            foreach (var contract in contracts)
            {
                if (yaFacturados.Contains(contract.ContractClientId))
                {
                    result.Skipped++;
                    continue;
                }

                var response = await CreateBillingForContractAsync(contract, note.YearNumber, note.MonthType, note.BillingNoteId, null, user.Id, $"{user.FirstName} {user.LastName}");
                if (!response.WasSuccess)
                {
                    //Se anota y se sigue: el lote no se detiene por un contrato incompleto
                    result.Issues.Add(new BillingLaunchIssueDto
                    {
                        ContractClientId = contract.ContractClientId,
                        ControlContrato = contract.ControlContrato,
                        ClientFullName = $"{contract.Client!.FirstName} {contract.Client.LastName}",
                        Reason = response.Message ?? string.Empty
                    });
                    continue;
                }

                result.Created++;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ActionResponse<BillingLaunchResultDto> { WasSuccess = true, Result = result };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return await _httpErrorHandler.HandleErrorAsync<BillingLaunchResultDto>(ex);
        }
    }

    //Lanza UN LOTE de contratos de la nota. El front va pidiendo lote tras lote y muestra el
    //avance; cada lote se confirma solo, asi una caida de red no deja a medias una transaccion
    //de mil contratos: lo ya facturado queda, y al reintentar esos se saltan.
    public async Task<ActionResponse<BillingLaunchResultDto>> LaunchBatchAsync(Guid id, List<Guid> contractClientIds, string username)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingLaunchResultDto>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var note = await _context.BillingNotes
                .FirstOrDefaultAsync(x => x.BillingNoteId == id && x.CorporationId == corporationId);

            if (note == null)
                return Fail<BillingLaunchResultDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (note.Created)
                return Fail<BillingLaunchResultDto>(_localizer["Billing_AlreadyLaunched"]);

            var result = new BillingLaunchResultDto { Contracts = contractClientIds.Count };
            if (contractClientIds.Count == 0)
            {
                await transaction.RollbackAsync();
                return new ActionResponse<BillingLaunchResultDto> { WasSuccess = true, Result = result };
            }

            var contracts = await GetBillableContractsQuery(corporationId)
                .Where(x => contractClientIds.Contains(x.ContractClientId))
                .OrderBy(x => x.ControlContrato)
                .AsSplitQuery()
                .ToListAsync();

            //Lo ya facturado del periodo, solo para los contratos del lote
            var yaFacturados = await BilledContractsForPeriodAsync(corporationId, note.YearNumber, note.MonthType, contractClientIds);

            //Un contrato activo debe estar completo: plan, IP, MAC, servidor, nodo, queue e
            //ipbinding. Si le falta algo no se le cobra: se reporta para que lo completen.
            var incompletos = await IncompleteContractsAsync(corporationId, contractClientIds);

            foreach (var contract in contracts)
            {
                if (yaFacturados.Contains(contract.ContractClientId))
                {
                    result.Skipped++;
                    continue;
                }

                if (incompletos.TryGetValue(contract.ContractClientId, out var falta))
                {
                    result.Issues.Add(new BillingLaunchIssueDto
                    {
                        ContractClientId = contract.ContractClientId,
                        ControlContrato = contract.ControlContrato,
                        ClientFullName = $"{contract.Client!.FirstName} {contract.Client.LastName}",
                        Reason = $"{_localizer["Billing_Incomplete"]}: {falta}"
                    });
                    continue;
                }

                var response = await CreateBillingForContractAsync(contract, note.YearNumber, note.MonthType, note.BillingNoteId, null, user.Id, $"{user.FirstName} {user.LastName}");
                if (!response.WasSuccess)
                {
                    //Un contrato incompleto no detiene el lote: se anota y se sigue
                    result.Issues.Add(new BillingLaunchIssueDto
                    {
                        ContractClientId = contract.ContractClientId,
                        ControlContrato = contract.ControlContrato,
                        ClientFullName = $"{contract.Client!.FirstName} {contract.Client.LastName}",
                        Reason = response.Message ?? string.Empty
                    });
                    continue;
                }

                result.Created++;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ActionResponse<BillingLaunchResultDto> { WasSuccess = true, Result = result };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return await _httpErrorHandler.HandleErrorAsync<BillingLaunchResultDto>(ex);
        }
    }

    //Cierra la nota general cuando ya se recorrieron todos los lotes.
    //Se reclama en UNA sentencia: si dos la cierran a la vez, solo una entra.
    public async Task<ActionResponse<BillingNote>> FinishLaunchAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingNote>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var claimed = await _context.BillingNotes
                .Where(x => x.BillingNoteId == id && x.CorporationId == corporationId && !x.Created)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Created, true)
                    .SetProperty(x => x.DateCreated, DateTime.UtcNow.Date));

            if (claimed == 0)
                return Fail<BillingNote>(_localizer["Billing_AlreadyLaunched"]);

            var note = await _context.BillingNotes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BillingNoteId == id && x.CorporationId == corporationId);

            return new ActionResponse<BillingNote> { WasSuccess = true, Result = note! };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<BillingNote>(ex);
        }
    }

    //De los contratos del lote, cuales estan incompletos y que les falta. Una sola consulta.
    private async Task<Dictionary<Guid, string>> IncompleteContractsAsync(int corporationId, List<Guid> contractClientIds)
    {
        var datos = await _context.ContractClients
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId && contractClientIds.Contains(x.ContractClientId))
            .Select(x => new
            {
                x.ContractClientId,
                HasPlan = x.ContractPlans!.Any(),
                HasIp = x.ContractIps!.Any(),
                HasMac = x.ContractMacs!.Any(),
                HasServer = x.ContractServers!.Any(),
                HasNode = x.ContractNodes!.Any(),
                HasQueue = _context.ContractQues.Any(q => q.ContractClientId == x.ContractClientId),
                HasBinding = _context.ContractBinds.Any(b => b.ContractClientId == x.ContractClientId)
            })
            .ToListAsync();

        var incompletos = new Dictionary<Guid, string>();
        foreach (var dato in datos)
        {
            var faltas = new List<string>();
            if (!dato.HasPlan) faltas.Add("Plan");
            if (!dato.HasIp) faltas.Add("IP");
            if (!dato.HasMac) faltas.Add("MAC");
            if (!dato.HasServer) faltas.Add("Servidor");
            if (!dato.HasNode) faltas.Add("Nodo");
            if (!dato.HasQueue) faltas.Add("Queue");
            if (!dato.HasBinding) faltas.Add("IpBinding");

            if (faltas.Count > 0)
                incompletos[dato.ContractClientId] = string.Join(", ", faltas);
        }

        return incompletos;
    }

    //Los contratos que ya tienen nota viva del periodo, de una sola vez.
    //Sale directo del indice (corporacion, ano, mes): antes se navegaba por la venta y su nota
    //general, que es justo el tipo de consulta que el hosting cancela por costo.
    private async Task<HashSet<Guid>> BilledContractsForPeriodAsync(int corporationId, int yearNumber, MonthType monthType, List<Guid>? soloEstos = null)
    {
        var ids = await _context.CxCBills
            .AsNoTracking()
            .Where(x => x.CorporationId == corporationId &&
                        x.YearNumber == yearNumber &&
                        x.MonthType == monthType &&
                        !x.Cancelled &&
                        (soloEstos == null || soloEstos.Contains(x.ContractClientId)))
            .Select(x => x.ContractClientId)
            .Distinct()
            .ToListAsync();

        return ids.ToHashSet();
    }

    //Revision de la nota individual: que se le va a cobrar a ese cliente y que le falta al
    //contrato. Es un solo contrato, asi que se mira de frente por su id.
    public async Task<ActionResponse<BillingOneCheckDto>> CheckBillingNoteOneAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingOneCheckDto>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var note = await _context.BillingNoteOnes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BillingNoteOneId == id && x.CorporationId == corporationId);

            if (note == null)
                return Fail<BillingOneCheckDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //Todo lo del contrato en una sola consulta: el estado y lo que tiene configurado
            var datos = await _context.ContractClients
                .AsNoTracking()
                .Where(x => x.ContractClientId == note.ContractClientId && x.CorporationId == corporationId)
                .Select(x => new
                {
                    x.ControlContrato,
                    ClientFullName = x.Client!.FirstName + " " + x.Client.LastName,
                    ZoneName = x.Zone!.ZoneName,
                    IsActive = x.ContractState == ContractState.Active,
                    AppliesTax = x.EstratoSocial == null || x.EstratoSocial.ApplyTax,
                    HasPlan = x.ContractPlans!.Any(),
                    HasIp = x.ContractIps!.Any(),
                    HasMac = x.ContractMacs!.Any(),
                    HasServer = x.ContractServers!.Any(),
                    HasNode = x.ContractNodes!.Any(),
                    HasQueue = _context.ContractQues.Any(q => q.ContractClientId == x.ContractClientId),
                    HasBinding = _context.ContractBinds.Any(b => b.ContractClientId == x.ContractClientId)
                })
                .FirstOrDefaultAsync();

            if (datos == null)
                return Fail<BillingOneCheckDto>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var dto = new BillingOneCheckDto
            {
                ControlContrato = datos.ControlContrato,
                ClientFullName = datos.ClientFullName,
                ZoneName = datos.ZoneName,
                IsActive = datos.IsActive,
                HasPlan = datos.HasPlan,
                HasIp = datos.HasIp,
                HasMac = datos.HasMac,
                HasServer = datos.HasServer,
                HasNode = datos.HasNode,
                HasQueue = datos.HasQueue,
                HasBinding = datos.HasBinding,
                AlreadyBilled = await HasBillingForPeriodAsync(note.ContractClientId, corporationId, note.YearNumber, note.MonthType)
            };

            //El plan del mes, con su impuesto si el estrato lo paga
            var plan = await _context.ContractPlans
                .AsNoTracking()
                .Where(x => x.ContractClientId == note.ContractClientId)
                .Select(x => new { x.Plan!.PlanName, x.Plan.Price, TaxRate = x.Plan.Tax == null ? 0 : x.Plan.Tax.Rate })
                .FirstOrDefaultAsync();

            if (plan != null)
            {
                var taxRate = datos.AppliesTax ? plan.TaxRate : 0;
                dto.PlanName = plan.PlanName;
                dto.PlanPrice = plan.Price + CalculateTax(plan.Price, taxRate);
            }

            //El adelanto del mes: si reservo solicitudes, esas entran en esta nota
            var prePayment = await _context.PrePayments
                .AsNoTracking()
                .Include(x => x.PrePaymentDetails!)
                .FirstOrDefaultAsync(x => x.CorporationId == corporationId &&
                                          x.ContractClientId == note.ContractClientId &&
                                          x.YearNumber == note.YearNumber &&
                                          x.MonthType == note.MonthType &&
                                          !x.Billed);

            var reservedRequestIds = prePayment?.PrePaymentDetails?
                .Where(x => x.ServiceRequestId.HasValue)
                .Select(x => x.ServiceRequestId!.Value)
                .Distinct()
                .ToList() ?? new List<Guid>();

            //Las solicitudes de servicio que se le van a cobrar en esta nota
            var serviceRequests = await GetPendingServiceRequestsAsync(note.ContractClientId, corporationId,
                note.YearNumber, note.MonthType, reservedRequestIds);

            foreach (var request in serviceRequests)
            {
                foreach (var detail in request.ServiceRequestDetails ?? Enumerable.Empty<ServiceRequestDetail>())
                {
                    dto.Services.Add(new BillingOneLineDto
                    {
                        Concept = BuildServiceConcept(request, detail),
                        Price = datos.AppliesTax ? detail.Total : detail.Price
                    });
                }
            }

            var exonerated = await _context.ContractExonerateds
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CorporationId == corporationId &&
                                          x.ContractClientId == note.ContractClientId &&
                                          x.YearNumber == note.YearNumber &&
                                          x.MonthType == note.MonthType &&
                                          !x.Billed);

            dto.ServicesTotal = dto.Services.Sum(x => x.Price);
            dto.Total = dto.PlanPrice + dto.ServicesTotal;
            dto.PrePayment = prePayment?.PriceWithTax ?? 0;
            dto.Exonerated = exonerated?.PriceWithTax ?? 0;
            dto.Balance = dto.Total - dto.PrePayment - dto.Exonerated;
            dto.PrePaymentAndExonerated = prePayment != null && exonerated != null;

            return new ActionResponse<BillingOneCheckDto> { WasSuccess = true, Result = dto };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<BillingOneCheckDto>(ex);
        }
    }

    public async Task<ActionResponse<BillingNoteOne>> LaunchBillingNoteOneAsync(Guid id, string username)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingNoteOne>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var note = await _context.BillingNoteOnes
                .FirstOrDefaultAsync(x => x.BillingNoteOneId == id && x.CorporationId == corporationId);

            if (note == null)
                return Fail<BillingNoteOne>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (note.Created)
                return Fail<BillingNoteOne>(_localizer["BillingOne_AlreadyLaunched"]);

            var contract = await GetBillableContractsQuery(corporationId)
                .FirstOrDefaultAsync(x => x.ContractClientId == note.ContractClientId);

            if (contract == null)
                return Fail<BillingNoteOne>(_localizer["BillingOne_ContractNotActive"]);

            if (await HasBillingForPeriodAsync(contract.ContractClientId, corporationId, note.YearNumber, note.MonthType))
                return Fail<BillingNoteOne>(_localizer["BillingOne_AlreadyBilled", contract.ControlContrato]);

            //Un contrato activo debe estar completo para poder cobrarle
            var incompletos = await IncompleteContractsAsync(corporationId, new List<Guid> { contract.ContractClientId });
            if (incompletos.TryGetValue(contract.ContractClientId, out var falta))
                return Fail<BillingNoteOne>($"{_localizer["Billing_Incomplete"]}: {falta}");

            //Se reclama la nota: si otro usuario la lanzo primero, aqui salen cero filas
            var claimed = await _context.BillingNoteOnes
                .Where(x => x.BillingNoteOneId == note.BillingNoteOneId && !x.Created)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Created, true)
                    .SetProperty(x => x.DateCreated, DateTime.UtcNow.Date));

            if (claimed == 0)
            {
                await transaction.RollbackAsync();
                return Fail<BillingNoteOne>(_localizer["BillingOne_AlreadyLaunched"]);
            }

            var response = await CreateBillingForContractAsync(contract, note.YearNumber, note.MonthType, null, note.BillingNoteOneId, user.Id, $"{user.FirstName} {user.LastName}");
            if (!response.WasSuccess)
            {
                await transaction.RollbackAsync();
                return Fail<BillingNoteOne>(response.Message!);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            //Lo que se devuelve refleja lo que quedo en la base
            note.Created = true;
            note.DateCreated = DateTime.UtcNow.Date;

            return new ActionResponse<BillingNoteOne> { WasSuccess = true, Result = note };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return await _httpErrorHandler.HandleErrorAsync<BillingNoteOne>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<BillingContractDto>>> SearchContractsAsync(string filter, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<BillingContractDto>>();

            filter = filter?.Trim() ?? string.Empty;
            if (filter.Length < 2)
                return new ActionResponse<IEnumerable<BillingContractDto>> { WasSuccess = true, Result = Enumerable.Empty<BillingContractDto>() };

            var contracts = await _context.ContractClients
                .Include(x => x.Client)
                .Include(x => x.Zone!)
                    .ThenInclude(x => x.City)
                .Include(x => x.ContractPlans!)
                    .ThenInclude(x => x.Plan)
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.ContractState == ContractState.Active &&
                            (EF.Functions.Like(x.Client!.FirstName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client!.LastName, $"%{filter}%") ||
                             EF.Functions.Like(x.Client!.FirstName + " " + x.Client!.LastName, $"%{filter}%") ||
                             EF.Functions.Like(x.ControlContrato.ToString(), $"%{filter}%")))
                .OrderBy(x => x.Client!.FirstName)
                .ThenBy(x => x.Client!.LastName)
                .Take(20)
                .Select(x => new BillingContractDto
                {
                    ContractClientId = x.ContractClientId,
                    ClientId = x.ClientId,
                    ControlContrato = x.ControlContrato,
                    ClientFullName = $"{x.Client!.FirstName} {x.Client!.LastName}",
                    PhoneNumber = x.PhoneNumber,
                    Address = x.Address,
                    CityName = x.Zone!.City!.Name,
                    ZoneName = x.Zone!.ZoneName,
                    PlanName = x.ContractPlans!.Select(p => p.Plan!.PlanName).FirstOrDefault(),
                    PlanPrice = x.ContractPlans!.Select(p => (decimal?)p.Plan!.Price).FirstOrDefault()
                })
                .AsSplitQuery()
                .ToListAsync();

            return new ActionResponse<IEnumerable<BillingContractDto>> { WasSuccess = true, Result = contracts };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<BillingContractDto>>(ex);
        }
    }

    private IQueryable<ContractClient> GetBillableContractsQuery(int corporationId) =>
        _context.ContractClients
            .Include(x => x.Client!)
                .ThenInclude(x => x.DocumentType)
            .Include(x => x.Zone!)
                .ThenInclude(x => x.City)
            .Include(x => x.EstratoSocial)
            .Include(x => x.ContractPlans!)
                .ThenInclude(x => x.Plan!)
                    .ThenInclude(x => x.Tax)
            .Where(x => x.CorporationId == corporationId && x.ContractState == ContractState.Active);

    //Si un contrato ya tiene nota viva del periodo. Lo primero es el camino corto por indice;
    //las notas viejas (creadas antes de guardar el periodo en la propia nota) se buscan por la
    //nota general, para que el corte siga siendo correcto con lo ya facturado.
    private async Task<bool> HasBillingForPeriodAsync(
        Guid contractClientId,
        int corporationId,
        int yearNumber,
        MonthType monthType)
    {
        var porPeriodo = await _context.CxCBills.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.ContractClientId == contractClientId &&
            x.YearNumber == yearNumber &&
            x.MonthType == monthType &&
            !x.Cancelled);

        if (porPeriodo)
            return true;

        return await _context.CxCBills.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.ContractClientId == contractClientId &&
            x.YearNumber == 0 &&
            !x.Cancelled &&
            ((x.BillingNoteOne != null &&
              x.BillingNoteOne.YearNumber == yearNumber &&
              x.BillingNoteOne.MonthType == monthType) ||
             (x.Sell != null &&
              x.Sell.BillingNote != null &&
              x.Sell.BillingNote.YearNumber == yearNumber &&
              x.Sell.BillingNote.MonthType == monthType) ||
             (x.Sell != null &&
              x.Sell.BillingNoteOne != null &&
              x.Sell.BillingNoteOne.YearNumber == yearNumber &&
              x.Sell.BillingNoteOne.MonthType == monthType)));
    }

    private async Task<ActionResponse<bool>> CreateBillingForContractAsync(
        ContractClient contract,
        int yearNumber,
        MonthType monthType,
        Guid? billingNoteId,
        Guid? billingNoteOneId,
        string userId,
        string usuarioOwner)
    {
        var contractPlan = contract.ContractPlans?.FirstOrDefault(x => x.Plan != null);
        if (contractPlan?.Plan == null)
        {
            return Fail<bool>($"El contrato {contract.ControlContrato} no tiene plan configurado.");
        }

        var plan = contractPlan.Plan;
        bool appliesTax = contract.EstratoSocial?.ApplyTax ?? true;
        var planTaxRate = appliesTax ? plan.Tax?.Rate ?? 0 : 0;
        var planTaxAmount = CalculateTax(plan.Price, planTaxRate);
        var planPrice = plan.Price + planTaxAmount;
        var corporationId = contract.CorporationId;
        var utcNow = DateTime.UtcNow;
        //Los consecutivos los entrega la base: dos usuarios nunca se llevan el mismo
        var invoiceNumber = $"FA-{await NumberSequence.NextAsync(_context, corporationId, NumberKind.Invoice):0000000}";
        var collectionNote = $"NC-{await NumberSequence.NextAsync(_context, corporationId, NumberKind.CollectionNote):0000000}";

        var sell = new Sell
        {
            SellId = Guid.NewGuid(),
            DateSell = utcNow.Date,
            InvoiceNumber = invoiceNumber,
            ContractClientId = contract.ContractClientId,
            ControlContrato = contract.ControlContrato,
            ClientId = contract.ClientId,
            ClientFullName = $"{contract.Client!.FirstName} {contract.Client.LastName}",
            DocumentTypeId = contract.Client.DocumentTypeId,
            DocumentTypeName = contract.Client.DocumentType?.DocumentName,
            Identification = contract.Client.Document,
            PhoneNumber = contract.PhoneNumber,
            Address = contract.Address,
            ZoneName = contract.Zone?.ZoneName,
            BillingNoteId = billingNoteId,
            BillingNoteOneId = billingNoteOneId,
            CorporationId = corporationId,
            UsuarioOwner = usuarioOwner,
            UserId = Guid.Parse(userId),
            SellDetails = new List<SellDetail>()
        };

        sell.SellDetails.Add(new SellDetail
        {
            SellDetailId = Guid.NewGuid(),
            SellId = sell.SellId,
            Code = plan.PlanId.ToString(),
            Origin = "Plan",
            Concept = $"Plan {plan.PlanName}",
            Quantity = 1,
            TaxId = plan.TaxId,
            TaxRate = planTaxRate,
            UnitPrice = plan.Price,
            TaxAmount = planTaxAmount,
            Price = planPrice,
            CorporationId = corporationId
        });

        //El pago adelantado del mes se busca antes: si reservo solicitudes, esas entran en ESTA nota
        //aunque su fecha quede fuera del corte. El cliente ya las pago.
        var prePayment = await _context.PrePayments
            .Include(x => x.PrePaymentDetails!)
            .FirstOrDefaultAsync(x =>
                x.CorporationId == corporationId &&
                x.ContractClientId == contract.ContractClientId &&
                x.YearNumber == yearNumber &&
                x.MonthType == monthType &&
                !x.Billed);

        var reservedRequestIds = prePayment?.PrePaymentDetails?
            .Where(x => x.ServiceRequestId.HasValue)
            .Select(x => x.ServiceRequestId!.Value)
            .Distinct()
            .ToList() ?? new List<Guid>();

        var serviceRequests = await GetPendingServiceRequestsAsync(contract.ContractClientId, corporationId, yearNumber, monthType, reservedRequestIds);
        foreach (var request in serviceRequests)
        {
            foreach (var detail in request.ServiceRequestDetails ?? Enumerable.Empty<ServiceRequestDetail>())
            {
                var sellDetail = new SellDetail
                {
                    SellDetailId = Guid.NewGuid(),
                    SellId = sell.SellId,
                    Code = detail.ServiceClientId.ToString(),
                    Origin = "SolicitudServicio",
                    Concept = BuildServiceConcept(request, detail),
                    Quantity = 1,
                    TaxId = detail.TaxId,
                    TaxRate = appliesTax ? detail.TaxRate : 0,
                    UnitPrice = detail.Price,
                    TaxAmount = appliesTax ? detail.TaxAmount : 0,
                    Price = appliesTax ? detail.Total : detail.Price,
                    ServiceRequestId = request.ServiceRequestId,
                    CorporationId = corporationId
                };

                sell.SellDetails.Add(sellDetail);
                detail.SellDetailId = sellDetail.SellDetailId;
            }

            if (request.ServiceRequestDetails?.Any() == true)
            {
                request.Billed = true;
                request.SellId = sell.SellId;
            }
        }

        var total = sell.SellDetails.Sum(x => x.TotalPrice);
        var preExonerated = await _context.ContractExonerateds.FirstOrDefaultAsync(x =>
            x.CorporationId == corporationId &&
            x.ContractClientId == contract.ContractClientId &&
            x.YearNumber == yearNumber &&
            x.MonthType == monthType &&
            !x.Billed);

        if (preExonerated != null && prePayment != null)
            return Fail<bool>($"El contrato {contract.ControlContrato} tiene exoneracion y pago adelantado para el mismo mes.");

        var discount = preExonerated?.PriceWithTax ?? 0;
        var payment = prePayment?.PriceWithTax ?? 0;
        var balance = total - discount - payment;
        var paid = balance <= 0;

        sell.Paid = paid;
        sell.DatePaid = paid ? utcNow.Date : null;

        var cxCBill = new CxCBill
        {
            CxCBillId = Guid.NewGuid(),
            DateNote = utcNow.Date,
            YearNumber = yearNumber,
            MonthType = monthType,
            CollectionNote = collectionNote,
            ClientId = contract.ClientId,
            ContractClientId = contract.ContractClientId,
            Description = $"Nota de cobro {collectionNote} - Contrato {contract.ControlContrato}",
            Total = total,
            Balance = balance,
            Paid = paid,
            DatePaid = paid ? utcNow.Date : null,
            SellId = sell.SellId,
            BillingNoteOneId = billingNoteOneId,
            CorporationId = corporationId,
            UsuarioOwner = usuarioOwner,
            UserId = Guid.Parse(userId),
            CxCBillDetails = new List<CxCBillDetail>()
        };

        var cxCBillDetail = new CxCBillDetail
        {
            CxCBillDetailId = Guid.NewGuid(),
            CxCBillId = cxCBill.CxCBillId,
            DatePayment = utcNow.Date,
            PaymentMode = prePayment == null ? null : "PrePayment",
            DiscountRate = preExonerated == null ? null : "ContractExonerated",
            Detail = BuildBillDetailText(prePayment, preExonerated, yearNumber, monthType),
            Debt = total,
            Payment = payment,
            Discount = discount,
            Balance = balance,
            CorporationId = corporationId,
            UsuarioOwner = usuarioOwner,
            UserId = Guid.Parse(userId)
        };

        cxCBill.CxCBillDetails.Add(cxCBillDetail);
        await _contractorPaymentService.CreateAccountPayableAsync(cxCBill, cxCBillDetail);

        if (preExonerated != null)
        {
            preExonerated.Billed = true;
            preExonerated.DateBilled = utcNow.Date;
            preExonerated.CxCBillId = cxCBill.CxCBillId;
        }

        if (prePayment != null)
        {
            prePayment.Billed = true;
            prePayment.DateBilled = utcNow.Date;
            prePayment.CxCBillId = cxCBill.CxCBillId;
        }

        _context.Sells.Add(sell);
        _context.CxCBills.Add(cxCBill);

        //Bitacora del dinero: la nota que nace, y el adelanto o la exoneracion que se cruzan
        PaymentAuditLog.Add(_context, corporationId, PaymentEventType.BillCreated,
            contract.ContractClientId, contract.ClientId, cxCBill.CxCBillId, nameof(CxCBill),
            total - (sell.SellDetails.Sum(x => x.TaxAmount)), sell.SellDetails.Sum(x => x.TaxAmount), total,
            $"Nota {collectionNote} - {monthType} {yearNumber} - saldo {balance:N2}",
            usuarioOwner, Guid.Parse(userId));

        if (prePayment != null)
        {
            PaymentAuditLog.Add(_context, corporationId, PaymentEventType.PrePaymentApplied,
                contract.ContractClientId, contract.ClientId, prePayment.PrePaymentId, nameof(PrePayment),
                prePayment.UnitPrice, prePayment.PriceWithTax - prePayment.UnitPrice, prePayment.PriceWithTax,
                $"Aplicado en la nota {collectionNote} - {monthType} {yearNumber}",
                usuarioOwner, Guid.Parse(userId));
        }

        if (preExonerated != null)
        {
            PaymentAuditLog.Add(_context, corporationId, PaymentEventType.ExoneratedApplied,
                contract.ContractClientId, contract.ClientId, preExonerated.ContractExoneratedId, nameof(ContractExonerated),
                preExonerated.UnitPrice, preExonerated.PriceWithTax - preExonerated.UnitPrice, preExonerated.PriceWithTax,
                $"Aplicada en la nota {collectionNote} - {monthType} {yearNumber}",
                usuarioOwner, Guid.Parse(userId));
        }

        return new ActionResponse<bool> { WasSuccess = true, Result = true };
    }

    //Las solicitudes que entran en la nota: las completadas sin facturar hasta el corte del mes,
    //mas las que un pago adelantado ya reservo (el cliente las pago, se cierran en esta nota).
    private async Task<List<ServiceRequest>> GetPendingServiceRequestsAsync(Guid contractClientId, int corporationId, int yearNumber, MonthType monthType, List<Guid> reservedRequestIds)
    {
        var lastDate = new DateTime(yearNumber, (int)monthType, 1).AddMonths(1).AddTicks(-1);
        return await _context.ServiceRequests
            .Include(x => x.ServiceRequestDetails!)
                .ThenInclude(x => x.ServiceClient)
            .Where(x => x.CorporationId == corporationId &&
                        x.ContractClientId == contractClientId &&
                        x.ScheduleStatus == ScheduleStatus.Completed &&
                        !x.Billed &&
                        ((x.CompletedAtUtc != null && x.CompletedAtUtc <= lastDate) ||
                         reservedRequestIds.Contains(x.ServiceRequestId)))
            .AsSplitQuery()
            .ToListAsync();
    }

    private static decimal CalculateTax(decimal unitPrice, decimal taxRate) =>
        Math.Round((unitPrice * taxRate) / 100, 2);

    private static string BuildServiceConcept(ServiceRequest request, ServiceRequestDetail detail)
    {
        var serviceName = detail.ServiceClient?.ServiceName ?? "Servicio";
        var executedDate = request.CompletedAtUtc?.ToString("dd/MM/yyyy") ?? string.Empty;
        var comment = string.IsNullOrWhiteSpace(request.TechnicianComment) ? request.ClientReason : request.TechnicianComment;
        return $"Solicitud #{request.RequestNumber} - {serviceName} - {executedDate} - {comment}";
    }

    private static string BuildBillDetailText(PrePayment? prePayment, ContractExonerated? preExonerated, int yearNumber, MonthType monthType)
    {
        if (preExonerated != null)
            return $"Deuda generada con exoneracion aplicada para {monthType} {yearNumber}.";

        if (prePayment != null)
            return $"Deuda generada con pago adelantado aplicado para {monthType} {yearNumber}.";

        return $"Deuda generada para {monthType} {yearNumber}.";
    }

    //El tablero de facturas: solo el mes en curso, para no recorrer el historico
    public async Task<ActionResponse<SellSummaryDto>> GetSellSummaryAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<SellSummaryDto>();

            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var nextMonth = monthStart.AddMonths(1);

            //Los tres conteos en una sola pasada
            var summary = await _context.Sells
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.DateSell >= monthStart &&
                            x.DateSell < nextMonth)
                .GroupBy(x => 1)
                .Select(g => new SellSummaryDto
                {
                    MonthCount = g.Count(x => !x.Cancelled),
                    MonthPaid = g.Count(x => x.Paid && !x.Cancelled),
                    MonthCancelled = g.Count(x => x.Cancelled)
                })
                .FirstOrDefaultAsync() ?? new SellSummaryDto();

            //El monto vive en los renglones: en la factura es una propiedad calculada.
            //Se arranca desde la factura, que es la que tiene el indice por fecha.
            summary.MonthTotal = await _context.Sells
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId &&
                            !x.Cancelled &&
                            x.DateSell >= monthStart &&
                            x.DateSell < nextMonth)
                .SelectMany(x => x.SellDetails!)
                .SumAsync(x => (decimal?)(x.Price * x.Quantity)) ?? 0;

            return new ActionResponse<SellSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<SellSummaryDto>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<Sell>>> GetSellsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<Sell>>();

            var queryable = _context.Sells.AsNoTracking()
                .Include(x => x.SellDetails)
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.InvoiceNumber!, $"%{filter}%") ||
                    EF.Functions.Like(x.ClientFullName, $"%{filter}%") ||
                    EF.Functions.Like(x.ControlContrato.ToString(), $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderByDescending(x => x.DateSell)
                .Paginate(pagination)
                .AsSplitQuery()
                .ToListAsync();

            return new ActionResponse<IEnumerable<Sell>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Sell>>(ex);
        }
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
