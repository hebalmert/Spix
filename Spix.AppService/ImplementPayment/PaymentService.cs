using Spix.AppService.ImplementContratos;
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
using Spix.Domain.Entities;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesPayment;
using Spix.Domain.EntitiesSchedule;
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
    private readonly IContractorPaymentService _contractorPaymentService;

    public PaymentService(DataContext context, IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper, ITransactionManager transactionManager, HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer, IEnumMultilLanguageService enumMultilLanguageService,
        IContractorPaymentService contractorPaymentService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _transactionManager = transactionManager;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _enumMultilLanguageService = enumMultilLanguageService;
        _contractorPaymentService = contractorPaymentService;
    }

    public async Task<ActionResponse<IEnumerable<CxCBill>>> GetCxCBillsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<CxCBill>>();

            var queryable = _context.CxCBills.AsNoTracking()
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
                .AsSplitQuery()
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

            //Una sola nota: se trae con lo que se le esta cobrando (plan y servicios) para poder
            //explicarle al cliente por que le llego ese valor.
            var model = await _context.CxCBills
                .AsNoTracking()
                .Include(x => x.Client)
                .Include(x => x.ContractClient)
                .Include(x => x.CxCBillDetails)
                .Include(x => x.Sell!)
                    .ThenInclude(x => x.SellDetails)
                .AsSplitQuery()
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

    //El recaudo de una nota de cobro. Es plata: la nota se reclama de forma atomica para que
    //dos usuarios no la cobren dos veces, y el movimiento queda en la bitacora del dinero.
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

            //Solo la fila de la nota: la factura y el detalle se tocan por su id, sin traerlos
            var bill = await _context.CxCBills
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CxCBillId == model.CxCBillId && x.CorporationId == user.CorporationId);

            if (bill == null)
                return await FailRollbackAsync<CxCBill>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (bill.Cancelled)
                return await FailRollbackAsync<CxCBill>(_localizer["Pay_BillCancelled"]);

            if (bill.Paid || bill.Balance <= 0)
                return await FailRollbackAsync<CxCBill>(_localizer["Pay_BillPaid"]);

            if (!IsValidDiscount(model.DiscountPercent))
                return await FailRollbackAsync<CxCBill>(_localizer["Pay_DiscountInvalid"]);

            if (model.DiscountPercent > 0 && string.IsNullOrWhiteSpace(model.Detail))
                return await FailRollbackAsync<CxCBill>(_localizer["Pay_DiscountReason"]);

            if (!IsValidPaymentMode(model.PaymentMode))
                return await FailRollbackAsync<CxCBill>(_localizer["Pay_ModeInvalid"]);

            //Las cuentas del recaudo
            var debt = bill.Balance;
            var discount = Math.Round((debt * model.DiscountPercent) / 100, 2);
            var payment = debt - discount;
            var balance = debt - discount - payment;
            var today = DateTime.UtcNow.Date;
            var isPaid = balance == 0;

            //Se reclama la nota. Si otro usuario la cobro primero, aqui salen cero filas
            var claimed = await _context.CxCBills
                .Where(x => x.CxCBillId == bill.CxCBillId &&
                            !x.Paid &&
                            !x.Cancelled &&
                            x.Balance == debt)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Balance, balance)
                    .SetProperty(x => x.Paid, isPaid)
                    .SetProperty(x => x.DatePaid, isPaid ? today : (DateTime?)null));

            if (claimed == 0)
                return await FailRollbackAsync<CxCBill>(_localizer["Pay_BillPaid"]);

            //La factura sigue el estado de la nota
            await _context.Sells
                .Where(x => x.SellId == bill.SellId && x.CorporationId == bill.CorporationId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Paid, isPaid)
                    .SetProperty(x => x.DatePaid, isPaid ? today : (DateTime?)null));

            //El renglon del recaudo. El id se fija aqui porque la cuenta por pagar del
            //contratista lo necesita antes de guardar.
            var detail = new CxCBillDetail
            {
                CxCBillDetailId = Guid.NewGuid(),
                CxCBillId = bill.CxCBillId,
                DatePayment = today,
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

            //La comision del contratista, si ese contrato la tiene
            await _contractorPaymentService.CreateAccountPayableAsync(bill, detail);

            //Bitacora del dinero: quien recibio, cuanto, con que modo y con que descuento
            AuditPayment(bill, detail, model.DiscountPercent, user);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            //Lo que se devuelve refleja lo que quedo en la base
            bill.Balance = balance;
            bill.Paid = isPaid;
            bill.DatePaid = isPaid ? today : null;

            return new ActionResponse<CxCBill> { WasSuccess = true, Result = bill };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<CxCBill>(ex);
        }
    }

    //Anota el recaudo en la bitacora del dinero
    private void AuditPayment(CxCBill bill, CxCBillDetail detail, int discountPercent, User user)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        var modo = detail.PaymentMode ?? string.Empty;
        var texto = discountPercent > 0
            ? $"{bill.CollectionNote} - {modo} - descuento {discountPercent}% ({detail.Discount:N2}) - {detail.Detail}"
            : $"{bill.CollectionNote} - {modo}";

        PaymentAuditLog.Add(
            _context,
            bill.CorporationId,
            PaymentEventType.PaymentReceived,
            bill.ContractClientId,
            bill.ClientId,
            bill.CxCBillId,
            nameof(CxCBill),
            detail.Payment,
            0,
            detail.Payment,
            texto,
            $"{user.FirstName} {user.LastName}",
            Guid.TryParse(user.Id, out var userId) ? userId : null,
            request?.HttpContext.Connection.RemoteIpAddress?.ToString(),
            request?.Headers["User-Agent"].ToString());
    }

    //Los modos de pago que acepta el recaudo
    private static bool IsValidPaymentMode(string? mode) =>
        mode is "Cash" or "Card" or "Transfer";

    //Deshace la transaccion y devuelve el motivo
    private async Task<ActionResponse<T>> FailRollbackAsync<T>(string message)
    {
        await _transactionManager.RollbackTransactionAsync();
        return new ActionResponse<T> { WasSuccess = false, Message = message };
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

            var isTechnician = await _context.UserRoleDetails
                .AnyAsync(x => x.UserId == user.Id && x.UserType == UserType.Technician);

            if (isTechnician)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<CxCBill> { WasSuccess = false, Message = "El tecnico no puede anular cuentas por cobrar." };
            }

            if (string.IsNullOrWhiteSpace(model.DescriptionCancelled))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<CxCBill> { WasSuccess = false, Message = "Debe especificar el motivo de anulacion." };
            }

            var bill = await _context.CxCBills
                .Include(x => x.Sell)
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
            bill.Paid = false;
            bill.DatePaid = null;

            if (bill.Sell != null)
            {
                bill.Sell.Cancelled = true;
                bill.Sell.DateCancelled = bill.DateCancelled;
                bill.Sell.Paid = false;
                bill.Sell.DatePaid = null;
            }

            var prePayments = await _context.PrePayments
                .Where(x => x.CorporationId == user.CorporationId && x.CxCBillId == bill.CxCBillId)
                .ToListAsync();

            foreach (var prePayment in prePayments)
            {
                prePayment.Billed = false;
                prePayment.DateBilled = null;
                prePayment.CxCBillId = null;
            }

            var preExonerateds = await _context.ContractExonerateds
                .Where(x => x.CorporationId == user.CorporationId && x.CxCBillId == bill.CxCBillId)
                .ToListAsync();

            foreach (var preExonerated in preExonerateds)
            {
                preExonerated.Billed = false;
                preExonerated.DateBilled = null;
                preExonerated.CxCBillId = null;
            }

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

    //Anota el movimiento del pago adelantado en la bitacora del dinero
    private void AuditPrePayment(PrePayment model, PaymentEventType eventType, User user)
    {
        var lineas = model.PrePaymentDetails?.Count(x => x.ServiceRequestDetailId.HasValue) ?? 0;
        var request = _httpContextAccessor.HttpContext?.Request;

        PaymentAuditLog.Add(
            _context,
            model.CorporationId,
            eventType,
            model.ContractClientId,
            model.ClientId,
            model.PrePaymentId,
            nameof(PrePayment),
            model.UnitPrice,
            model.PriceWithTax - model.UnitPrice,
            model.PriceWithTax,
            $"{model.MonthType} {model.YearNumber} - plan y {lineas} servicio(s)",
            $"{user.FirstName} {user.LastName}",
            Guid.TryParse(user.Id, out var userId) ? userId : null,
            request?.HttpContext.Connection.RemoteIpAddress?.ToString(),
            request?.Headers["User-Agent"].ToString());
    }

    //El tablero: lo recibido que aun no se cruza con una nota, y lo que se cruzo en el mes
    public async Task<ActionResponse<PrePaymentSummaryDto>> GetPrePaymentSummaryAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<PrePaymentSummaryDto>();

            var pendientes = _context.PrePayments
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId && !x.Billed);

            //Una sola pasada para los tres numeros de lo pendiente
            var summary = await pendientes
                .GroupBy(x => 1)
                .Select(g => new PrePaymentSummaryDto
                {
                    Pending = g.Count(),
                    PendingTotal = g.Sum(x => x.PriceWithTax),
                    Contracts = g.Select(x => x.ContractClientId).Distinct().Count()
                })
                .FirstOrDefaultAsync() ?? new PrePaymentSummaryDto();

            var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var nextMonth = monthStart.AddMonths(1);

            var delMes = _context.PrePayments
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.Billed &&
                            x.DateBilled >= monthStart &&
                            x.DateBilled < nextMonth);

            summary.BilledMonth = await delMes.CountAsync();
            summary.BilledMonthTotal = await delMes.SumAsync(x => (decimal?)x.PriceWithTax) ?? 0;

            return new ActionResponse<PrePaymentSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<PrePaymentSummaryDto>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<PrePayment>>> GetPrePaymentsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<PrePayment>>();

            var queryable = _context.PrePayments.AsNoTracking()
                .Include(x => x.Client)
                .Include(x => x.ContractClient)
                .Include(x => x.Plan)
                .Include(x => x.PrePaymentDetails!)
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
                .AsSplitQuery()
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
                .Include(x => x.PrePaymentDetails!)
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
                .AsSplitQuery()
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
                    Message = _localizer["PrePayment_Repeated"]
                };
            }

            _context.PrePayments.Add(model);

            //Es plata recibida: queda su rastro dentro de la misma transaccion
            AuditPrePayment(model, PaymentEventType.PrePaymentCreated, user);

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
                    Message = _localizer["PrePayment_BilledNoEdit"]
                };
            }

            current.DatePayment = NormalizeDate(model.DatePayment);
            current.PaymentControl = model.PaymentControl;
            current.ContractClientId = model.ContractClientId;
            current.YearNumber = model.YearNumber;
            current.MonthType = model.MonthType;

            //Las lineas se arman de nuevo: se borran las anteriores y se liberan sus servicios
            var oldLines = await _context.PrePaymentDetails
                .Where(x => x.PrePaymentId == current.PrePaymentId)
                .ToListAsync();
            _context.PrePaymentDetails.RemoveRange(oldLines);
            current.PrePaymentDetails = model.PrePaymentDetails;

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
                    Message = _localizer["PrePayment_Repeated"]
                };
            }

            _context.PrePayments.Update(current);

            AuditPrePayment(current, PaymentEventType.PrePaymentUpdated, user);

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
                    Message = _localizer["PrePayment_BilledNoDelete"]
                };
            }

            //La foto queda antes de borrar: el rastro sobrevive al registro
            AuditPrePayment(current, PaymentEventType.PrePaymentDeleted, user);

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

    public async Task<ActionResponse<IEnumerable<ContractExonerated>>> GetContractExoneratedsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ContractExonerated>>();

            var queryable = _context.ContractExonerateds.AsNoTracking()
                .Include(x => x.Client)
                .Include(x => x.ContractClient)
                .Include(x => x.Plan)
                //Solo las vigentes: las cerradas quedan como historia del registro
                .Where(x => x.CorporationId == user.CorporationId && !x.Billed && x.DateEnded == null)
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

            return new ActionResponse<IEnumerable<ContractExonerated>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractExonerated>>(ex);
        }
    }

    public async Task<ActionResponse<ContractExonerated>> GetContractExoneratedAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ContractExonerated>();

            var model = await _context.ContractExonerateds
                .Include(x => x.Client)
                .Include(x => x.ContractClient!)
                    .ThenInclude(x => x.Zone!)
                        .ThenInclude(x => x.City)
                .Include(x => x.ContractClient!)
                    .ThenInclude(x => x.ContractPlans!)
                        .ThenInclude(x => x.Plan!)
                            .ThenInclude(x => x.Tax)
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x => x.ContractExoneratedId == id && x.CorporationId == user.CorporationId);

            if (model == null)
            {
                return new ActionResponse<ContractExonerated>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<ContractExonerated> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractExonerated>(ex);
        }
    }

    public async Task<ActionResponse<ContractExonerated>> AddContractExoneratedAsync(ContractExonerated model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ContractExonerated>();
            }

            var response = await PrepareContractExoneratedAsync(model, user.CorporationId!.Value);
            if (!response.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return response;
            }

            model.DateExonerated = NormalizeDate(model.DateExonerated == default ? DateTime.UtcNow : model.DateExonerated);
            ApplyDefaultMonth(model);
            model.CorporationId = user.CorporationId.Value;
            model.UserByName = $"{user.FirstName} {user.LastName}";
            model.UserId = Guid.Parse(user.Id);

            //Solo estorba una exoneracion VIGENTE del mismo mes; las cerradas son historia
            var exists = await _context.ContractExonerateds.AnyAsync(x =>
                x.CorporationId == model.CorporationId &&
                x.ContractClientId == model.ContractClientId &&
                x.YearNumber == model.YearNumber &&
                x.MonthType == model.MonthType &&
                x.DateEnded == null);

            if (exists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractExonerated>
                {
                    WasSuccess = false,
                    Message = "Ya existe una exoneracion para este contrato, ano y mes."
                };
            }

            _context.ContractExonerateds.Add(model);

            await ContractAuditLog.AddAsync(_context, model.ContractClientId, ContractEventType.MonthExonerated,
                $"{model.MonthType} {model.YearNumber}", model.UserByName, model.UserId,
                referenceId: model.ContractExoneratedId, clientId: model.ClientId,
                corporationId: model.CorporationId);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractExonerated> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractExonerated>(ex);
        }
    }

    public async Task<ActionResponse<ContractExonerated>> UpdateContractExoneratedAsync(ContractExonerated model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ContractExonerated>();
            }

            var current = await _context.ContractExonerateds
                .FirstOrDefaultAsync(x => x.ContractExoneratedId == model.ContractExoneratedId && x.CorporationId == user.CorporationId);

            if (current == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractExonerated>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            if (current.Billed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractExonerated>
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

            var response = await PrepareContractExoneratedAsync(current, user.CorporationId!.Value);
            if (!response.WasSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return response;
            }

            var exists = await _context.ContractExonerateds.AnyAsync(x =>
                x.DateEnded == null &&
                x.ContractExoneratedId != current.ContractExoneratedId &&
                x.CorporationId == current.CorporationId &&
                x.ContractClientId == current.ContractClientId &&
                x.YearNumber == current.YearNumber &&
                x.MonthType == current.MonthType);

            if (exists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractExonerated>
                {
                    WasSuccess = false,
                    Message = "Ya existe una exoneracion para este contrato, ano y mes."
                };
            }

            _context.ContractExonerateds.Update(current);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractExonerated> { WasSuccess = true, Result = current };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractExonerated>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteContractExoneratedAsync(Guid id, string username)
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

            var current = await _context.ContractExonerateds
                .FirstOrDefaultAsync(x => x.ContractExoneratedId == id && x.CorporationId == user.CorporationId);

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

            //No se borra: se cierra, y asi queda quien la retiro y cuando
            current.DateEnded = DateTime.UtcNow;
            current.UserIdEnded = Guid.Parse(user.Id);
            current.UserByNameEnded = $"{user.FirstName} {user.LastName}";
            _context.ContractExonerateds.Update(current);

            await ContractAuditLog.AddAsync(_context, current.ContractClientId, ContractEventType.MonthExoneratedClosed,
                $"{current.MonthType} {current.YearNumber}", current.UserByNameEnded, current.UserIdEnded,
                referenceId: current.ContractExoneratedId, clientId: current.ClientId,
                corporationId: current.CorporationId);
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

    //Arma el pago adelantado con sus lineas: el plan del contrato y los servicios que el usuario elija.
    //Los precios SIEMPRE salen de la base, nunca del navegador, y quedan congelados: es plata recibida.
    //Si el plan sube antes de facturar, la nota de cobro se queda con la diferencia como saldo.
    private async Task<ActionResponse<PrePayment>> PreparePrePaymentAsync(PrePayment model, int corporationId)
    {
        //Validacion: el contrato tiene que estar activo
        var contract = await _context.ContractClients
            .Include(x => x.Client)
            .Include(x => x.Zone)
                .ThenInclude(x => x!.City)
            .Include(x => x.ContractPlans!)
                .ThenInclude(x => x.Plan!)
                    .ThenInclude(x => x.Tax)
            .FirstOrDefaultAsync(x => x.ContractClientId == model.ContractClientId &&
                                      x.CorporationId == corporationId &&
                                      x.ContractState == ContractState.Active);

        if (contract == null)
        {
            return Fail<PrePayment>(_localizer["PrePayment_NeedActiveContract"]);
        }

        var contractPlan = contract.ContractPlans?.FirstOrDefault(x => x.Plan != null);
        if (contractPlan?.Plan == null)
        {
            return Fail<PrePayment>(_localizer["PrePayment_NoPlan"]);
        }

        var plan = contractPlan.Plan;
        var planRate = plan.Tax?.Rate ?? 0;
        var planTax = CalculateTax(plan.Price, planRate);

        //Los servicios que llegan del formulario: del cliente solo se toman los ids
        var selectedDetailIds = (model.PrePaymentDetails ?? new List<PrePaymentDetail>())
            .Where(x => x.ServiceRequestDetailId.HasValue)
            .Select(x => x.ServiceRequestDetailId!.Value)
            .Distinct()
            .ToList();

        var lines = new List<PrePaymentDetail>
        {
            new()
            {
                LineType = PrePaymentLineType.Plan,
                Concept = $"Plan {plan.PlanName}",
                PlanId = plan.PlanId,
                TaxRate = planRate,
                UnitPrice = plan.Price,
                TaxAmount = planTax,
                PriceWithTax = plan.Price + planTax,
                CorporationId = corporationId
            }
        };

        if (selectedDetailIds.Count > 0)
        {
            //Solo se aceptan servicios del mismo contrato, completados, sin facturar y libres
            var available = await GetAvailableServicesAsync(model.ContractClientId, corporationId, model.PrePaymentId);
            var byId = available.ToDictionary(x => x.ServiceRequestDetailId);

            foreach (var detailId in selectedDetailIds)
            {
                if (!byId.TryGetValue(detailId, out var service))
                {
                    return Fail<PrePayment>(_localizer["PrePayment_ServiceNotAvailable"]);
                }

                lines.Add(new PrePaymentDetail
                {
                    LineType = PrePaymentLineType.Service,
                    Concept = $"Solicitud #{service.RequestNumber} - {service.ServiceName}",
                    ServiceRequestId = service.ServiceRequestId,
                    ServiceRequestDetailId = service.ServiceRequestDetailId,
                    TaxRate = service.TaxRate,
                    UnitPrice = service.Price,
                    TaxAmount = service.TaxAmount,
                    PriceWithTax = service.Total,
                    CorporationId = corporationId
                });
            }
        }

        //El encabezado es la suma de sus lineas
        model.ClientId = contract.ClientId;
        model.PlanId = plan.PlanId;
        model.TaxRate = planRate;
        model.UnitPrice = lines.Sum(x => x.UnitPrice);
        model.PriceWithTax = lines.Sum(x => x.PriceWithTax);
        model.Billed = false;
        model.DateBilled = null;
        model.CxCBillId = null;
        model.PrePaymentDetails = lines;

        return new ActionResponse<PrePayment> { WasSuccess = true, Result = model };
    }

    //Los servicios que se pueden adelantar de un contrato: completados, sin facturar, con valor
    //mayor a cero y que no esten reservados en otro pago adelantado.
    private async Task<List<PrePaymentServiceDto>> GetAvailableServicesAsync(Guid contractClientId, int corporationId, Guid? currentPrePaymentId)
    {
        //Los que ya estan reservados en otro pago adelantado sin facturar
        var reserved = _context.PrePaymentDetails
            .Where(x => x.CorporationId == corporationId &&
                        x.ServiceRequestDetailId != null &&
                        x.PrePaymentId != currentPrePaymentId &&
                        !x.PrePayment!.Billed)
            .Select(x => x.ServiceRequestDetailId!.Value);

        return await _context.ServiceRequestDetails
            .AsNoTracking()
            .Where(x => x.ServiceRequest!.CorporationId == corporationId &&
                        x.ServiceRequest.ContractClientId == contractClientId &&
                        x.ServiceRequest.ScheduleStatus == ScheduleStatus.Completed &&
                        !x.ServiceRequest.Billed &&
                        x.Price > 0 &&
                        !reserved.Contains(x.ServiceRequestDetailId))
            .OrderBy(x => x.ServiceRequest!.RequestNumber)
            .Select(x => new PrePaymentServiceDto
            {
                ServiceRequestId = x.ServiceRequestId,
                ServiceRequestDetailId = x.ServiceRequestDetailId,
                RequestNumber = x.ServiceRequest!.RequestNumber,
                CompletedAtUtc = x.ServiceRequest.CompletedAtUtc,
                ServiceName = x.ServiceClient!.ServiceName,
                Detail = x.Detail,
                TaxRate = x.TaxRate,
                Price = x.Price,
                TaxAmount = x.TaxAmount,
                Total = x.Price + x.TaxAmount
            })
            .ToListAsync();
    }

    //Los servicios que puede adelantar un contrato, para el formulario
    public async Task<ActionResponse<IEnumerable<PrePaymentServiceDto>>> GetPrePaymentServicesAsync(Guid contractClientId, Guid? prePaymentId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<PrePaymentServiceDto>>();

            var list = await GetAvailableServicesAsync(contractClientId, user.CorporationId!.Value, prePaymentId);

            return new ActionResponse<IEnumerable<PrePaymentServiceDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<PrePaymentServiceDto>>(ex);
        }
    }

    private static decimal CalculateTax(decimal unitPrice, decimal taxRate) =>
        Math.Round((unitPrice * taxRate) / 100, 2);

    private ActionResponse<T> Fail<T>(string message) => new() { WasSuccess = false, Message = message };

    private async Task<ActionResponse<ContractExonerated>> PrepareContractExoneratedAsync(ContractExonerated model, int corporationId)
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
            return new ActionResponse<ContractExonerated>
            {
                WasSuccess = false,
                Message = "Debe seleccionar un contrato activo."
            };
        }

        var contractPlan = contract.ContractPlans?.FirstOrDefault(x => x.Plan != null);
        if (contractPlan?.Plan == null)
        {
            return new ActionResponse<ContractExonerated>
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

        //Foto del momento: lo que se ve hoy queda guardado en el registro
        model.ControlContrato = contract.ControlContrato;
        model.ClientName = $"{contract.Client?.FirstName} {contract.Client?.LastName}".Trim();
        model.ClientDocument = contract.Client?.Document;
        model.ContractAddress = contract.Address;
        model.ContractPhone = contract.PhoneNumber;
        model.CityName = contract.Zone?.City?.Name;
        model.ZoneName = contract.Zone?.ZoneName;
        model.PlanName = plan.PlanName;

        return new ActionResponse<ContractExonerated> { WasSuccess = true, Result = model };
    }

    private static void ApplyDefaultMonth(PrePayment model)
    {
        if (model.YearNumber > 0 && Enum.IsDefined(model.MonthType))
            return;

        var nextMonth = DateTime.UtcNow.AddMonths(1);
        model.YearNumber = nextMonth.Year;
        model.MonthType = (MonthType)nextMonth.Month;
    }

    private static void ApplyDefaultMonth(ContractExonerated model)
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
