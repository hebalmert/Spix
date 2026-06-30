using Spix.AppService.ImplementContratos;
using Spix.AppService.ImplementEntitiesData;
using Spix.AppService.ImplementEntitiesGen;
using Spix.AppService.ImplementEntitiesNet;
using Spix.AppService.ImplementMk;
using Spix.AppService.ImplementSchedule;
using Spix.AppService.InterfaceContratos;
using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.AppService.InterfaceEntities;
using Spix.AppService.InterfaceEntitiesNet;
using Spix.AppService.InterfaceSchedule;
using Spix.AppService.InterfacesEntitiesData;
using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppService.InterfacesInven;
using Spix.AppService.InterfacesMk;
using Spix.AppService.InterfacesOper;
using Spix.AppService.InterfacesSecure;
using Spix.AppServiceX.ImplementContratos;
using Spix.AppServiceX.ImplementContratos.ImplementContractControl;
using Spix.AppServiceX.ImplementEntitiesGen;
using Spix.AppServiceX.ImplementEntitiesNet;
using Spix.AppServiceX.ImplementInven;
using Spix.AppServiceX.ImplementMk;
using Spix.AppServiceX.ImplementSchedule;
using Spix.AppServiceX.InterfaceContratos;
using Spix.AppServiceX.InterfaceContratos.InterfaceContractControl;
using Spix.AppServiceX.InterfaceEntities;
using Spix.AppServiceX.InterfaceEntitiesNet;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.AppServiceX.InterfacesEntitiesData;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesInven;
using Spix.AppServiceX.InterfacesMk;
using Spix.AppServiceX.InterfacesOper;
using Spix.AppServiceX.InterfacesSecure;
using Spix.Services.ImplementContratos;
using Spix.Services.ImplementEntties;
using Spix.Services.ImplementInven;
using Spix.Services.ImplementOper;
using Spix.Services.ImplementSecure;
using Spix.ServiceX.ImplementSecure;
using Spix.UnitOfWork.ImplementEntities;
using Spix.UnitOfWork.ImplementEntitiesData;
using Spix.UnitOfWork.ImplementOper;
using Spix.UnitOfWork.ImplementSecure;

