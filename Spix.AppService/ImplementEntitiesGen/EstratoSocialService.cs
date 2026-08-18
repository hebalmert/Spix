using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfacesEntitiesGen;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementEntitiesGen;

public class EstratoSocialService : IEstratoSocialService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public EstratoSocialService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager,
        IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<GuidItemModel>>> ComboAsync(string username)
    {
        try
        {
            User? user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<GuidItemModel>>();
            }

            int corporationId = Convert.ToInt32(user.CorporationId);
            await EnsureDefaultEstratosSocialesAsync(corporationId);

            List<GuidItemModel> list = await _context.EstratosSociales.AsNoTracking()
                .Where(x => x.CorporationId == corporationId)
                .OrderBy(x => x.EstratoSocialName)
                .Select(x => new GuidItemModel
                {
                    Value = x.EstratoSocialId,
                    Name = x.EstratoSocialName
                })
                .ToListAsync();

            list.Insert(0, new GuidItemModel
            {
                Value = Guid.Empty,
                Name = $"[{_localizer[nameof(Resource.Select_EstratoSocial)]}]"
            });

            return new ActionResponse<IEnumerable<GuidItemModel>>
            {
                WasSuccess = true,
                Result = list
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<GuidItemModel>>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<EstratoSocial>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            User? user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return AuthFail<IEnumerable<EstratoSocial>>();
            }

            int corporationId = Convert.ToInt32(user.CorporationId);
            await EnsureDefaultEstratosSocialesAsync(corporationId);

            IQueryable<EstratoSocial> queryable = _context.EstratosSociales
                .Where(x => x.CorporationId == corporationId);

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                string filter = pagination.Filter.Trim();
                queryable = queryable.Where(x => EF.Functions.Like(x.EstratoSocialName, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            List<EstratoSocial> modelo = await queryable
                .OrderBy(x => x.EstratoSocialName)
                .Paginate(pagination)
                .ToListAsync();

            return new ActionResponse<IEnumerable<EstratoSocial>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<EstratoSocial>>(ex);
        }
    }

    public async Task<ActionResponse<EstratoSocial>> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return Fail<EstratoSocial>(nameof(Resource.Generic_InvalidId));
        }

        try
        {
            EstratoSocial? modelo = await _context.EstratosSociales.FindAsync(id);
            if (modelo == null)
            {
                return Fail<EstratoSocial>(nameof(Resource.Generic_IdNotFound));
            }

            return new ActionResponse<EstratoSocial>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<EstratoSocial>(ex);
        }
    }

    public async Task<ActionResponse<EstratoSocial>> UpdateAsync(EstratoSocial modelo)
    {
        if (modelo == null || modelo.EstratoSocialId == Guid.Empty)
        {
            return Fail<EstratoSocial>(nameof(Resource.Generic_InvalidId));
        }

        await _transactionManager.BeginTransactionAsync();

        try
        {
            _context.EstratosSociales.Update(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<EstratoSocial>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<EstratoSocial>(ex);
        }
    }

    public async Task<ActionResponse<EstratoSocial>> AddAsync(EstratoSocial modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out _))
        {
            return Fail<EstratoSocial>(nameof(Resource.Generic_InvalidModel));
        }

        await _transactionManager.BeginTransactionAsync();

        try
        {
            User? user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<EstratoSocial>();
            }

            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            _context.EstratosSociales.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<EstratoSocial>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<EstratoSocial>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            EstratoSocial? dataRemove = await _context.EstratosSociales.FindAsync(id);
            if (dataRemove == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return Fail<bool>(nameof(Resource.Generic_IdNotFound));
            }

            _context.EstratosSociales.Remove(dataRemove);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    private async Task EnsureDefaultEstratosSocialesAsync(int corporationId)
    {
        bool hasEstratosSociales = await _context.EstratosSociales.AnyAsync(x => x.CorporationId == corporationId);
        if (hasEstratosSociales)
        {
            return;
        }

        List<EstratoSocial> estratosSociales =
        [
            new EstratoSocial { EstratoSocialName = "Estrato 1 - Bajo-Bajo", ApplyTax = false, CorporationId = corporationId },
            new EstratoSocial { EstratoSocialName = "Estrato 2 - Bajo", ApplyTax = false, CorporationId = corporationId },
            new EstratoSocial { EstratoSocialName = "Estrato 3 - Medio-Bajo", ApplyTax = false, CorporationId = corporationId },
            new EstratoSocial { EstratoSocialName = "Estrato 4 - Medio", ApplyTax = true, CorporationId = corporationId },
            new EstratoSocial { EstratoSocialName = "Estrato 5 - Medio-Alto", ApplyTax = true, CorporationId = corporationId },
            new EstratoSocial { EstratoSocialName = "Estrato 6 - Alto", ApplyTax = true, CorporationId = corporationId }
        ];

        _context.EstratosSociales.AddRange(estratosSociales);
        await _context.SaveChangesAsync();
    }

    private ActionResponse<T> AuthFail<T>()
    {
        return Fail<T>(nameof(Resource.Generic_AuthIdFail));
    }

    private ActionResponse<T> Fail<T>(string resourceName)
    {
        return new ActionResponse<T>
        {
            WasSuccess = false,
            Message = _localizer[resourceName]
        };
    }
}
