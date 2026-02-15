using Spix.AppService.InterfaceEntities;
using Spix.AppService.InterfacesSecure;
using Spix.AppServiceX.InterfaceEntities;
using Spix.AppServiceX.InterfacesSecure;
using Spix.Services.ImplementEntties;
using Spix.Services.ImplementSecure;
using Spix.ServiceX.ImplementSecure;
using Spix.UnitOfWork.ImplementEntities;
using Spix.UnitOfWork.ImplementSecure;

namespace Spin.AppBack.DependencyInjection
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

        }
    }
}