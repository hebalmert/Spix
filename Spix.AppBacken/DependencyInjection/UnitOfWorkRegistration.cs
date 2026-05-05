using Spix.AppService.ImplementEntitiesData;
using Spix.AppService.ImplementEntitiesGen;
using Spix.AppService.InterfaceEntities;
using Spix.AppService.InterfacesEntitiesData;
using Spix.AppService.InterfacesEntitiesGen;
using Spix.AppService.InterfacesInven;
using Spix.AppService.InterfacesSecure;
using Spix.AppServiceX.ImplementEntitiesGen;
using Spix.AppServiceX.ImplementInven;
using Spix.AppServiceX.InterfaceEntities;
using Spix.AppServiceX.InterfacesEntitiesData;
using Spix.AppServiceX.InterfacesEntitiesGen;
using Spix.AppServiceX.InterfacesInven;
using Spix.AppServiceX.InterfacesSecure;
using Spix.Domain.EntitiesInven;
using Spix.Services.ImplementEntties;
using Spix.Services.ImplementInven;
using Spix.Services.ImplementSecure;
using Spix.ServiceX.ImplementSecure;
using Spix.UnitOfWork.ImplementEntities;
using Spix.UnitOfWork.ImplementEntitiesData;
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
        }
    }
}