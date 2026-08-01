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
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

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

        public ContractControlService(DataContext context, IHttpContextAccessor httpContextAccessor,
            ITransactionManager transactionManager, IUserHelper userHelper, IMapperService mapperService,
            HttpErrorHandler httpErrorHandler, IStringLocalizer localizer,
            IContractActivationIntegrityService contractActivationIntegrityService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _transactionManager = transactionManager;
            _userHelper = userHelper;
            _mapperService = mapperService;
            _httpErrorHandler = httpErrorHandler;
            _localizer = localizer;
            _contractActivationIntegrityService = contractActivationIntegrityService;
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
                    .Include(x => x.Client).ThenInclude(x => x!.DocumentType)
                    .Include(x => x.Contractor)
                    .Include(x => x.Zone).ThenInclude(x => x!.City)
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
                var modelo = await _context.ContractClients
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
