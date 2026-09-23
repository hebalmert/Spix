using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Sequences;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesPayment;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementPayment;

public class ContractorPaymentService : IContractorPaymentService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public ContractorPaymentService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper,
        ITransactionManager transactionManager,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _transactionManager = transactionManager;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task CreateAccountPayableAsync(CxCBill cxCBill, CxCBillDetail cxCBillDetail)
    {
        if (cxCBillDetail.Payment <= 0)
        {
            return;
        }

        var alreadyCreated = await _context.ContractorAccountPayables.AnyAsync(x =>
            x.CorporationId == cxCBill.CorporationId &&
            x.CxCBillDetailId == cxCBillDetail.CxCBillDetailId);

        if (alreadyCreated)
        {
            return;
        }

        var contract = await _context.ContractClients
            .Include(x => x.Contractor)
            .FirstOrDefaultAsync(x => x.ContractClientId == cxCBill.ContractClientId &&
                                      x.CorporationId == cxCBill.CorporationId);

        var contractor = contract?.Contractor;
        if (contractor == null || !contractor.Active || !contractor.GuardarPago || contractor.Rate <= 0)
        {
            return;
        }

        var total = Math.Round((cxCBillDetail.Payment * contractor.Rate) / 100, 2);
        if (total <= 0)
        {
            return;
        }

        var accountPayable = new ContractorAccountPayable
        {
            ContractorAccountPayableId = Guid.NewGuid(),
            DateCreated = cxCBillDetail.DatePayment,
            ContractorId = contractor.ContractorId,
            ContractClientId = cxCBill.ContractClientId,
            CxCBillId = cxCBill.CxCBillId,
            CxCBillDetailId = cxCBillDetail.CxCBillDetailId,
            Rate = contractor.Rate,
            BaseAmount = cxCBillDetail.Payment,
            Total = total,
            Balance = total,
            Paid = false,
            CorporationId = cxCBill.CorporationId,
            UsuarioOwner = cxCBillDetail.UsuarioOwner,
            UserId = cxCBillDetail.UserId
        };

        _context.ContractorAccountPayables.Add(accountPayable);

        //Bitacora del dinero: la comision que se le causa al contratista por ese recaudo
        PaymentAuditLog.Add(
            _context,
            cxCBill.CorporationId,
            PaymentEventType.ContractorAccrued,
            cxCBill.ContractClientId,
            cxCBill.ClientId,
            accountPayable.ContractorAccountPayableId,
            nameof(ContractorAccountPayable),
            total,
            0,
            total,
            $"{contractor.FirstName} {contractor.LastName} - {contractor.Rate:N2}% de {cxCBillDetail.Payment:N2} - {cxCBill.CollectionNote}",
            cxCBillDetail.UsuarioOwner,
            cxCBillDetail.UserId);
    }

    //El tablero: lo que se le debe a los contratistas y lo que sigue sin agrupar
    public async Task<ActionResponse<CxCContractorSummaryDto>> GetCxCSummaryAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<CxCContractorSummaryDto>();

            var summary = new CxCContractorSummaryDto();

            //Las comisiones causadas que todavia no estan en ninguna cuenta
            var pendientes = await _context.ContractorAccountPayables
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.CxCContractorId == null &&
                            !x.Paid)
                .GroupBy(x => 1)
                .Select(g => new { Count = g.Count(), Total = g.Sum(x => x.Balance) })
                .FirstOrDefaultAsync();

            summary.Pending = pendientes?.Count ?? 0;
            summary.PendingTotal = pendientes?.Total ?? 0;

            //Las cuentas abiertas y lo que falta por pagarles
            var abiertas = await _context.CxCContractors
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId && !x.Paid && !x.Cancelled)
                .GroupBy(x => 1)
                .Select(g => new { Count = g.Count(), Total = g.Sum(x => x.Balance) })
                .FirstOrDefaultAsync();

            summary.OpenNotes = abiertas?.Count ?? 0;
            summary.OpenBalance = abiertas?.Total ?? 0;

            return new ActionResponse<CxCContractorSummaryDto> { WasSuccess = true, Result = summary };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<CxCContractorSummaryDto>(ex);
        }
    }

    //Las comisiones pendientes de un contratista, para armar su cuenta
    public async Task<ActionResponse<IEnumerable<ContractorPendingDto>>> GetPendingAsync(Guid contractorId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ContractorPendingDto>>();

            var list = await _context.ContractorAccountPayables
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.ContractorId == contractorId &&
                            x.CxCContractorId == null &&
                            !x.Paid)
                .OrderBy(x => x.DateCreated)
                .Select(x => new ContractorPendingDto
                {
                    ContractorAccountPayableId = x.ContractorAccountPayableId,
                    DateCreated = x.DateCreated,
                    ControlContrato = x.ContractClient!.ControlContrato,
                    ClientFullName = x.ContractClient.Client!.FirstName + " " + x.ContractClient.Client.LastName,
                    CollectionNote = x.CxCBill!.CollectionNote,
                    BaseAmount = x.BaseAmount,
                    Rate = x.Rate,
                    Total = x.Balance
                })
                .ToListAsync();

            return new ActionResponse<IEnumerable<ContractorPendingDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractorPendingDto>>(ex);
        }
    }

    //Los contratistas que tienen comisiones pendientes, con el neutro al frente
    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboContractorsAsync(string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<GuidItemModel>>();

            var list = await _context.ContractorAccountPayables
                .AsNoTracking()
                .Where(x => x.CorporationId == user.CorporationId &&
                            x.CxCContractorId == null &&
                            !x.Paid)
                .Select(x => new GuidItemModel
                {
                    Value = x.ContractorId,
                    Name = x.Contractor!.FirstName + " " + x.Contractor.LastName
                })
                .Distinct()
                .OrderBy(x => x.Name)
                .ToListAsync();

            list.Insert(0, new GuidItemModel { Value = Guid.Empty, Name = _localizer["Contractor_SelectOne"] });

            return new ActionResponse<IEnumerable<GuidItemModel>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex);
        }
    }

    //Arma la cuenta del contratista con las comisiones que se le indiquen. Si no viene
    //ninguna, se agrupan TODAS las pendientes de ese contratista.
    public async Task<ActionResponse<CxCContractor>> CreateCxCContractorAsync(CxCContractorCreateDto model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null || !user.CorporationId.HasValue)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<CxCContractor>();
            }

            var corporationId = user.CorporationId.Value;
            var contractor = await _context.Contractors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractorId == model.ContractorId &&
                                          x.CorporationId == corporationId);

            if (contractor == null)
                return await FailRollbackAsync<CxCContractor>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var ids = model.ContractorAccountPayableIds.Where(x => x != Guid.Empty).Distinct().ToList();

            //La cuenta nace primero: las comisiones se amarran a ella
            var note = new CxCContractor
            {
                CxCContractorId = Guid.NewGuid(),
                DateNote = DateTime.UtcNow.Date,
                NoteNumber = $"CC-{await NumberSequence.NextAsync(_context, corporationId, NumberKind.ContractorPayment):0000000}",
                ContractorId = contractor.ContractorId,
                Description = $"{contractor.FirstName} {contractor.LastName}",
                CorporationId = corporationId,
                UsuarioOwner = $"{user.FirstName} {user.LastName}",
                UserId = Guid.Parse(user.Id)
            };

            _context.CxCContractors.Add(note);
            await _context.SaveChangesAsync();

            //Se reclaman las comisiones: las que ya esten en otra cuenta no salen
            var claimed = await _context.ContractorAccountPayables
                .Where(x => x.CorporationId == corporationId &&
                            x.ContractorId == contractor.ContractorId &&
                            x.CxCContractorId == null &&
                            !x.Paid &&
                            (ids.Count == 0 || ids.Contains(x.ContractorAccountPayableId)))
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.CxCContractorId, note.CxCContractorId));

            if (claimed == 0)
                return await FailRollbackAsync<CxCContractor>(_localizer["Contractor_NothingPending"]);

            //El total sale de lo que quedo amarrado, no de lo que mando la pantalla
            var total = await _context.ContractorAccountPayables
                .Where(x => x.CxCContractorId == note.CxCContractorId)
                .SumAsync(x => (decimal?)x.Balance) ?? 0;

            note.Total = total;
            note.Balance = total;

            //Bitacora del dinero: la cuenta que nace y con cuantas comisiones
            var request = _httpContextAccessor.HttpContext?.Request;
            PaymentAuditLog.Add(
                _context,
                corporationId,
                PaymentEventType.ContractorAccrued,
                null,
                null,
                note.CxCContractorId,
                nameof(CxCContractor),
                total,
                0,
                total,
                $"{note.NoteNumber} - {note.Description} - {claimed} comision(es)",
                note.UsuarioOwner,
                note.UserId,
                request?.HttpContext.Connection.RemoteIpAddress?.ToString(),
                request?.Headers["User-Agent"].ToString());

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<CxCContractor> { WasSuccess = true, Result = note };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<CxCContractor>(ex);
        }
    }

    //Las cuentas por pagar a contratistas, paginadas
    public async Task<ActionResponse<IEnumerable<CxCContractor>>> GetCxCContractorsAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<CxCContractor>>();

            var queryable = _context.CxCContractors
                .AsNoTracking()
                .Include(x => x.Contractor)
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.NoteNumber!, $"%{filter}%") ||
                    EF.Functions.Like(x.Contractor!.FirstName, $"%{filter}%") ||
                    EF.Functions.Like(x.Contractor!.LastName, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderBy(x => x.Paid)
                .ThenByDescending(x => x.DateNote)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<CxCContractor>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<CxCContractor>>(ex);
        }
    }

    //Las comisiones que componen la cuenta, pagina por pagina: pueden ser cientos
    public async Task<ActionResponse<IEnumerable<ContractorPendingDto>>> GetCommissionsAsync(Guid id, PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ContractorPendingDto>>();

            var queryable = _context.ContractorAccountPayables
                .AsNoTracking()
                .Where(x => x.CxCContractorId == id && x.CorporationId == user.CorporationId)
                .Select(x => new ContractorPendingDto
                {
                    ContractorAccountPayableId = x.ContractorAccountPayableId,
                    DateCreated = x.DateCreated,
                    ControlContrato = x.ContractClient!.ControlContrato,
                    ClientFullName = x.ContractClient.Client!.FirstName + " " + x.ContractClient.Client.LastName,
                    CollectionNote = x.CxCBill!.CollectionNote,
                    BaseAmount = x.BaseAmount,
                    Rate = x.Rate,
                    Total = x.Total
                });

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderBy(x => x.ControlContrato)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ContractorPendingDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractorPendingDto>>(ex);
        }
    }

    //Los abonos de la cuenta, pagina por pagina
    public async Task<ActionResponse<IEnumerable<CxCContractorPaymentItemDto>>> GetPaymentsAsync(Guid id, PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<CxCContractorPaymentItemDto>>();

            var queryable = _context.CxCContractorDetails
                .AsNoTracking()
                .Where(x => x.CxCContractorId == id && x.CorporationId == user.CorporationId)
                .Select(x => new CxCContractorPaymentItemDto
                {
                    DatePayment = x.DatePayment,
                    PaymentMode = x.PaymentMode,
                    Reference = x.Reference,
                    Detail = x.Detail,
                    Payment = x.Payment,
                    Balance = x.Balance,
                    UsuarioOwner = x.UsuarioOwner
                });

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderByDescending(x => x.DatePayment)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<CxCContractorPaymentItemDto>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<CxCContractorPaymentItemDto>>(ex);
        }
    }

    //Una cuenta con lo que la compone y lo que se le ha pagado
    public async Task<ActionResponse<CxCContractor>> GetCxCContractorAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<CxCContractor>();

            //Solo la cabecera: las comisiones y los abonos se piden aparte y paginados
            var model = await _context.CxCContractors
                .AsNoTracking()
                .Include(x => x.Contractor)
                .FirstOrDefaultAsync(x => x.CxCContractorId == id && x.CorporationId == user.CorporationId);

            if (model == null)
                return Fail<CxCContractor>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            return new ActionResponse<CxCContractor> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<CxCContractor>(ex);
        }
    }

    //Le paga al contratista contra su cuenta: completo o por partes
    public async Task<ActionResponse<CxCContractor>> PayCxCContractorAsync(CxCContractorPaymentDto model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null || !user.CorporationId.HasValue)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<CxCContractor>();
            }

            var corporationId = user.CorporationId.Value;
            var note = await _context.CxCContractors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CxCContractorId == model.CxCContractorId &&
                                          x.CorporationId == corporationId);

            if (note == null)
                return await FailRollbackAsync<CxCContractor>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (note.Cancelled)
                return await FailRollbackAsync<CxCContractor>(_localizer["Contractor_NoteCancelled"]);

            if (note.Paid || note.Balance <= 0)
                return await FailRollbackAsync<CxCContractor>(_localizer["Contractor_NotePaid"]);

            var payment = Math.Round(model.Payment, 2);
            if (payment <= 0)
                return await FailRollbackAsync<CxCContractor>(_localizer["Contractor_PaymentInvalid"]);

            if (payment > note.Balance)
                return await FailRollbackAsync<CxCContractor>(_localizer["Contractor_PaymentOverBalance"]);

            if (!IsValidPaymentMode(model.PaymentMode))
                return await FailRollbackAsync<CxCContractor>(_localizer["Pay_ModeInvalid"]);

            var debt = note.Balance;
            var balance = debt - payment;
            var isPaid = balance == 0;
            var today = DateTime.UtcNow.Date;

            //Se reclama la cuenta con el saldo que se vio: si otro abono entro primero, no cuadra
            var claimed = await _context.CxCContractors
                .Where(x => x.CxCContractorId == note.CxCContractorId &&
                            !x.Paid &&
                            !x.Cancelled &&
                            x.Balance == debt)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Balance, balance)
                    .SetProperty(x => x.Paid, isPaid)
                    .SetProperty(x => x.DatePaid, isPaid ? today : (DateTime?)null));

            if (claimed == 0)
                return await FailRollbackAsync<CxCContractor>(_localizer["Contractor_NoteChanged"]);

            //Cuando queda saldada, sus comisiones quedan pagadas
            if (isPaid)
            {
                await _context.ContractorAccountPayables
                    .Where(x => x.CxCContractorId == note.CxCContractorId && !x.Paid)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.Paid, true)
                        .SetProperty(x => x.Balance, 0m)
                        .SetProperty(x => x.DatePaid, today));
            }

            var detail = new CxCContractorDetail
            {
                CxCContractorDetailId = Guid.NewGuid(),
                CxCContractorId = note.CxCContractorId,
                DatePayment = today,
                PaymentMode = model.PaymentMode,
                Reference = model.Reference?.Trim(),
                Detail = model.Detail?.Trim(),
                Debt = debt,
                Payment = payment,
                Balance = balance,
                CorporationId = corporationId,
                UsuarioOwner = $"{user.FirstName} {user.LastName}",
                UserId = Guid.Parse(user.Id)
            };

            _context.CxCContractorDetails.Add(detail);

            //Bitacora del dinero: cuanto se le entrego y como
            var request = _httpContextAccessor.HttpContext?.Request;
            PaymentAuditLog.Add(
                _context,
                corporationId,
                PaymentEventType.ContractorPaid,
                null,
                null,
                note.CxCContractorId,
                nameof(CxCContractor),
                payment,
                0,
                payment,
                $"{note.NoteNumber} - {model.PaymentMode} - saldo {balance:N2}",
                detail.UsuarioOwner,
                detail.UserId,
                request?.HttpContext.Connection.RemoteIpAddress?.ToString(),
                request?.Headers["User-Agent"].ToString());

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            //Lo que se devuelve refleja lo que quedo en la base
            note.Balance = balance;
            note.Paid = isPaid;
            note.DatePaid = isPaid ? today : null;

            return new ActionResponse<CxCContractor> { WasSuccess = true, Result = note };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<CxCContractor>(ex);
        }
    }

    //Anula la cuenta y devuelve sus comisiones a pendientes. Solo si no se le ha abonado.
    public async Task<ActionResponse<CxCContractor>> CancelCxCContractorAsync(Guid id, string motivo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null || !user.CorporationId.HasValue)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<CxCContractor>();
            }

            var corporationId = user.CorporationId.Value;
            var note = await _context.CxCContractors
                .FirstOrDefaultAsync(x => x.CxCContractorId == id && x.CorporationId == corporationId);

            if (note == null)
                return await FailRollbackAsync<CxCContractor>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            if (note.Cancelled)
                return await FailRollbackAsync<CxCContractor>(_localizer["Contractor_NoteCancelled"]);

            if (string.IsNullOrWhiteSpace(motivo))
                return await FailRollbackAsync<CxCContractor>(_localizer["Contractor_CancelReason"]);

            var tieneAbonos = await _context.CxCContractorDetails
                .AnyAsync(x => x.CxCContractorId == note.CxCContractorId);

            if (tieneAbonos)
                return await FailRollbackAsync<CxCContractor>(_localizer["Contractor_NoteWithPayments"]);

            //Las comisiones vuelven a quedar pendientes de agrupar
            await _context.ContractorAccountPayables
                .Where(x => x.CxCContractorId == note.CxCContractorId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.CxCContractorId, (Guid?)null));

            note.Cancelled = true;
            note.DateCancelled = DateTime.UtcNow.Date;
            note.DescriptionCancelled = motivo.Trim();
            note.Balance = 0;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<CxCContractor> { WasSuccess = true, Result = note };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<CxCContractor>(ex);
        }
    }

    //Los modos de pago que acepta la liquidacion
    private static bool IsValidPaymentMode(string? mode) =>
        mode is "Cash" or "Card" or "Transfer";

    public async Task<ActionResponse<IEnumerable<ContractorAccountPayable>>> GetAccountPayablesAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<ContractorAccountPayable>>();
            }

            var queryable = _context.ContractorAccountPayables.AsNoTracking()
                .Include(x => x.Contractor)
                .Include(x => x.ContractClient)
                .Include(x => x.CxCBill)
                .Where(x => x.CorporationId == user.CorporationId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.Contractor!.FirstName, $"%{filter}%") ||
                    EF.Functions.Like(x.Contractor.LastName, $"%{filter}%") ||
                    EF.Functions.Like(x.CxCBill!.CollectionNote!, $"%{filter}%") ||
                    EF.Functions.Like(x.ContractClient!.ControlContrato.ToString(), $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var result = await queryable
                .OrderBy(x => x.Paid)
                .ThenByDescending(x => x.DateCreated)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<ContractorAccountPayable>>
            {
                WasSuccess = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractorAccountPayable>>(ex);
        }
    }

    //La liquidacion de lo que se le debe al contratista. Es plata que sale: las cuentas se
    //reclaman de forma atomica para que dos usuarios no las paguen dos veces.
    public async Task<ActionResponse<ContractorPayment>> PayAsync(ContractorPaymentCreateDto model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null || !user.CorporationId.HasValue)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ContractorPayment>();
            }

            var corporationId = user.CorporationId.Value;
            var payableIds = model.ContractorAccountPayableIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (model.ContractorId == Guid.Empty || payableIds.Count == 0)
                return await FailRollbackAsync<ContractorPayment>(_localizer["Contractor_SelectPayables"]);

            var contractor = await _context.Contractors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractorId == model.ContractorId &&
                                          x.CorporationId == corporationId &&
                                          x.Active);

            if (contractor == null)
                return await FailRollbackAsync<ContractorPayment>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            //Solo las cuentas de ese contratista
            var accountPayables = await _context.ContractorAccountPayables
                .AsNoTracking()
                .Where(x => payableIds.Contains(x.ContractorAccountPayableId) &&
                            x.ContractorId == contractor.ContractorId &&
                            x.CorporationId == corporationId)
                .ToListAsync();

            if (accountPayables.Count != payableIds.Count)
                return await FailRollbackAsync<ContractorPayment>(_localizer["Contractor_PayablesMismatch"]);

            if (accountPayables.Any(x => x.Paid || x.Balance <= 0))
                return await FailRollbackAsync<ContractorPayment>(_localizer["Contractor_PayablesSettled"]);

            var total = accountPayables.Sum(x => x.Balance);
            var today = DateTime.UtcNow.Date;

            //Se reclaman las cuentas. Si otro usuario liquido alguna primero, no salen todas
            var claimed = await _context.ContractorAccountPayables
                .Where(x => payableIds.Contains(x.ContractorAccountPayableId) &&
                            x.ContractorId == contractor.ContractorId &&
                            x.CorporationId == corporationId &&
                            !x.Paid &&
                            x.Balance > 0)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Balance, 0m)
                    .SetProperty(x => x.Paid, true)
                    .SetProperty(x => x.DatePaid, today));

            if (claimed != accountPayables.Count)
                return await FailRollbackAsync<ContractorPayment>(_localizer["Contractor_PayablesSettled"]);

            //El consecutivo lo entrega la base, no la memoria
            var paymentNumber = $"PC-{await NumberSequence.NextAsync(_context, corporationId, NumberKind.ContractorPayment):0000000}";

            var contractorPayment = new ContractorPayment
            {
                ContractorPaymentId = Guid.NewGuid(),
                DatePayment = today,
                PaymentNumber = paymentNumber,
                ContractorId = contractor.ContractorId,
                PaymentMode = model.PaymentMode.Trim(),
                Reference = model.Reference?.Trim(),
                Detail = model.Detail?.Trim(),
                Total = total,
                CorporationId = corporationId,
                UsuarioOwner = $"{user.FirstName} {user.LastName}",
                UserId = Guid.Parse(user.Id),
                ContractorPaymentDetails = new List<ContractorPaymentDetail>()
            };

            foreach (var accountPayable in accountPayables)
            {
                contractorPayment.ContractorPaymentDetails.Add(new ContractorPaymentDetail
                {
                    ContractorPaymentDetailId = Guid.NewGuid(),
                    ContractorPaymentId = contractorPayment.ContractorPaymentId,
                    ContractorAccountPayableId = accountPayable.ContractorAccountPayableId,
                    Payment = accountPayable.Balance
                });
            }

            _context.ContractorPayments.Add(contractorPayment);

            //Bitacora del dinero: cuanto se le pago al contratista y con cuantas cuentas
            var request = _httpContextAccessor.HttpContext?.Request;
            PaymentAuditLog.Add(
                _context,
                corporationId,
                PaymentEventType.ContractorPaid,
                null,
                null,
                contractorPayment.ContractorPaymentId,
                nameof(ContractorPayment),
                total,
                0,
                total,
                $"{paymentNumber} - {contractor.FirstName} {contractor.LastName} - {accountPayables.Count} cuenta(s) - {contractorPayment.PaymentMode}",
                contractorPayment.UsuarioOwner,
                contractorPayment.UserId,
                request?.HttpContext.Connection.RemoteIpAddress?.ToString(),
                request?.Headers["User-Agent"].ToString());

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractorPayment>
            {
                WasSuccess = true,
                Result = contractorPayment
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractorPayment>(ex);
        }
    }

    //Deshace la transaccion y devuelve el motivo
    private async Task<ActionResponse<T>> FailRollbackAsync<T>(string message)
    {
        await _transactionManager.RollbackTransactionAsync();
        return new ActionResponse<T> { WasSuccess = false, Message = message };
    }

    private ActionResponse<T> AuthFail<T>()
    {
        return new ActionResponse<T>
        {
            WasSuccess = false,
            Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
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
