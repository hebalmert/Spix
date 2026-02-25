using Microsoft.EntityFrameworkCore;
using Spix.AppBack.LoadCountries;
using Spix.AppInfra;
using Spix.AppInfra.UserHelper;
using Spix.Domain.Entities;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppBack.Data;

public class SeedDb
{
    private readonly DataContext _context;
    private readonly IApiService _apiService;
    private readonly IUserHelper _userHelper;

    public SeedDb(DataContext context, IApiService apiService, IUserHelper userHelper)
    {
        _context = context;
        _apiService = apiService;
        _userHelper = userHelper;
    }

    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await CheckRolesAsync();
        await CheckCountries();
        await CheckSoftPlan();
        await CheckCorporationAsync();
        await CheckUserAsync("Nexxtplanet", "TrialPro", "hebalmert", "merchanhebert@gmail.com", "+1 786 503", UserType.Admin);
    }

    private async Task CheckSoftPlan()
    {
        if (!_context.SoftPlans.Any())
        {
            //Alimentando Planes
            _context.SoftPlans.Add(new SoftPlan
            {
                Name = "Plan 1 Mes",
                Price = 50,
                Meses = 1,
                ClientsCount = 2,
                Active = true
            });
            _context.SoftPlans.Add(new SoftPlan
            {
                Name = "Plan 6 Mes",
                Price = 300,
                Meses = 6,
                ClientsCount = 10,
                Active = true
            });
            _context.SoftPlans.Add(new SoftPlan
            {
                Name = "Plan 12 Mes",
                Price = 600,
                Meses = 12,
                ClientsCount = 100,
                Active = true
            });
            await _context.SaveChangesAsync();
        }
    }


    private async Task CheckCorporationAsync()
    {
        if (!_context.Corporations.Any())
        {
            Corporation corporation = new()
            {
                Name = "Nexxtplanet LLC",
                NroDocument = "3445645645",
                Phone = "786 503 4489",
                Address = "Street 45",
                CountryId = 1,
                SoftPlanId = 3,
                DateStart = DateTime.Now,
                DateEnd = DateTime.Now.AddYears(10),
                Active = true
            };
            _context.Corporations.Add(corporation);
            await _context.SaveChangesAsync();
        }
    }

    private async Task<User> CheckUserAsync(string firstName, string lastName, string username, string email,
    string phone, UserType userType)
    {
        User user = await _userHelper.GetUserByUserNameAsync(username);
        if (user == null)
        {
            user = new()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                UserName = username,
                PhoneNumber = phone,
                JobPosition = "Administrador",
                UserFrom = "SeedDb",
                UserRoleDetails = new List<UserRoleDetails> { new UserRoleDetails { UserType = userType } },
                Active = true,
            };

            await _userHelper.AddUserAsync(user, "hebert1234");
            await _userHelper.AddUserToRoleAsync(user, userType.ToString());

            //Para Confirmar automaticamente el Usuario y activar la cuenta
            string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            await _userHelper.ConfirmEmailAsync(user, token);
            await _userHelper.AddUserClaims(userType, username);
        }
        return user;
    }



    private async Task CheckRolesAsync()
    {
        await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
        await _userHelper.CheckRoleAsync(UserType.Administrator.ToString());
        await _userHelper.CheckRoleAsync(UserType.Auxiliar.ToString());
        await _userHelper.CheckRoleAsync(UserType.Cachier.ToString());
        await _userHelper.CheckRoleAsync(UserType.Collector.ToString());
        await _userHelper.CheckRoleAsync(UserType.Contractor.ToString());
        await _userHelper.CheckRoleAsync(UserType.Technician.ToString());
        await _userHelper.CheckRoleAsync(UserType.Usuario.ToString());
        await _userHelper.CheckRoleAsync(UserType.WarehouseLead.ToString());
    }


    private async Task CheckCountries()
    {
        Response responseCountries = await _apiService.GetListAsync<CountryResponse>("/v1", "/countries");
        if (responseCountries.IsSuccess)
        {
            List<CountryResponse> NlistCountry = (List<CountryResponse>)responseCountries.Result!;
            List<CountryResponse> countries = NlistCountry.Where(x => x.Name == "United States").ToList();

            foreach (CountryResponse item in countries)
            {
                Country? country = await _context.Countries.FirstOrDefaultAsync(c => c.Name == item.Name);
                if (country == null)
                {
                    country = new() { Name = item.Name!, States = new List<State>() };
                    Response responseStates = await _apiService.GetListAsync<StateResponse>("/v1", $"/countries/{item.Iso2}/states");
                    if (responseStates.IsSuccess)
                    {
                        List<StateResponse> states = (List<StateResponse>)responseStates.Result!;
                        foreach (StateResponse stateResponse in states!)
                        {
                            State state = country.States!.FirstOrDefault(s => s.Name == stateResponse.Name!)!;
                            if (state == null)
                            {
                                state = new() { Name = stateResponse.Name!, Cities = new List<City>() };
                                Response responseCities = await _apiService.GetListAsync<CityResponse>("/v1", $"/countries/{item.Iso2}/states/{stateResponse.Iso2}/cities");
                                if (responseCities.IsSuccess)
                                {
                                    List<CityResponse> cities = (List<CityResponse>)responseCities.Result!;
                                    foreach (CityResponse cityResponse in cities)
                                    {
                                        if (cityResponse.Name == "Mosfellsbær" || cityResponse.Name == "Șăulița")
                                        {
                                            continue;
                                        }
                                        City city = state.Cities!.FirstOrDefault(c => c.Name == cityResponse.Name!)!;
                                        if (city == null)
                                        {
                                            state.Cities.Add(new City() { Name = cityResponse.Name! });
                                        }
                                    }
                                }
                                if (state.CitiesNumber > 0)
                                {
                                    country.States.Add(state);
                                }
                            }
                        }
                    }
                    if (country.StatesNumber > 0)
                    {
                        _context.Countries.Add(country);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}