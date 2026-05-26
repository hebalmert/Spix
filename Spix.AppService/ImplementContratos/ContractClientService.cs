using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.Services.ImplementContratos
{
    public class ContractClientService : IContractClientService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITransactionManager _transactionManager;
        private readonly IUserHelper _userHelper;
        private readonly IMapperService _mapperService;
        private readonly HttpErrorHandler _httpErrorHandler;
        private readonly IStringLocalizer _localizer;

        public ContractClientService(DataContext context, IHttpContextAccessor httpContextAccessor,
            ITransactionManager transactionManager, IUserHelper userHelper, IMapperService mapperService,
            HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _transactionManager = transactionManager;
            _userHelper = userHelper;
            _mapperService = mapperService;
            _httpErrorHandler = httpErrorHandler;
            _localizer = localizer;
        }
        public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatusAsync()
        {
            try
            {
                List<IntItemModel> list = Enum.GetValues(typeof(ContractState)).Cast<ContractState>().Select(c => new IntItemModel()
                {
                    Name = c.ToLocalizedString(_localizer),
                    Value = (int)c
                }).ToList();

                list.Insert(0, new IntItemModel() 
                { 
                    Name = _localizer[nameof(Resource.Select_Status)], 
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
                return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex); // ✅ Manejo de errores automático
            }
        }

        public async Task<ActionResponse<IEnumerable<ContractClient>>> GetControlContratos(PaginationDTO pagination, string username)
        {
            try
            {
                var user = await _userHelper.GetUserByUserNameAsync(username);
                if (user == null)
                {
                    return new ActionResponse<IEnumerable<ContractClient>>
                    {
                        WasSuccess = false,
                        Message = "Problemas de Validacion de Usuario"
                    };
                }

                var queryable = _context.ContractClients
                    .Include(x => x.Client)
                    .Include(x => x.Contractor)
                    .Include(x => x.Zone)
                    .Include(c => c.ContractIDPic)
                    .Where(x => x.CorporationId == user.CorporationId &&
                                (x.ContractState == ContractState.Active || x.ContractState == ContractState.Exempt || x.ContractState == ContractState.Suspended))
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(pagination.Filter))
                {
                    var filter = pagination.Filter.Trim();
                    queryable = queryable.Where(u =>
                        EF.Functions.Like(u.Client!.FirstName, $"%{filter}%") ||
                        EF.Functions.Like(u.Client!.LastName, $"%{filter}%") ||
                        EF.Functions.Like(u.Client!.FirstName + " " + u.Client!.LastName, $"%{filter}%"));
                }

                await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
                var modelo = await queryable.Paginate(pagination).ToListAsync();

                return new ActionResponse<IEnumerable<ContractClient>>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractClient>>(ex);
            }
        }

        public async Task<ActionResponse<IEnumerable<ContractClient>>> GetAsync(PaginationDTO pagination, string username)
        {
            try
            {
                var user = await _userHelper.GetUserByUserNameAsync(username);
                if (user == null)
                {
                    return new ActionResponse<IEnumerable<ContractClient>>
                    {
                        WasSuccess = false,
                        Message = "Problemas de Validacion de Usuario"
                    };
                }

                var queryable = _context.ContractClients
                    .Include(x => x.Client)
                    .Include(x => x.Contractor)
                    .Include(x => x.Zone)
                    .Include(c => c.ContractIDPic)
                    .Where(x => x.CorporationId == user.CorporationId)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(pagination.Filter))
                {
                    var filter = pagination.Filter.Trim();
                    queryable = queryable.Where(u =>
                        EF.Functions.Like(u.Client!.FirstName, $"%{filter}%") ||
                        EF.Functions.Like(u.Client!.LastName, $"%{filter}%") ||
                        EF.Functions.Like(u.Client!.FirstName + " " + u.Client!.LastName, $"%{filter}%"));
                }

                await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
                var modelo = await queryable.OrderByDescending(x=> x.ControlContrato).Paginate(pagination).ToListAsync();

                return new ActionResponse<IEnumerable<ContractClient>>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractClient>>(ex);
            }
        }

        public async Task<ActionResponse<ContractClient>> GetAsync(Guid id)
        {
            try
            {
                var modelo = await _context.ContractClients
                    .Include(x => x.Client)
                    .Include(x=> x.Contractor)
                    .Include(c => c.ContractIDPic)
                    .FirstOrDefaultAsync(x => x.ContractClientId == id);
                var ZoneDetail = await _context.Zones.FirstOrDefaultAsync(x => x.ZoneId == modelo!.ZoneId);
                modelo!.StateId = ZoneDetail!.StateId;
                modelo.CityId = ZoneDetail.CityId;
                if (modelo == null)
                {
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Problemas para Enconstrar el Registro Indicado"
                    };
                }

                return new ActionResponse<ContractClient>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex);
            }
        }

        public async Task<ActionResponse<ContractClient>> GetProcesandoAsync(Guid id)
        {
            try
            {
                await _transactionManager.BeginTransactionAsync();
                var modelo = await _context.ContractClients
                    .FirstOrDefaultAsync(x => x.ContractClientId == id);
                if (modelo == null)
                {
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Problemas para Encontrar el Registro Indicado"
                    };
                }
                if (modelo.ContractState == ContractState.PendingApproval)
                {
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Solo se puede Cambiar de Creando a Procesando"
                    };
                }
                if (modelo.ContractState == ContractState.PendingApproval)
                {
                    modelo.ContractState = ContractState.Draft;
                }
                else
                {
                    modelo.ContractState = ContractState.Draft;
                }
                _context.ContractClients.Update(modelo);

                await _transactionManager.SaveChangesAsync();
                await _transactionManager.CommitTransactionAsync();

                return new ActionResponse<ContractClient>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex);
            }
        }

        public async Task<ActionResponse<ContractClient>> UpdateAsync(ContractClient modelo)
        {
            await _transactionManager.BeginTransactionAsync();

            try
            {
                //Implementando el Mapeo de Modelos con Mapster
                ContractClient NuevoModelo = _mapperService.Map<ContractClient, ContractClient>(modelo);

                _context.ContractClients.Update(NuevoModelo);

                await _transactionManager.SaveChangesAsync();
                await _transactionManager.CommitTransactionAsync();

                return new ActionResponse<ContractClient>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                await _transactionManager.RollbackTransactionAsync();
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex);
            }
        }

        public async Task<ActionResponse<ContractClient>> AddAsync(ContractClient modelo, string username)
        {
            await _transactionManager.BeginTransactionAsync();
            try
            {
                var user = await _userHelper.GetUserByUserNameAsync(username);
                if (user == null)
                {
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Problemas de Validacion de Usuario"
                    };
                }

                //Para crear el correlativo de Contratos
                var lastNumber = await _context.ContractClients.AsNoTracking()
                    .Where(x => x.CorporationId == user.CorporationId)
                                    .MaxAsync(x => (long?)x.ControlContrato) ?? 0;
                modelo.ControlContrato = lastNumber + 1;

                modelo.CorporationId = Convert.ToInt32(user.CorporationId);
                modelo.ContractState = ContractState.Draft;
                modelo.DateCreado = DateTime.Now;
                //control de Auditoria
                modelo.UsuarioOwner = $"{user.FirstName!} {user.LastName!}";
                modelo.UserId = Guid.Parse(user.Id);

                _context.ContractClients.Add(modelo);
                await _transactionManager.SaveChangesAsync();
                await _transactionManager.CommitTransactionAsync();

                return new ActionResponse<ContractClient>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                await _transactionManager.RollbackTransactionAsync();
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex);
            }
        }

        public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
        {
            await _transactionManager.BeginTransactionAsync();
            try
            {
                var DataRemove = await _context.ContractClients.FindAsync(id);
                if (DataRemove == null)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "Problemas para Enconstrar el Registro Indicado"
                    };
                }
                _context.ContractClients.Remove(DataRemove);

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
    }
}