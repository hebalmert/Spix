using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesPayment;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementPayment;

public class PaymentService : IPaymentService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly IEnumMultilLanguageService _enumMultilLanguageService;

    public PaymentService(DataContext context, IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper, ITransactionManager transactionManager, HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer, IEnumMultilLanguageService enumMultilLanguageService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _transactionManager = transactionManager;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _enumMultilLanguageService = enumMultilLanguageService;
    }

    public async Task<ActionResponse<IEnumerable<CxCBill>>> GetCxCBillsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<CxCBill>>();

            var queryable = _context.CxCBills
                .Include(x => x.Client)
                .Include(x => x.ContractClient)
                .Include(x => x.CxCBillDetails)
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();

            if (pagination.GuidId.HasValue && pagination.GuidId.Value != Guid.Empty)
                queryable = queryable.Where(x => x.ContractClientId == pagination.GuidId.Value);

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.CollectionNote!, $"%{filter}%") ||
                    EF.Functions.Like(x.Description, $"%{filter}%") ||
                    EF.Functions.Like(x.Client!.FirstName, $"%{filter}%") ||
                    EF.Functions.Like(x.Client!.LastName, $"%{filter}%") ||
                    EF.Functions.Like(x.ContractClient!.ControlContrato.ToString(), $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderByDescending(x => x.DateNote)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<CxCBill>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<CxCBill>>(ex);
        }
    }

    public async Task<ActionResponse<CxCBill>> GetCxCBillAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<CxCBill>();

            var model = await _context.CxCBills
                .Include(x => x.Client)
                .Include(x => x.ContractClient)
                .Include(x => x.CxCBillDetails)
                .FirstOrDefaultAsync(x => x.CxCBillId == id && x.CorporationId == user.CorporationId);

            if (model == null)
            {
                return new ActionResponse<CxCBill>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<CxCBill> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<CxCBill>(ex);
        }
    }

    public async Task<ActionResponse<CxCBill>> PayCxCBillAsync(CxCBillPaymentDto model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<CxCBill>();
            }

            var bill = await _context.CxCBills
                .Include(x => x.CxCBillDetails)
                .FirstOrDefaultAsync(x => x.CxCBillId == model.CxCBillId && x.CorporationId == user.CorporationId);

            if (bill == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<CxCBill>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            if (bill.Cancelled)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<CxCBill> { WasSuccess = false, Message = "La cuenta por cobrar esta anulada." };
            }

            if (bill.Paid || bill.Balance <= 0)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<CxCBill> { WasSuccess = false, Message = "La cuenta por cobrar ya esta pagada." };
            }

            if (!IsValidDiscount(model.DiscountPercent))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<CxCBill> { WasSuccess = false, Message = "El descuento seleccionado no es valido." };
            }

            if (model.DiscountPercent > 0 && string.IsNullOrWhiteSpace(model.Detail))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<CxCBill> { WasSuccess = false, Message = "Debe especificar la razon del descuento." };
            }

            var debt = bill.Balance;
            var discount = Math.Round((debt * model.DiscountPercent) / 100, 2);
            var payment = debt - discount;
            var balance = debt - discount - payment;

            var detail = new CxCBillDetail
            {
                CxCBillId = bill.CxCBillId,
                DatePayment = DateTime.UtcNow.Date,
                PaymentMode = model.PaymentMode,
                DiscountRate = model.DiscountPercent == 0 ? null : $"{model.DiscountPercent}%",
                Detail = model.Detail,
                Debt = debt,
                Payment = payment,
                Discount = discount,
                Balance = balance,
                CorporationId = bill.CorporationId,
                UsuarioOwner = $"{user.FirstName} {user.LastName}",
                UserId = Guid.Parse(user.Id)
            };

            _context.CxCBillDetails.Add(detail);

            bill.Balance = balance;
            bill.Paid = balance == 0;
            bill.DatePaid = bill.Paid ? DateTime.UtcNow.Date : null;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<CxCBill> { WasSuccess = true, Result = bill };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<CxCBill>(ex);
        }
    }

    public async Task<ActionResponse<CxCBill>> CancelCxCBillAsync(CxCBillCancelDto model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<CxCBill>();
            }

            if (string.IsNullOrWhiteSpace(model.DescriptionCancelled))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<CxCBill> { WasSuccess = false, Message = "Debe especificar el motivo de anulacion." };
            }

            var bill = await _context.CxCBills
                .FirstOrDefaultAsync(x => x.CxCBillId == model.CxCBillId && x.CorporationId == user.CorporationId);

            if (bill == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<CxCBill>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            if (bill.Paid)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<CxCBill> { WasSuccess = false, Message = "No se puede anular una cuenta por cobrar pagada." };
            }

            if (bill.Cancelled)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<CxCBill> { WasSuccess = false, Message = "La cuenta por cobrar ya esta anulada." };
            }

            bill.Cancelled = true;
            bill.DateCancelled = DateTime.UtcNow.Date;
            bill.DescriptionCancelled = model.DescriptionCancelled.Trim();
            bill.UsuarioOwnerCancelled = $"{user.FirstName} {user.LastName}";
            bill.UserIdCancelled = Guid.Parse(user.Id);
            bill.Balance = 0;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<CxCBill> { WasSuccess = true, Result = bill };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<CxCBill>(ex);
        }
    }

    private static bool IsValidDiscount(int discount) =>
        discount is 0 or 25 or 50 or 75 or 100;

    public async Task<ActionResponse<IEnumerable<PrePayment>>> GetPrePaymentsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<PrePayment>>();

            var queryable = _context.PrePayments
                .Include(x => x.Client)
                .Include(x => x.ContractClient)
                .Include(x => x.Plan)
                .Where(x => x.CorporationId == user.CorporationId && !x.Billed)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.Client!.FirstName, $"%{filter}%") ||
                    EF.Functions.Like(x.Client!.LastName, $"%{filter}%") ||
                    EF.Functions.Like(x.Client!.FirstName + " " + x.Client!.LastName, $"%{filter}%") ||
                    EF.Functions.Like(x.ContractClient!.ControlContrato.ToString(), $"%{filter}%") ||
                    EF.Functions.Like(x.Plan!.PlanName, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderBy(x => x.Client!.FirstName)
                .ThenBy(x => x.Client!.LastName)
                .ThenBy(x => x.ContractClient!.ControlContrato)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<PrePayment>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<PrePayment>>(ex);
        }
    }

    public async Task<ActionResponse<PrePayment>> GetPrePaymentAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<PrePayment>();

            var model = await _context.PrePayments
                .Include(x => x.Client)
                .Include(x => x.ContractClient!)
                    .ThenInclude(x => x.Zone!)
                        .ThenInclude(x => x.City)
                .Include(x => x.ContractClient!)
                    .ThenInclude(x => x.ContractPlans!)
                        .ThenInclude(x => x.Plan!)
                            .ThenInclude(x => x.Tax)
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x => x.PrePaymentId == id && x.CorporationId == user.CorporationId);

            if (model == null)
            {
                return new ActionResponse<PrePayment>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<PrePayment> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<PrePayment>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboMonthsAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<IntItemModel>>();

            var list = _enumMultilLanguageService.GetEnumSelectList<MonthType>("Select_Month");
            return new ActionResponse<IEnumerable<IntItemModel>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
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
                    .ThenInclude(x => x.Plan!)
                        .ThenInclude(x => x.Tax)
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
                    PlanId = x.ContractPlans!.Select(p => p.PlanId).FirstOrDefault(),
                    PlanName = x.ContractPlans!.Select(p => p.Plan!.PlanName).FirstOrDefault(),
                    PlanPrice = x.ContractPlans!.Select(p => (decimal?)p.Plan!.Price).FirstOrDefault(),
                    TaxRate = x.ContractPlans!.Select(p => (decimal?)p.Plan!.Tax!.Rate).FirstOrDefault(),
                    PlanPriceWithTax = x.ContractPlans!.Select(p => (decimal?)Math.Round(p.Plan!.Price + ((p.Plan!.Price * p.Plan!.Tax!.Rate) / 100), 2)).FirstOrDefault()
                })
                .ToListAsync();

            return new ActionResponse<IEnumerable<BillingContractDto>> { WasSuccess = true, Result = contracts };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<BillingContractDto>>(ex);
        }
    }

    public async Task<ActionResponse<PrePayment>> AddPrePaymentAsync(PrePayment model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<PrePayment>();
            }

            var response = await PreparePrePaymentAsync(model, user.CorporationId!.Value);
            if (!response.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return response;
            }

            model.DatePayment = NormalizeDate(model.DatePayment == default ? DateTime.UtcNow : model.DatePayment);
            ApplyDefaultMonth(model);
            model.CorporationId = user.CorporationId.Value;
            model.UsuarioOwner = $"{user.FirstName} {user.LastName}";
            model.UserId = Guid.Parse(user.Id);

            var exists = await _context.PrePayments.AnyAsync(x =>
                x.CorporationId == model.CorporationId &&
                x.ContractClientId == model.ContractClientId &&
                x.YearNumber == model.YearNumber &&
                x.MonthType == model.MonthType);

            if (exists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<PrePayment>
                {
                    WasSuccess = false,
                    Message = "Ya existe un pago adelantado para este contrato, ano y mes."
                };
            }

            _context.PrePayments.Add(model);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<PrePayment> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<PrePayment>(ex);
        }
    }

    public async Task<ActionResponse<PrePayment>> UpdatePrePaymentAsync(PrePayment model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<PrePayment>();
            }

            var current = await _context.PrePayments
                .FirstOrDefaultAsync(x => x.PrePaymentId == model.PrePaymentId && x.CorporationId == user.CorporationId);

            if (current == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<PrePayment>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            if (current.Billed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<PrePayment>
                {
                    WasSuccess = false,
                    Message = "No se puede editar un pago adelantado facturado."
                };
            }

            current.DatePayment = NormalizeDate(model.DatePayment);
            current.PaymentControl = model.PaymentControl;
            current.ContractClientId = model.ContractClientId;
            current.YearNumber = model.YearNumber;
            current.MonthType = model.MonthType;

            var response = await PreparePrePaymentAsync(current, user.CorporationId!.Value);
            if (!response.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return response;
            }

            var exists = await _context.PrePayments.AnyAsync(x =>
                x.PrePaymentId != current.PrePaymentId &&
                x.CorporationId == current.CorporationId &&
                x.ContractClientId == current.ContractClientId &&
                x.YearNumber == current.YearNumber &&
                x.MonthType == current.MonthType);

            if (exists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<PrePayment>
                {
                    WasSuccess = false,
                    Message = "Ya existe un pago adelantado para este contrato, ano y mes."
                };
            }

            _context.PrePayments.Update(current);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<PrePayment> { WasSuccess = true, Result = current };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<PrePayment>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeletePrePaymentAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<bool>();
            }

            var current = await _context.PrePayments
                .FirstOrDefaultAsync(x => x.PrePaymentId == id && x.CorporationId == user.CorporationId);

            if (current == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            if (current.Billed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar un pago adelantado facturado."
                };
            }

            _context.PrePayments.Remove(current);
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

    public async Task<ActionResponse<IEnumerable<PreExonerated>>> GetPreExoneratedsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<PreExonerated>>();

            var queryable = _context.PreExonerateds
                .Include(x => x.Client)
                .Include(x => x.ContractClient)
                .Include(x => x.Plan)
                .Where(x => x.CorporationId == user.CorporationId && !x.Billed)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.Client!.FirstName, $"%{filter}%") ||
                    EF.Functions.Like(x.Client!.LastName, $"%{filter}%") ||
                    EF.Functions.Like(x.Client!.FirstName + " " + x.Client!.LastName, $"%{filter}%") ||
                    EF.Functions.Like(x.ContractClient!.ControlContrato.ToString(), $"%{filter}%") ||
                    EF.Functions.Like(x.Plan!.PlanName, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderBy(x => x.Client!.FirstName)
                .ThenBy(x => x.Client!.LastName)
                .ThenBy(x => x.ContractClient!.ControlContrato)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<PreExonerated>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<PreExonerated>>(ex);
        }
    }

    public async Task<ActionResponse<PreExonerated>> GetPreExoneratedAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<PreExonerated>();

            var model = await _context.PreExonerateds
                .Include(x => x.Client)
                .Include(x => x.ContractClient!)
                    .ThenInclude(x => x.Zone!)
                        .ThenInclude(x => x.City)
                .Include(x => x.ContractClient!)
                    .ThenInclude(x => x.ContractPlans!)
                        .ThenInclude(x => x.Plan!)
                            .ThenInclude(x => x.Tax)
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x => x.PreExoneratedId == id && x.CorporationId == user.CorporationId);

            if (model == null)
            {
                return new ActionResponse<PreExonerated>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<PreExonerated> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<PreExonerated>(ex);
        }
    }

    public async Task<ActionResponse<PreExonerated>> AddPreExoneratedAsync(PreExonerated model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<PreExonerated>();
            }

            var response = await PreparePreExoneratedAsync(model, user.CorporationId!.Value);
            if (!response.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return response;
            }

            model.DateExonerated = NormalizeDate(model.DateExonerated == default ? DateTime.UtcNow : model.DateExonerated);
            ApplyDefaultMonth(model);
            model.CorporationId = user.CorporationId.Value;
            model.UsuarioOwner = $"{user.FirstName} {user.LastName}";
            model.UserId = Guid.Parse(user.Id);

            var exists = await _context.PreExonerateds.AnyAsync(x =>
                x.CorporationId == model.CorporationId &&
                x.ContractClientId == model.ContractClientId &&
                x.YearNumber == model.YearNumber &&
                x.MonthType == model.MonthType);

            if (exists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<PreExonerated>
                {
                    WasSuccess = false,
                    Message = "Ya existe una exoneracion para este contrato, ano y mes."
                };
            }

            _context.PreExonerateds.Add(model);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<PreExonerated> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<PreExonerated>(ex);
        }
    }

    public async Task<ActionResponse<PreExonerated>> UpdatePreExoneratedAsync(PreExonerated model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<PreExonerated>();
            }

            var current = await _context.PreExonerateds
                .FirstOrDefaultAsync(x => x.PreExoneratedId == model.PreExoneratedId && x.CorporationId == user.CorporationId);

            if (current == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<PreExonerated>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            if (current.Billed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<PreExonerated>
                {
                    WasSuccess = false,
                    Message = "No se puede editar una exoneracion facturada."
                };
            }

            current.DateExonerated = NormalizeDate(model.DateExonerated);
            current.ExoneratedControl = model.ExoneratedControl;
            current.ContractClientId = model.ContractClientId;
            current.YearNumber = model.YearNumber;
            current.MonthType = model.MonthType;

            var response = await PreparePreExoneratedAsync(current, user.CorporationId!.Value);
            if (!response.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return response;
            }

            var exists = await _context.PreExonerateds.AnyAsync(x =>
                x.PreExoneratedId != current.PreExoneratedId &&
                x.CorporationId == current.CorporationId &&
                x.ContractClientId == current.ContractClientId &&
                x.YearNumber == current.YearNumber &&
                x.MonthType == current.MonthType);

            if (exists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<PreExonerated>
                {
                    WasSuccess = false,
                    Message = "Ya existe una exoneracion para este contrato, ano y mes."
                };
            }

            _context.PreExonerateds.Update(current);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<PreExonerated> { WasSuccess = true, Result = current };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<PreExonerated>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeletePreExoneratedAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<bool>();
            }

            var current = await _context.PreExonerateds
                .FirstOrDefaultAsync(x => x.PreExoneratedId == id && x.CorporationId == user.CorporationId);

            if (current == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            if (current.Billed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No se puede eliminar una exoneracion facturada."
                };
            }

            _context.PreExonerateds.Remove(current);
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

    private async Task<ActionResponse<PrePayment>> PreparePrePaymentAsync(PrePayment model, int corporationId)
    {
        var contract = await _context.ContractClients
            .Include(x => x.ContractPlans!)
                .ThenInclude(x => x.Plan!)
                    .ThenInclude(x => x.Tax)
            .FirstOrDefaultAsync(x => x.ContractClientId == model.ContractClientId &&
                                      x.CorporationId == corporationId &&
                                      x.ContractState == ContractState.Active);

        if (contract == null)
        {
            return new ActionResponse<PrePayment>
            {
                WasSuccess = false,
                Message = "Debe seleccionar un contrato activo."
            };
        }

        var contractPlan = contract.ContractPlans?.FirstOrDefault(x => x.Plan != null);
        if (contractPlan?.Plan == null)
        {
            return new ActionResponse<PrePayment>
            {
                WasSuccess = false,
                Message = "El contrato seleccionado no tiene plan configurado."
            };
        }

        var plan = contractPlan.Plan;
        var rate = plan.Tax?.Rate ?? 0;

        model.ClientId = contract.ClientId;
        model.PlanId = plan.PlanId;
        model.TaxRate = rate;
        model.UnitPrice = plan.Price;
        model.PriceWithTax = Math.Round(plan.Price + ((plan.Price * rate) / 100), 2);
        model.Billed = false;
        model.DateBilled = null;
        model.CxCBillId = null;

        return new ActionResponse<PrePayment> { WasSuccess = true, Result = model };
    }

    private async Task<ActionResponse<PreExonerated>> PreparePreExoneratedAsync(PreExonerated model, int corporationId)
    {
        var contract = await _context.ContractClients
            .Include(x => x.ContractPlans!)
                .ThenInclude(x => x.Plan!)
                    .ThenInclude(x => x.Tax)
            .FirstOrDefaultAsync(x => x.ContractClientId == model.ContractClientId &&
                                      x.CorporationId == corporationId &&
                                      x.ContractState == ContractState.Active);

        if (contract == null)
        {
            return new ActionResponse<PreExonerated>
            {
                WasSuccess = false,
                Message = "Debe seleccionar un contrato activo."
            };
        }

        var contractPlan = contract.ContractPlans?.FirstOrDefault(x => x.Plan != null);
        if (contractPlan?.Plan == null)
        {
            return new ActionResponse<PreExonerated>
            {
                WasSuccess = false,
                Message = "El contrato seleccionado no tiene plan configurado."
            };
        }

        var plan = contractPlan.Plan;
        var rate = plan.Tax?.Rate ?? 0;

        model.ClientId = contract.ClientId;
        model.PlanId = plan.PlanId;
        model.TaxRate = rate;
        model.UnitPrice = plan.Price;
        model.PriceWithTax = Math.Round(plan.Price + ((plan.Price * rate) / 100), 2);
        model.Billed = false;
        model.DateBilled = null;
        model.CxCBillId = null;

        return new ActionResponse<PreExonerated> { WasSuccess = true, Result = model };
    }

    private static void ApplyDefaultMonth(PrePayment model)
    {
        if (model.YearNumber > 0 && Enum.IsDefined(model.MonthType))
            return;

        var nextMonth = DateTime.UtcNow.AddMonths(1);
        model.YearNumber = nextMonth.Year;
        model.MonthType = (MonthType)nextMonth.Month;
    }

    private static void ApplyDefaultMonth(PreExonerated model)
    {
        if (model.YearNumber > 0 && Enum.IsDefined(model.MonthType))
            return;

        var nextMonth = DateTime.UtcNow.AddMonths(1);
        model.YearNumber = nextMonth.Year;
        model.MonthType = (MonthType)nextMonth.Month;
    }

    private static DateTime NormalizeDate(DateTime date) =>
        DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };
}
