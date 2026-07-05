using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesPayment;
using Spix.Domain.EntitiesPayment;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementPayment;

public class PaymentService : IPaymentService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public PaymentService(DataContext context, IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
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

    private ActionResponse<T> AuthFail<T>() => new()
    {
        WasSuccess = false,
        Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
    };
}
