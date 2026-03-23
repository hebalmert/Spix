using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

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

        public ContractClientService(DataContext context, IHttpContextAccessor httpContextAccessor,
            ITransactionManager transactionManager, IUserHelper userHelper, IMapperService mapperService,
            HttpErrorHandler httpErrorHandler)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _transactionManager = transactionManager;
            _userHelper = userHelper;
            _mapperService = mapperService;
            _httpErrorHandler = httpErrorHandler;
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
                    .Include(x => x.Zone)
                    .Where(x => x.CorporationId == user.CorporationId &&
                                (x.StateType == StateType.Procesando || x.StateType == StateType.Activo))
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
                return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractClient>>(ex); // ✅ Manejo de errores automático
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
                    .Include(x => x.Zone)
                    .Where(x => x.CorporationId == user.CorporationId &&
                            (x.StateType == StateType.Creando || x.StateType == StateType.Procesando))
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
                return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractClient>>(ex); // ✅ Manejo de errores automático
            }
        }

        public async Task<ActionResponse<ContractClient>> GetAsync(Guid id)
        {
            try
            {
                var modelo = await _context.ContractClients
                    .Include(x => x.Client)
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
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex); // ✅ Manejo de errores automático
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
                if (modelo.StateType == StateType.Procesando)
                {
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Solo se puede Cambiar de Creando a Procesando"
                    };
                }
                if (modelo.StateType == StateType.Procesando)
                {
                    modelo.StateType = StateType.Creando;
                }
                else
                {
                    modelo.StateType = StateType.Procesando;
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
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex); // ✅ Manejo de errores automático
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
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex); // ✅ Manejo de errores automático
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
                var reg = await _context.Registers.FirstOrDefaultAsync(x => x.CorporationId == user.CorporationId);
                if (reg == null)
                {
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Problemas Para Asiganar el Consecutivo de Contrato"
                    };
                }
                reg.Contratos += 1;
                _context.Registers.Update(reg);

                modelo.ControlContrato = Convert.ToString(reg.Contratos);
                modelo.CorporationId = Convert.ToInt32(user.CorporationId);
                modelo.StateType = StateType.Creando;
                modelo.DateCreado = DateTime.Now;
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
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex); // ✅ Manejo de errores automático
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
                return await _httpErrorHandler.HandleErrorAsync<bool>(ex); // ✅ Manejo de errores automático
            }
        }
    }
}