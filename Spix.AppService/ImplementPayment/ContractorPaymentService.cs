using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesPayment;
using Spix.Domain.EntitiesPayment;
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
    }

    public async Task<ActionResponse<IEnumerable<ContractorAccountPayable>>> GetAccountPayablesAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<ContractorAccountPayable>>();
            }

            var queryable = _context.ContractorAccountPayables
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

    public async Task<ActionResponse<ContractorPayment>> PayAsync(ContractorPaymentCreateDto model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ContractorPayment>();
            }

            var payableIds = model.ContractorAccountPayableIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (model.ContractorId == Guid.Empty || payableIds.Count == 0)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ContractorPayment>("Debe seleccionar al menos una cuenta por pagar del contratista.");
            }

            var contractor = await _context.Contractors.FirstOrDefaultAsync(x =>
                x.ContractorId == model.ContractorId &&
                x.CorporationId == user.CorporationId &&
                x.Active);

            if (contractor == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ContractorPayment>(_localizer[nameof(Resource.Generic_IdNotFound)]);
            }

            var accountPayables = await _context.ContractorAccountPayables
                .Where(x => payableIds.Contains(x.ContractorAccountPayableId) &&
                            x.ContractorId == contractor.ContractorId &&
                            x.CorporationId == user.CorporationId)
                .ToListAsync();

            if (accountPayables.Count != payableIds.Count)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ContractorPayment>("Una o mas cuentas por pagar no pertenecen al contratista seleccionado.");
            }

            if (accountPayables.Any(x => x.Paid || x.Balance <= 0))
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<ContractorPayment>("Una o mas cuentas por pagar ya fueron liquidadas.");
            }

            var total = accountPayables.Sum(x => x.Balance);
            var contractorPayment = new ContractorPayment
            {
                ContractorPaymentId = Guid.NewGuid(),
                DatePayment = DateTime.UtcNow.Date,
                ContractorId = contractor.ContractorId,
                PaymentMode = model.PaymentMode.Trim(),
                Reference = model.Reference?.Trim(),
                Detail = model.Detail?.Trim(),
                Total = total,
                CorporationId = user.CorporationId!.Value,
                UsuarioOwner = $"{user.FirstName} {user.LastName}",
                UserId = Guid.Parse(user.Id),
                ContractorPaymentDetails = new List<ContractorPaymentDetail>()
            };

            foreach (var accountPayable in accountPayables)
            {
                var payment = accountPayable.Balance;
                accountPayable.Balance = 0;
                accountPayable.Paid = true;
                accountPayable.DatePaid = contractorPayment.DatePayment;

                contractorPayment.ContractorPaymentDetails.Add(new ContractorPaymentDetail
                {
                    ContractorPaymentDetailId = Guid.NewGuid(),
                    ContractorPaymentId = contractorPayment.ContractorPaymentId,
                    ContractorAccountPayableId = accountPayable.ContractorAccountPayableId,
                    Payment = payment
                });
            }

            _context.ContractorPayments.Add(contractorPayment);
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
