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

    public async Task<ActionResponse<BillingNote>> LaunchBillingNoteAsync(Guid id, string username)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<BillingNote>();

            var corporationId = Convert.ToInt32(user.CorporationId);
            var note = await _context.BillingNotes
                .FirstOrDefaultAsync(x => x.BillingNoteId == id && x.CorporationId == corporationId);

            if (note == null)
                return Fail<BillingNote>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (note.Created)
                return Fail<BillingNote>("La nota general ya fue lanzada.");

            var contracts = await GetBillableContractsQuery(corporationId)
                .OrderBy(x => x.ControlContrato)
                .ToListAsync();

            if (contracts.Count == 0)
                return Fail<BillingNote>("No hay contratos activos para lanzar notas.");

            var register = await GetOrCreateRegisterAsync(corporationId);
            foreach (var contract in contracts)
            {
                var response = await CreateBillingForContractAsync(contract, note.YearNumber, note.MonthType, note.BillingNoteId, null, register, user.Id, $"{user.FirstName} {user.LastName}");
                if (!response.WasSuccess)
                {
                    await transaction.RollbackAsync();
                    return Fail<BillingNote>(response.Message!);
                }
            }

            note.Created = true;
            note.DateCreated = DateTime.UtcNow.Date;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ActionResponse<BillingNote> { WasSuccess = true, Result = note };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return await _httpErrorHandler.HandleErrorAsync<BillingNote>(ex);
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
                return Fail<BillingNoteOne>("La nota individual ya fue lanzada.");

            var contract = await GetBillableContractsQuery(corporationId)
                .FirstOrDefaultAsync(x => x.ContractClientId == note.ContractClientId);

            if (contract == null)
                return Fail<BillingNoteOne>("Debe seleccionar un contrato activo.");

            var register = await GetOrCreateRegisterAsync(corporationId);
            var response = await CreateBillingForContractAsync(contract, note.YearNumber, note.MonthType, null, note.BillingNoteOneId, register, user.Id, $"{user.FirstName} {user.LastName}");
            if (!response.WasSuccess)
            {
                await transaction.RollbackAsync();
                return Fail<BillingNoteOne>(response.Message!);
            }

            note.Created = true;
            note.DateCreated = DateTime.UtcNow.Date;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

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
            .Include(x => x.ContractPlans!)
                .ThenInclude(x => x.Plan!)
                    .ThenInclude(x => x.Tax)
            .Where(x => x.CorporationId == corporationId && x.ContractState == ContractState.Active);

    private async Task<ActionResponse<bool>> CreateBillingForContractAsync(
        ContractClient contract,
        int yearNumber,
        MonthType monthType,
        Guid? billingNoteId,
        Guid? billingNoteOneId,
        Register register,
        string userId,
        string usuarioOwner)
    {
        var contractPlan = contract.ContractPlans?.FirstOrDefault(x => x.Plan != null);
        if (contractPlan?.Plan == null)
        {
            return Fail<bool>($"El contrato {contract.ControlContrato} no tiene plan configurado.");
        }

        var plan = contractPlan.Plan;
        var planTaxRate = plan.Tax?.Rate ?? 0;
        var planTaxAmount = CalculateTax(plan.Price, planTaxRate);
        var planPrice = plan.Price + planTaxAmount;
        var corporationId = contract.CorporationId;
        var utcNow = DateTime.UtcNow;
        var invoiceNumber = NextFormattedNumber(register, NumberKind.Invoice);
        var collectionNote = NextFormattedNumber(register, NumberKind.CollectionNote);

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

        var serviceRequests = await GetPendingServiceRequestsAsync(contract.ContractClientId, corporationId, yearNumber, monthType);
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
                    TaxRate = detail.TaxRate,
                    UnitPrice = detail.Price,
                    TaxAmount = detail.TaxAmount,
                    Price = detail.Total,
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
        var preExonerated = await _context.PreExonerateds.FirstOrDefaultAsync(x =>
            x.CorporationId == corporationId &&
            x.ContractClientId == contract.ContractClientId &&
            x.YearNumber == yearNumber &&
            x.MonthType == monthType &&
            !x.Billed);

        var prePayment = await _context.PrePayments.FirstOrDefaultAsync(x =>
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

        var cxCBill = new CxCBill
        {
            CxCBillId = Guid.NewGuid(),
            DateNote = utcNow.Date,
            CollectionNote = collectionNote,
            ClientId = contract.ClientId,
            ContractClientId = contract.ContractClientId,
            Description = $"Nota de cobro {collectionNote} - Contrato {contract.ControlContrato}",
            Total = total,
            Balance = balance,
            SellId = sell.SellId,
            BillingNoteOneId = billingNoteOneId,
            CorporationId = corporationId,
            UsuarioOwner = usuarioOwner,
            UserId = Guid.Parse(userId),
            CxCBillDetails = new List<CxCBillDetail>()
        };

        cxCBill.CxCBillDetails.Add(new CxCBillDetail
        {
            CxCBillDetailId = Guid.NewGuid(),
            CxCBillId = cxCBill.CxCBillId,
            DatePayment = utcNow.Date,
            PaymentMode = prePayment == null ? null : "PrePayment",
            DiscountRate = preExonerated == null ? null : "PreExonerated",
            Detail = BuildBillDetailText(prePayment, preExonerated, yearNumber, monthType),
            Debt = total,
            Payment = payment,
            Discount = discount,
            Balance = balance,
            CorporationId = corporationId,
            UsuarioOwner = usuarioOwner,
            UserId = Guid.Parse(userId)
        });

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

        return new ActionResponse<bool> { WasSuccess = true, Result = true };
    }

    private async Task<List<ServiceRequest>> GetPendingServiceRequestsAsync(Guid contractClientId, int corporationId, int yearNumber, MonthType monthType)
    {
        var lastDate = new DateTime(yearNumber, (int)monthType, 1).AddMonths(1).AddTicks(-1);
        return await _context.ServiceRequests
            .Include(x => x.ServiceRequestDetails!)
                .ThenInclude(x => x.ServiceClient)
            .Where(x => x.CorporationId == corporationId &&
                        x.ContractClientId == contractClientId &&
                        x.ScheduleStatus == ScheduleStatus.Completed &&
                        !x.Billed &&
                        x.CompletedAtUtc != null &&
                        x.CompletedAtUtc <= lastDate)
            .ToListAsync();
    }

    private async Task<Register> GetOrCreateRegisterAsync(int corporationId)
    {
        var register = await _context.Registers.FirstOrDefaultAsync(x => x.CorporationId == corporationId);
        if (register != null)
            return register;

        register = new Register { RegisterId = Guid.NewGuid(), CorporationId = corporationId };
        _context.Registers.Add(register);
        return register;
    }

    private static string NextFormattedNumber(Register register, NumberKind kind)
    {
        return kind switch
        {
            NumberKind.Invoice => $"FA-{++register.Factura:0000000}",
            NumberKind.CollectionNote => $"NC-{++register.NotaCobro:0000000}",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
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

    private static string BuildBillDetailText(PrePayment? prePayment, PreExonerated? preExonerated, int yearNumber, MonthType monthType)
    {
        if (preExonerated != null)
            return $"Deuda generada con exoneracion aplicada para {monthType} {yearNumber}.";

        if (prePayment != null)
            return $"Deuda generada con pago adelantado aplicado para {monthType} {yearNumber}.";

        return $"Deuda generada para {monthType} {yearNumber}.";
    }

    private enum NumberKind
    {
        Invoice,
        CollectionNote
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
