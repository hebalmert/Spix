using Spix.Services.ImplementEntitiesData;
using Spix.Services.ImplementEntitiesGen;
using Spix.Services.ImplementEntitiesNet;
using Spix.Services.ImplementEntties;
using Spix.Services.ImplementInven;
using Spix.Services.ImplementSecure;
using Spix.Services.InterfaceEntities;
using Spix.Services.InterfaceEntitiesNet;
using Spix.Services.InterfacesEntitiesData;
using Spix.Services.InterfacesEntitiesGen;
using Spix.Services.InterfacesInven;
using Spix.Services.InterfacesSecure;
using Spix.UnitOfWork.ImplementEntities;
using Spix.UnitOfWork.ImplementEntitiesData;
using Spix.UnitOfWork.ImplementEntitiesGen;
using Spix.UnitOfWork.ImplementEntitiesNet;
using Spix.UnitOfWork.ImplementInven;
using Spix.UnitOfWork.ImplementSecure;
using Spix.UnitOfWork.InterfaceEntities;
using Spix.UnitOfWork.InterfaceEntitiesNet;
using Spix.UnitOfWork.InterfacesEntitiesData;
using Spix.UnitOfWork.InterfacesEntitiesGen;
using Spix.UnitOfWork.InterfacesInven;
using Spix.UnitOfWork.InterfacesSecure;

namespace Spix.AppBack.DependencyInjection;

public class UnitOfWorkRegistration
{
    public static void AddUnitOfWorkRegistration(IServiceCollection services)
    {
        //EntitiesSecurities Software
        services.AddScoped<IAccountUnitOfWork, AccountUnitOfWork>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IUsuarioUnitOfWork, UsuarioUnitOfWork>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IUsuarioRoleUnitOfWork, UsuarioRoleUnitOfWork>();
        services.AddScoped<IUsuarioRoleService, UsuarioRoleService>();

        //Entities
        services.AddScoped<ICountryUnitOfWork, CountryUnitOfWork>();
        services.AddScoped<ICountryServices, CountryService>();
        services.AddScoped<IStateUnitOfWork, StateUnitOfWork>();
        services.AddScoped<IStateService, StateService>();
        services.AddScoped<ICityUnitOfWork, CityUnitOfWork>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<ISoftPlanUnitOfWork, SoftPlanUnitOfWork>();
        services.AddScoped<ISoftPlanService, SoftPlanService>();
        services.AddScoped<ICorporationUnitOfWork, CorporationUnitOfWork>();
        services.AddScoped<ICorporationService, CorporationService>();
        services.AddScoped<IManagerUnitOfWork, ManagerUnitOfWork>();
        services.AddScoped<IManagerService, ManagerService>();

        //EntitiesData
        services.AddScoped<IChainTypesUnitOfWork, ChainTypesUnitOfWork>();
        services.AddScoped<IChainTypesService, ChainTypesService>();
        services.AddScoped<IChannelUnitOfWork, ChannelUnitOfWork>();
        services.AddScoped<IChannelService, ChannelService>();
        services.AddScoped<IFrecuencyTypeUnitOfWork, FrecuencyTypeUnitOfWork>();
        services.AddScoped<IFrecuencyTypeService, FrecuencyTypeService>();
        services.AddScoped<IFrecuencyUnitOfWork, FrecuencyUnitOfWork>();
        services.AddScoped<IFrecuencyService, FrecuencyService>();
        services.AddScoped<IHotSpotTypeUnitOfWork, HotSpotTypeUnitOfWork>();
        services.AddScoped<IHotSpotTypeService, HotSpotTypeService>();
        services.AddScoped<IOperationUnitOfWork, OperationUnitOfWork>();
        services.AddScoped<IOperationService, OperationService>();
        services.AddScoped<ISecurityUnitOfWork, SecurityUnitOfWork>();
        services.AddScoped<ISecurityService, SecurityService>();

        //EntitiesGen
        services.AddScoped<IDocumentTypeUnitOfWork, DocumentTypeUnitOfWork>();
        services.AddScoped<IDocumentTypeService, DocumentTypeService>();
        services.AddScoped<IRegisterUnitOfWork, RegisterUnitOfWork>();
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<IZoneUnitOfWork, ZoneUnitOfWork>();
        services.AddScoped<IZoneService, ZoneService>();
        services.AddScoped<IMarkUnitOfWork, MarkUnitOfWork>();
        services.AddScoped<IMarkService, MarkService>();
        services.AddScoped<IMarkModelUnitOfWork, MarkModelUnitOfWork>();
        services.AddScoped<IMarkModelService, MarkModelService>();
        services.AddScoped<ITaxUnitOfWork, TaxUnitOfWork>();
        services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<IProductCategoryUnitOfWork, ProductCategoryUnitOfWork>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IProductUnitOfWork, ProductUnitOfWork>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IServiceCategoryUnitOfWork, ServiceCategoryUnitOfWork>();
        services.AddScoped<IServiceCategoryService, ServiceCategoryService>();
        services.AddScoped<IServiceClientUnitOfWork, ServiceClientUnitOfWork>();
        services.AddScoped<IServiceClientService, ServiceClientService>();
        services.AddScoped<IPlanCategoryUnitOfWork, PlanCategoryUnitOfWork>();
        services.AddScoped<IPlanCategoryService, PlanCategoryService>();
        services.AddScoped<IPlanUnitOfWork, PlanUnitOfWork>();
        services.AddScoped<IPlanService, PlanService>();

        //EntitiesInven
        services.AddScoped<ISupplierUnitOfWork, SupplierUnitOfWork>();
        services.AddScoped<ISupplierServices, SupplierService>();
        services.AddScoped<IProductStockUnitOfWork, ProductStockUnitOfWork>();
        services.AddScoped<IProductStockService, ProductStockService>();
        services.AddScoped<IProductStorageUnitOfWork, ProductStorageUnitOfWork>();
        services.AddScoped<IProductStorageService, ProductStorageService>();
        services.AddScoped<IPurchaseUnitOfWork, PurchaseUnitOfWork>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<IPurchaseDetailsUnitOfWork, PurchaseDetailsUnitOfWork>();
        services.AddScoped<IPurchaseDetailsService, PurchaseDetailsService>();
        services.AddScoped<ITransferUnitOfWork, TransferUnitOfWork>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<ITransferDetailsUnitOfWork, TransferDetailsUnitOfWork>();
        services.AddScoped<ITransferDetailsService, TransferDetailsService>();
        services.AddScoped<ICargueUnitOfWork, CargueUnitOfWork>();
        services.AddScoped<ICargueService, CargueService>();
        services.AddScoped<ICargueDetailsUnitOfWork, CargueDetailsUnitOfWork>();
        services.AddScoped<ICargueDetailsService, CargueDetailsService>();

        //EntitiesNet
        services.AddScoped<IIpNetworkUnitOfWork, IpNetworkUnitOfWork>();
        services.AddScoped<IIpNetworkService, IpNetworkService>();
        services.AddScoped<IIpNetUnitOfWork, IpNetUnitOfWork>();
        services.AddScoped<IIpNetService, IpNetService>();
        services.AddScoped<INodeUnitOfWork, NodeUnitOfWork>();
        services.AddScoped<INodeService, NodeService>();
        services.AddScoped<IServerUnitOfWork, ServerUnitOfWork>();
        services.AddScoped<IServerService, ServerService>();
    }
}