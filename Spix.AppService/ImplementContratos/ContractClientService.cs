using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos
{
    public class ContractClientService : IContractClientService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITransactionManager _transactionManager;
        private readonly IUserHelper _userHelper;
        private readonly IMapperService _mapperService;
        private readonly HttpErrorHandler _httpErrorHandler;
        private readonly IEnumMultilLanguageService _enumMultilLanguageService;
        private readonly IContractActivationIntegrityService _contractActivationIntegrityService;

        public ContractClientService(DataContext context, IHttpContextAccessor httpContextAccessor,
            ITransactionManager transactionManager, IUserHelper userHelper, IMapperService mapperService,
            HttpErrorHandler httpErrorHandler, IEnumMultilLanguageService enumMultilLanguageService,
            IContractActivationIntegrityService contractActivationIntegrityService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _transactionManager = transactionManager;
            _userHelper = userHelper;
            _mapperService = mapperService;
            _httpErrorHandler = httpErrorHandler;
            _enumMultilLanguageService = enumMultilLanguageService;
            _contractActivationIntegrityService = contractActivationIntegrityService;
        }
        public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatusAsync()
        {
            try
            {
                List<IntItemModel> list = _enumMultilLanguageService.GetEnumSelectList<ContractState>(nameof(Resource.Select_Status));
                int[] contractStateOrder =
                {
                    (int)ContractState.Draft,
                    (int)ContractState.PendingApproval,
                    (int)ContractState.InProgress,
                    (int)ContractState.Active,
                    (int)ContractState.Exempt,
                    (int)ContractState.Suspended,
                    (int)ContractState.Cancelled,
                    (int)ContractState.Terminated
                };

                list = list
                    .OrderBy(x => Array.IndexOf(contractStateOrder, x.Value))
                    .ToList();

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

        public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetContractClientComboStatusAsync()
        {
            try
            {
                int[] contractClientStates =
                {
                    (int)ContractState.Draft,
                    (int)ContractState.PendingApproval,
                    (int)ContractState.InProgress
                };

                List<IntItemModel> list = _enumMultilLanguageService
                    .GetEnumSelectList<ContractState>(nameof(Resource.Select_Status))
                    .Where(x => contractClientStates.Contains(x.Value))
                    .OrderBy(x => Array.IndexOf(contractClientStates, x.Value))
                    .ToList();

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
                    .Include(x => x.EstratoSocial)
                    .Include(c => c.ContractIDPic)
                    .Where(x => x.CorporationId == user.CorporationId)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(pagination.Filter))
                {
                    var filter = pagination.Filter.Trim();
                    queryable = queryable.Where(u =>
                        EF.Functions.Like(u.Client!.FirstName, $"%{filter}%") ||
                        EF.Functions.Like(u.Client!.LastName, $"%{filter}%") ||
                        EF.Functions.Like(u.Client!.FirstName + " " + u.Client!.LastName, $"%{filter}%") ||
                        EF.Functions.Like(u.Client.Document, $"%{filter}%"));
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
                var ZoneDetail = await _context.Zones.AsNoTracking().FirstOrDefaultAsync(x => x.ZoneId == modelo!.ZoneId);
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

        public async Task<ActionResponse<ContractClient>> UpdateAsync(ContractClient modelo)
        {
            await _transactionManager.BeginTransactionAsync();

            try
            {
                var currentContract = await _context.ContractClients
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.ContractClientId,
                        x.ContractState,
                        x.CorporationId
                    })
                    .FirstOrDefaultAsync(x => x.ContractClientId == modelo.ContractClientId);

                bool isChangingContractState = currentContract != null &&
                    currentContract.ContractState != modelo.ContractState;

                if (isChangingContractState &&
                    modelo.ContractState == ContractState.Active)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Result = modelo,
                        Message = "Para activar el contrato debe finalizar su configuracion desde ContractControl."
                    };
                }

                bool requiresMikrotikValidation = modelo.ContractState == ContractState.Suspended;

                if (isChangingContractState &&
                    requiresMikrotikValidation &&
                    currentContract is not null)
                {
                    var integrityResponse = await _contractActivationIntegrityService.ValidateAsync(
                        modelo.ContractClientId,
                        currentContract.CorporationId);
                    if (!integrityResponse.WasSuccess)
                    {
                        await _transactionManager.RollbackTransactionAsync();
                        return new ActionResponse<ContractClient>
                        {
                            WasSuccess = false,
                            Result = modelo,
                            Message = integrityResponse.Message
                        };
                    }
                }

                //Implementando el Mapeo de Modelos con Mapster
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



