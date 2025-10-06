using Spix.Services.ImplementEntitiesData;
using Spix.Services.ImplementEntitiesGen;
using Spix.Services.ImplementEntties;
using Spix.Services.ImplementSecure;
using Spix.Services.InterfaceEntities;
using Spix.Services.InterfacesEntitiesData;
using Spix.Services.InterfacesEntitiesGen;
using Spix.Services.InterfacesSecure;
using Spix.UnitOfWork.ImplementEntities;
using Spix.UnitOfWork.ImplementEntitiesData;
using Spix.UnitOfWork.ImplementEntitiesGen;
using Spix.UnitOfWork.ImplementSecure;
using Spix.UnitOfWork.InterfaceEntities;
using Spix.UnitOfWork.InterfacesEntitiesData;
using Spix.UnitOfWork.InterfacesEntitiesGen;
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
    }
}