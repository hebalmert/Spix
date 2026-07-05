using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesBilling;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesContratos;
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

    public BillingService(DataContext context, IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer,
        IEnumMultilLanguageService enumMultilLanguageService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _enumMultilLanguageService = enumMultilLanguageService;
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

    public async Task<ActionResponse<IEnumerable<BillingNoteOne>>> GetBillingNoteOnesAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<BillingNoteOne>>();

            var queryable = _context.BillingNoteOnes
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
                return Fail<BillingNoteOne>("Debe seleccionar un contrato activo.");

            var contract = await _context.ContractClients
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ContractClientId == model.ContractClientId &&
                                          x.CorporationId == user.CorporationId &&
                                          x.ContractState == ContractState.Active);

            if (contract == null)
                return Fail<BillingNoteOne>("Debe seleccionar un contrato activo.");

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

            var exists = await _context.BillingNoteOnes.AnyAsync(x =>
                x.CorporationId == model.CorporationId &&
                x.ContractClientId == model.ContractClientId &&
                x.YearNumber == model.YearNumber &&
                x.MonthType == model.MonthType);

            if (exists)
                return Fail<BillingNoteOne>("Ya existe una nota individual para ese contrato, mes y año.");

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
                return Fail<BillingNoteOne>("La nota individual ya fue lanzada y no puede modificarse.");

            if (model.ContractClientId == Guid.Empty)
                return Fail<BillingNoteOne>("Debe seleccionar un contrato activo.");

            var contract = await _context.ContractClients
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ContractClientId == model.ContractClientId &&
                                          x.CorporationId == user.CorporationId &&
                                          x.ContractState == ContractState.Active);

            if (contract == null)
                return Fail<BillingNoteOne>("Debe seleccionar un contrato activo.");

            if (model.DateBill == default)
                return Fail<BillingNoteOne>("Debe seleccionar la fecha.");

            if (model.YearNumber <= 0)
                model.YearNumber = model.DateBill.Year;

            if (!Enum.IsDefined(model.MonthType))
                model.MonthType = (MonthType)model.DateBill.Month;

            var exists = await _context.BillingNoteOnes.AnyAsync(x =>
                x.BillingNoteOneId != current.BillingNoteOneId &&
                x.CorporationId == current.CorporationId &&
                x.ContractClientId == model.ContractClientId &&
                x.YearNumber == model.YearNumber &&
                x.MonthType == model.MonthType);

            if (exists)
                return Fail<BillingNoteOne>("Ya existe una nota individual para ese contrato, mes y año.");

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
                .ToListAsync();

            return new ActionResponse<IEnumerable<BillingContractDto>> { WasSuccess = true, Result = contracts };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<BillingContractDto>>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<Sell>>> GetSellsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<Sell>>();

            var queryable = _context.Sells
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
