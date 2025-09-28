using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Entities;
using Spix.Domain.Enum;
using Spix.Domain.Resources;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesSecure;

namespace Spix.Services.ImplementSecure;

public class UsuarioRoleService : IUsuarioRoleService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public UsuarioRoleService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper,
        HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync()
    {
        try
        {
            List<IntItemModel> list = Enum.GetValues(typeof(UserTypeDTO)).Cast<UserTypeDTO>().Select(c => new IntItemModel()
            {
                Name = c.ToString(),
                Value = (int)c
            }).ToList();

            list.Insert(0, new IntItemModel
            {
                Name = _localizer[nameof(Resource.Select_B_Role)],
                Value = 0
            });

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

    public async Task<ActionResponse<IEnumerable<UsuarioRole>>> GetAsync(PaginationDTO pagination)
    {
        try
        {
            var queryable = _context.UsuarioRoles.Where(x => x.UsuarioId == pagination.Id).AsQueryable();

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<UsuarioRole>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<UsuarioRole>>(ex);
        }
    }

    public async Task<ActionResponse<UsuarioRole>> GetAsync(int id)
    {
        try
        {
            var modelo = await _context.UsuarioRoles.FirstOrDefaultAsync(x => x.UsuarioRoleId == id);
            if (modelo == null)
            {
                return new ActionResponse<UsuarioRole>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            return new ActionResponse<UsuarioRole>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<UsuarioRole>(ex);
        }
    }

    public async Task<ActionResponse<UsuarioRole>> AddAsync(UsuarioRole modelo, string username)
    {
        User userAsp = await _userHelper.GetUserByUserNameAsync(username);
        if (userAsp == null)
        {
            return new ActionResponse<UsuarioRole>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var CurrentUser = await _context.Usuarios.FindAsync(modelo.UsuarioId);
            var UserSystem = await _userHelper.GetUserByUserNameAsync(CurrentUser!.UserName);

            modelo.CorporationId = Convert.ToInt32(userAsp.CorporationId);
            _context.UsuarioRoles.Add(modelo);
            await _transactionManager.SaveChangesAsync();

            UserRoleDetails newUserRoleDetail = new()
            {
                UserId = UserSystem.Id,
                UserType = modelo.UserType
            };
            _context.UserRoleDetails.Add(newUserRoleDetail);
            await _userHelper.AddUserToRoleAsync(userAsp, modelo.UserType.ToString());
            await _userHelper.AddUserClaims(modelo.UserType, userAsp.UserName!);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<UsuarioRole>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<UsuarioRole>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.UsuarioRoles.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }
            _context.UsuarioRoles.Remove(DataRemove);

            var usuario = await _context.Usuarios.FindAsync(DataRemove.UsuarioId);
            var userAsp = await _userHelper.GetUserByUserNameAsync(usuario!.UserName);
            var registro = await _context.UserRoleDetails.Where(c => c.UserId == userAsp.Id && c.UserType == DataRemove.UserType).FirstOrDefaultAsync();
            await _userHelper.RemoveUserToRoleAsync(userAsp, DataRemove.UserType.ToString());
            await _userHelper.RemoveUserClaims(DataRemove.UserType, userAsp.UserName!);

            _context.UserRoleDetails.Remove(registro!);
            await _transactionManager.SaveChangesAsync();

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Message = _localizer[nameof(Resource.Generic_Success)]
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }
}