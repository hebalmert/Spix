using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.ImplementContratos;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.Services.ImplementContratos
{
    public class ContractControlService : IContractControlService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITransactionManager _transactionManager;
        private readonly IUserHelper _userHelper;
        private readonly IMapperService _mapperService;
        private readonly HttpErrorHandler _httpErrorHandler;
        private readonly IStringLocalizer _localizer;
        private readonly IContractActivationIntegrityService _contractActivationIntegrityService;
        private readonly IEnumMultilLanguageService _enumMultilLanguageService;

        public ContractControlService(DataContext context, IHttpContextAccessor httpContextAccessor,
            ITransactionManager transactionManager, IUserHelper userHelper, IMapperService mapperService,
            HttpErrorHandler httpErrorHandler, IStringLocalizer localizer,
            IContractActivationIntegrityService contractActivationIntegrityService,
            IEnumMultilLanguageService enumMultilLanguageService)
        {
            _enumMultilLanguageService = enumMultilLanguageService;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _transactionManager = transactionManager;
            _userHelper = userHelper;
            _mapperService = mapperService;
            _httpErrorHandler = httpErrorHandler;
            _localizer = localizer;
            _contractActivationIntegrityService = contractActivationIntegrityService;
        }


        //Estados a los que puede pasar ESTE contrato, listos para pintar en el combo:
        //el neutro traducido en la posicion 0 y solo los destinos validos desde su estado actual.
        public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetStateChangeOptionsAsync(Guid contractClientId, string username)
        {
            try
            {
                var user = await _userHelper.GetUserByUserNameAsync(username);
                if (user == null)
                {
                    return new ActionResponse<IEnumerable<IntItemModel>> { WasSuccess = false, Message = _localizer[nameof(Resource.Generic_AuthIdFail)] };
                }

                var contract = await _context.ContractClients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId && x.CorporationId == user.CorporationId);

                if (contract == null)
                {
                    return new ActionResponse<IEnumerable<IntItemModel>> { WasSuccess = false, Message = _localizer[nameof(Resource.Generic_IdNotFound)] };
                }

                var permitidos = ContractStateRules.GetAllowedStates(contract.ContractState);

                var list = new List<IntItemModel>
                {
                    new() { Value = 0, Name = _localizer["Select_NewStatus"] }
                };

                list.AddRange(permitidos.Select(estado => new IntItemModel
                {
                    Value = (int)estado,
                    Name = _enumMultilLanguageService.GetLocalizedName(estado)
                }));

                return new ActionResponse<IEnumerable<IntItemModel>> { WasSuccess = true, Result = list };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
            }
        }

        //Cambia el estado del contrato. Valida la transicion y el estado del MikroTik:
        //aqui es donde se evita que un contrato quede suspendido pero con servicio.
        public async Task<ActionResponse<ContractClient>> ChangeStateAsync(Guid contractClientId, int newState, string? motivo, string username)
        {
            await _transactionManager.BeginTransactionAsync();
            try
            {
                var user = await _userHelper.GetUserByUserNameAsync(username);
                if (user == null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient> { WasSuccess = false, Message = _localizer[nameof(Resource.Generic_AuthIdFail)] };
                }

                var contract = await _context.ContractClients
                    .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId && x.CorporationId == user.CorporationId);

                if (contract == null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient> { WasSuccess = false, Message = _localizer[nameof(Resource.Generic_IdNotFound)] };
                }

                if (!Enum.IsDefined(typeof(ContractState), newState))
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient> { WasSuccess = false, Message = _localizer["Validation_SelectStatus"] };
                }

                var destino = (ContractState)newState;

                //La transicion tiene que ser una de las permitidas desde el estado actual
                if (!ContractStateRules.GetAllowedStates(contract.ContractState).Contains(destino))
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient> { WasSuccess = false, Message = _localizer["ContractState_NoTransitions"] };
                }

                //Y el MikroTik tiene que estar como corresponde para ese destino
                var bloqueo = await ContractStateRules.GetBlockingReasonAsync(_context, contractClientId, destino);
                if (bloqueo != null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient> { WasSuccess = false, Message = _localizer[bloqueo] };
                }

                //El MikroTik se toca ANTES de asentar nada: si falla, no se cambia el estado.
                //Se reusan las mismas piezas del corte masivo y del modulo de suspendidos.
                bool usaHotSpot = await _contractActivationIntegrityService.UsesHotSpotControlAsync(contract.CorporationId);

                if (usaHotSpot && destino == ContractState.Suspended)
                {
                    var suspension = await _contractActivationIntegrityService.SuspendHotSpotBindingsAsync(contract);
                    if (!suspension.WasSuccess)
                    {
                        await _transactionManager.RollbackTransactionAsync();
                        return new ActionResponse<ContractClient> { WasSuccess = false, Message = suspension.Message };
                    }
                }

                if (usaHotSpot && destino == ContractState.Active)
                {
                    var activacion = await _contractActivationIntegrityService.ActivateHotSpotBindingsAsync(contract);
                    if (!activacion.WasSuccess)
                    {
                        await _transactionManager.RollbackTransactionAsync();
                        return new ActionResponse<ContractClient> { WasSuccess = false, Message = activacion.Message };
                    }
                }

                contract.ContractState = destino;

                //Registro de la suspension: se abre al suspender y se cierra al salir de suspendido
                var userId = Guid.TryParse(user.Id, out var id) ? id : (Guid?)null;
                var userName = $"{user.FirstName} {user.LastName}".Trim();

                if (destino == ContractState.Suspended)
                {
                    await ContractSuspendedRegistry.OpenAsync(_context, contract, SuspendedOrigin.Manual,
                        motivo, null, userName, userId);
                }
                else
                {
                    await ContractSuspendedRegistry.CloseAsync(_context, contract.ContractClientId, userName, userId);
                }

                await _transactionManager.SaveChangesAsync();
                await _transactionManager.CommitTransactionAsync();

                return new ActionResponse<ContractClient> { WasSuccess = true, Result = contract };
            }
            catch (Exception ex)
            {
                await _transactionManager.RollbackTransactionAsync();
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex);
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

                var queryable = _context.ContractClients.AsNoTracking()
                    .Include(x => x.Client).ThenInclude(x => x!.DocumentType)
                    .Include(x => x.Contractor)
                    .Include(x => x.Zone).ThenInclude(x => x!.City)
                    .Include(x => x.EstratoSocial)
                    .Where(x => x.CorporationId == user.CorporationId &&
                                (x.ContractState == ContractState.InProgress ||
                                 x.ContractState == ContractState.Active ||
                                 x.ContractState == ContractState.Exempt ||
                                 x.ContractState == ContractState.Suspended))
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

                //Filtro por estado desde el dropdown del listado (pagination.Id = ContractState; 0 = todos).
                //Solo puede acotar dentro de los estados que ya muestra esta pantalla.
                if (pagination.Id > 0 && Enum.IsDefined(typeof(ContractState), pagination.Id))
                {
                    var state = (ContractState)pagination.Id;
                    queryable = queryable.Where(x => x.ContractState == state);
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

        public async Task<ActionResponse<ContractClient>> GetAsync(Guid id)
        {
            try
            {
                var modelo = await _context.ContractClients.AsNoTracking()
                    .Include(x => x.Client).ThenInclude(x => x!.DocumentType)
                    .Include(x => x.Contractor)
                    .Include(x => x.Zone).ThenInclude(x => x!.City)
                    .Include(x=> x.ContractIps)
                    .Include(x => x.ContractMacs)
                    .Include(x => x.ContractServers)
                    .Include(x => x.ContractPlans)
                    .Include(x => x.ContractNodes)
                    .Include(x => x.ContractMaps)
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

        public async Task<ActionResponse<ContractClient>> ActivateAsync(Guid id, string username)
        {
            await _transactionManager.BeginTransactionAsync();

            try
            {
                var user = await _userHelper.GetUserByUserNameAsync(username);
                if (user == null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Problemas de Validacion de Usuario"
                    };
                }

                var contract = await _context.ContractClients
                    .Include(x => x.Client)
                    .FirstOrDefaultAsync(x => x.ContractClientId == id &&
                                              x.CorporationId == user.CorporationId);
                if (contract == null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Problemas para Encontrar el Registro Indicado"
                    };
                }

                if (contract.ContractState != ContractState.InProgress)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "El contrato debe estar en InProgress para poder activarse."
                    };
                }

                var integrityResponse = await _contractActivationIntegrityService.ValidateAsync(
                    contract.ContractClientId,
                    contract.CorporationId);
                if (!integrityResponse.WasSuccess)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = integrityResponse.Message
                    };
                }

                bool usesHotSpotControl = await _contractActivationIntegrityService
                    .UsesHotSpotControlAsync(contract.CorporationId);
                if (usesHotSpotControl)
                {
                    var activationResponse = await _contractActivationIntegrityService
                        .ActivateHotSpotBindingsAsync(contract);
                    if (!activationResponse.WasSuccess)
                    {
                        await _transactionManager.RollbackTransactionAsync();
                        return new ActionResponse<ContractClient>
                        {
                            WasSuccess = false,
                            Message = activationResponse.Message
                        };
                    }
                }

                contract.ContractState = ContractState.Active;
                await _transactionManager.SaveChangesAsync();
                await _transactionManager.CommitTransactionAsync();

                return new ActionResponse<ContractClient>
                {
                    WasSuccess = true,
                    Result = contract
                };
            }
            catch (Exception ex)
            {
                await _transactionManager.RollbackTransactionAsync();
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex);
            }
        }
    }
}