namespace Spix.AppBack.DependencyInjection
{
    public class UnitOfWorkRegistration
    {
        public static void AddUnitOfWorkRegistration(IServiceCollection services)
        {
            //EntitiesSecurities Software
            services.AddScoped<IAccountServiceX, AccountServiceX>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUsuarioServiceX, UsuarioServiceX>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IUsuarioRoleServiceX, UsuarioRoleServiceX>();
            services.AddScoped<IUsuarioRoleService, UsuarioRoleService>();

            //Entities
            services.AddScoped<ICountryServiceX, CountryServiceX>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IStateServiceX, StateServiceX>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<ICityServiceX, CityServiceX>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ICorporationServiceX, CorporationServiceX>();
            services.AddScoped<ICorporationService, CorporationService>();
            services.AddScoped<IManagerServiceX, ManagerServiceX>();
            services.AddScoped<IManagerService, ManagerService>();
            services.AddScoped<ISoftPlanServiceX, SoftPlanServiceX>();
            services.AddScoped<ISoftPlanService, SoftPlanService>();

            //EntitiesData
            services.AddScoped<IChainTypesServiceX, ChainTypesServiceX>();
            services.AddScoped<IChainTypesService, ChainTypesService>();
            services.AddScoped<IChannelServiceX, ChannelServiceX>();
            services.AddScoped<IChannelService, ChannelService>();
            services.AddScoped<IFrecuencyServiceX, FrecuencyServiceX>();
            services.AddScoped<IFrecuencyService, FrecuencyService>();
            services.AddScoped<IFrecuencyTypeServiceX, FrecuencyTypeServiceX>();
            services.AddScoped<IFrecuencyTypeService, FrecuencyTypeService>();
            services.AddScoped<IHotSpotTypeServiceX, HotSpotTypeServiceX>();
            services.AddScoped<IHotSpotTypeService, HotSpotTypeService>();
            services.AddScoped<IOperationServiceX, OperationServiceX>();
            services.AddScoped<IOperationService, OperationService>();
            services.AddScoped<ISecurityServiceX, SecurityServiceX>();
            services.AddScoped<ISecurityService, SecurityService>();


            //EntitiesGen
            services.AddScoped<IDocumentTypeServiceX, DocumentTypeServiceX>();
            services.AddScoped<IDocumentTypeService, DocumentTypeService>();
            services.AddScoped<IMarkModelServiceX, MarkModelServiceX>();
            services.AddScoped<IMarkModelService, MarkModelService>();
            services.AddScoped<IMarkServiceX, MarkServiceX>();
            services.AddScoped<IMarkService, MarkService>();
            services.AddScoped<IPlanCategoryServiceX, PlanCategoryServiceX>();
            services.AddScoped<IPlanCategoryService, PlanCategoryService>();
            services.AddScoped<IPlanServiceX, PlanServiceX>();
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<IProductCategoryServiceX, ProductCategoryServiceX>();
            services.AddScoped<IProductCategoryService, ProductCategoryService>();
            services.AddScoped<IProductServiceX, ProductServiceX>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IRegisterServiceX, RegisterServiceX>();
            services.AddScoped<IRegisterService, RegisterService>();
            services.AddScoped<IServiceCategoryServiceX, ServiceCategoryServiceX>();
            services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
            services.AddScoped<IServiceClientServiceX, ServiceClientServiceX>();
            services.AddScoped<IServiceClientService, ServiceClientService>();
            services.AddScoped<ITaxServiceX, TaxServiceX>();
            services.AddScoped<ITaxService, TaxService>();
            services.AddScoped<IZoneServiceX, ZoneServiceX>();
            services.AddScoped<IZoneService, ZoneService>();

            //EntitiesInven
            services.AddScoped<IProductStockServiceX, ProductStockServiceX>();
            services.AddScoped<IProductStockService, ProductStockService>();
            services.AddScoped<IProductStorageServiceX, ProductStorageServiceX>();
            services.AddScoped<IProductStorageService, ProductStorageService>();
            services.AddScoped<ISupplierServiceX, SupplierServiceX>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<IPurchaseServiceX, PurchaseServiceX>();
            services.AddScoped<IPurchaseService, PurchaseService>();
            services.AddScoped<IPurchaseDetailsServiceX, PurchaseDetailsServiceX>();
            services.AddScoped<IPurchaseDetailsService, PurchaseDetailsService>();
            services.AddScoped<ICargueServiceX, CargueServiceX>();
            services.AddScoped<ICargueService, CargueService>();
            services.AddScoped<ICargueDetailsServiceX, CargueDetailsServiceX>();
            services.AddScoped<ICargueDetailsService, CargueDetailsService>();

            //EntitiesNet
            services.AddScoped<IIpNetServiceX, IpNetServiceX>();
            services.AddScoped<IIpNetService, IpNetService>();
            services.AddScoped<IIpNetworkServiceX, IpNetworkServiceX>();
            services.AddScoped<IIpNetworkService, IpNetworkService>();
            services.AddScoped<INodeServiceX, NodeServiceX>();
            services.AddScoped<INodeService, NodeService>();
            services.AddScoped<IServerServiceX, ServerServiceX>();
            services.AddScoped<IServerService, ServerService>();

            //MikrotikServices
            services.AddScoped<IMkConnectionServiceX, MkConnectionServiceX>();
            services.AddScoped<IMkConnectionService, MkConnectionService>();

            //Operaciones
            services.AddScoped<IClientServiceX, ClientServiceX>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IContractorServiceX, ContractorServiceX>();
            services.AddScoped<IContractorService, ContractorService>();
            services.AddScoped<ITechnitianServiceX, TechnitianServiceX>();
            services.AddScoped<ITechnitianService, TechnitianService>();

            //Contratos
            services.AddScoped<IContractClientServiceX, ContractClientServiceX>();
            services.AddScoped<IContractClientService, ContractClientService>();
            services.AddScoped<IContractControlServiceX, ContractControlServiceX>();
            services.AddScoped<IContractControlService, ContractControlService>();
            services.AddScoped<IContractIDPicServiceX, ContractIDPicServiceX>();
            services.AddScoped<IContractIDPicService, ContractIDPicService>();

            //ContratosContractControl
            services.AddScoped<IContractIpServiceX, ContractIpServiceX>();
            services.AddScoped<IContractIpService, ContractIpService>();
            services.AddScoped<IContractMacServiceX, ContractMacServiceX>();
            services.AddScoped<IContractMacService, ContractMacService>();

            //Schedule
            services.AddScoped<IScheduleServiceX, ScheduleServiceX>();
            services.AddScoped<IScheduleService, ScheduleService>();

        }
    }
}