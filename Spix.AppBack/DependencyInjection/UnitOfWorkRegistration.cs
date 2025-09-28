using Spix.Services.ImplementEntties;
using Spix.Services.ImplementSecure;
using Spix.Services.InterfaceEntities;
using Spix.Services.InterfacesSecure;
using Spix.UnitOfWork.ImplementEntities;
using Spix.UnitOfWork.ImplementSecure;
using Spix.UnitOfWork.InterfaceEntities;
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
    }
}