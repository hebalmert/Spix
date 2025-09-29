using Microsoft.EntityFrameworkCore;
using Spix.AppBack.LoadCountries;
using Spix.AppInfra;
using Spix.AppInfra.UserHelper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.SpixResponse;

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
        await CheckSoftPlan();
        await CheckFrecuencies();
        await CheckChannel();
        await CheckHotSpotTypes();
        await CheckOperations();
        await CheckSecurity();
        await CheckCountries();
        await CheckCorporationAsync();
        await CheckUserAsync("Nexxtplanet", "TrialPro", "hebalmert", "hebert@optimusu.com", "+1 786 503", UserType.Admin);
    }

    private async Task CheckRolesAsync()
    {
        await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
        await _userHelper.CheckRoleAsync(UserType.Administrator.ToString());
        await _userHelper.CheckRoleAsync(UserType.Auxiliar.ToString());
        await _userHelper.CheckRoleAsync(UserType.Cliente.ToString());
        await _userHelper.CheckRoleAsync(UserType.Cajero.ToString());
        await _userHelper.CheckRoleAsync(UserType.Tecnico.ToString());
        await _userHelper.CheckRoleAsync(UserType.Cobrador.ToString());
        await _userHelper.CheckRoleAsync(UserType.Contratista.ToString());
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
                StudyCount = 2,
                Active = true
            });
            _context.SoftPlans.Add(new SoftPlan
            {
                Name = "Plan 6 Mes",
                Price = 300,
                Meses = 6,
                StudyCount = 10,
                Active = true
            });
            _context.SoftPlans.Add(new SoftPlan
            {
                Name = "Plan 12 Mes",
                Price = 600,
                Meses = 12,
                StudyCount = 100,
                Active = true
            });
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckFrecuencies()
    {
        if (!_context.FrecuencyTypes.Any())
        {
            _context.FrecuencyTypes.Add(new FrecuencyType
            {
                TypeName = "2,4 GHz",
                Active = true,
                Frecuencies = new List<Frecuency>
                {
                    new Frecuency { FrecuencyName = 2412, Active = true},
                    new Frecuency { FrecuencyName = 2417, Active = true},
                    new Frecuency { FrecuencyName = 2422, Active = true},
                    new Frecuency { FrecuencyName = 2427, Active = true},
                    new Frecuency { FrecuencyName = 2432, Active = true},
                    new Frecuency { FrecuencyName = 2437, Active = true},
                    new Frecuency { FrecuencyName = 2442, Active = true},
                    new Frecuency { FrecuencyName = 2447, Active = true},
                    new Frecuency { FrecuencyName = 2452, Active = true},
                    new Frecuency { FrecuencyName = 2457, Active = true},
                    new Frecuency { FrecuencyName = 2462, Active = true},
                    new Frecuency { FrecuencyName = 2467, Active = true},
                    new Frecuency { FrecuencyName = 2472, Active = true},
                    new Frecuency { FrecuencyName = 2484, Active = true},
                }
            });

            _context.Add(new FrecuencyType
            {
                TypeName = "5,8 GHz",
                Active = true,
                Frecuencies = new List<Frecuency>
                {
                    new Frecuency { FrecuencyName = 4920, Active = true},
                    new Frecuency { FrecuencyName = 4940, Active = true},
                    new Frecuency { FrecuencyName = 4960, Active = true},
                    new Frecuency { FrecuencyName = 4980, Active = true},
                    new Frecuency { FrecuencyName = 5000, Active = true},
                    new Frecuency { FrecuencyName = 5020, Active = true},
                    new Frecuency { FrecuencyName = 5040, Active = true},
                    new Frecuency { FrecuencyName = 5060, Active = true},
                    new Frecuency { FrecuencyName = 5080, Active = true},
                    new Frecuency { FrecuencyName = 5100, Active = true},
                    new Frecuency { FrecuencyName = 5120, Active = true},
                    new Frecuency { FrecuencyName = 5140, Active = true},
                    new Frecuency { FrecuencyName = 5160, Active = true},
                    new Frecuency { FrecuencyName = 5180, Active = true},
                    new Frecuency { FrecuencyName = 5200, Active = true},
                    new Frecuency { FrecuencyName = 5220, Active = true},
                    new Frecuency { FrecuencyName = 5240, Active = true},
                    new Frecuency { FrecuencyName = 5260, Active = true},
                    new Frecuency { FrecuencyName = 5280, Active = true},
                    new Frecuency { FrecuencyName = 5300, Active = true},
                    new Frecuency { FrecuencyName = 5320, Active = true},
                    new Frecuency { FrecuencyName = 5340, Active = true},
                    new Frecuency { FrecuencyName = 5360, Active = true},
                    new Frecuency { FrecuencyName = 5380, Active = true},
                    new Frecuency { FrecuencyName = 5400, Active = true},
                    new Frecuency { FrecuencyName = 5420, Active = true},
                    new Frecuency { FrecuencyName = 5440, Active = true},
                    new Frecuency { FrecuencyName = 5460, Active = true},
                    new Frecuency { FrecuencyName = 5480, Active = true},
                    new Frecuency { FrecuencyName = 5500, Active = true},
                    new Frecuency { FrecuencyName = 5520, Active = true},
                    new Frecuency { FrecuencyName = 5540, Active = true},
                    new Frecuency { FrecuencyName = 5560, Active = true},
                    new Frecuency { FrecuencyName = 5580, Active = true},
                    new Frecuency { FrecuencyName = 5600, Active = true},
                    new Frecuency { FrecuencyName = 5620, Active = true},
                    new Frecuency { FrecuencyName = 5640, Active = true},
                    new Frecuency { FrecuencyName = 5660, Active = true},
                    new Frecuency { FrecuencyName = 5680, Active = true},
                    new Frecuency { FrecuencyName = 5700, Active = true},
                    new Frecuency { FrecuencyName = 5720, Active = true},
                    new Frecuency { FrecuencyName = 5740, Active = true},
                    new Frecuency { FrecuencyName = 5760, Active = true},
                    new Frecuency { FrecuencyName = 5780, Active = true},
                    new Frecuency { FrecuencyName = 5800, Active = true},
                    new Frecuency { FrecuencyName = 5820, Active = true},
                    new Frecuency { FrecuencyName = 5840, Active = true},
                    new Frecuency { FrecuencyName = 5860, Active = true},
                    new Frecuency { FrecuencyName = 5880, Active = true},
                    new Frecuency { FrecuencyName = 5900, Active = true},
                    new Frecuency { FrecuencyName = 5920, Active = true},
                    new Frecuency { FrecuencyName = 5940, Active = true},
                    new Frecuency { FrecuencyName = 5960, Active = true},
                    new Frecuency { FrecuencyName = 5980, Active = true},
                    new Frecuency { FrecuencyName = 6000, Active = true},
                    new Frecuency { FrecuencyName = 6020, Active = true},
                    new Frecuency { FrecuencyName = 6040, Active = true},
                    new Frecuency { FrecuencyName = 6060, Active = true},
                    new Frecuency { FrecuencyName = 6080, Active = true},
                    new Frecuency { FrecuencyName = 6100, Active = true},
                    new Frecuency { FrecuencyName = 6120, Active = true},
                    new Frecuency { FrecuencyName = 6140, Active = true},
                    new Frecuency { FrecuencyName = 6160, Active = true},
                    new Frecuency { FrecuencyName = 6180, Active = true},
                    new Frecuency { FrecuencyName = 6200, Active = true},
                    new Frecuency { FrecuencyName = 6220, Active = true},
                    new Frecuency { FrecuencyName = 6240, Active = true},
                    new Frecuency { FrecuencyName = 6260, Active = true},
                    new Frecuency { FrecuencyName = 6280, Active = true},
                    new Frecuency { FrecuencyName = 6300, Active = true},
                    new Frecuency { FrecuencyName = 6320, Active = true},
                    new Frecuency { FrecuencyName = 6340, Active = true},
                    new Frecuency { FrecuencyName = 6360, Active = true},
                    new Frecuency { FrecuencyName = 6380, Active = true},
                    new Frecuency { FrecuencyName = 6400, Active = true},
                }
            });

            _context.Add(new FrecuencyType
            {
                TypeName = "6 - 7 GHz",
                Active = true,
                Frecuencies = new List<Frecuency>
                {
                    new Frecuency { FrecuencyName = 58320, Active = true},
                    new Frecuency { FrecuencyName = 59400, Active = true},
                    new Frecuency { FrecuencyName = 60480, Active = true},
                    new Frecuency { FrecuencyName = 61560, Active = true},
                    new Frecuency { FrecuencyName = 62640, Active = true},
                    new Frecuency { FrecuencyName = 63720, Active = true},
                    new Frecuency { FrecuencyName = 64800, Active = true},
                    new Frecuency { FrecuencyName = 65880, Active = true},
                    new Frecuency { FrecuencyName = 66960, Active = true},
                    new Frecuency { FrecuencyName = 68040, Active = true},
                    new Frecuency { FrecuencyName = 69120, Active = true},
                    new Frecuency { FrecuencyName = 70200, Active = true},
                    new Frecuency { FrecuencyName = 71280, Active = true},
                    new Frecuency { FrecuencyName = 72360, Active = true},
                    new Frecuency { FrecuencyName = 73440, Active = true}
                }
            });
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckChannel()
    {
        if (!_context.Channels.Any())
        {
            _context.Channels.Add(new Channel { ChannelName = "20 Mhz", Active = true });
            _context.Channels.Add(new Channel { ChannelName = "40 Mhz", Active = true });
            _context.Channels.Add(new Channel { ChannelName = "50 Mhz", Active = true });
            _context.Channels.Add(new Channel { ChannelName = "60 Mhz", Active = true });
            _context.Channels.Add(new Channel { ChannelName = "80 Mhz", Active = true });
            _context.Channels.Add(new Channel { ChannelName = "100 Mhz", Active = true });
            _context.Channels.Add(new Channel { ChannelName = "160 Mhz", Active = true });
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckHotSpotTypes()
    {
        if (!_context.HotSpotTypes.Any())
        {
            _context.Add(new HotSpotType { TypeName = "blocked", Active = true });
            _context.Add(new HotSpotType { TypeName = "bypassed", Active = true });
            _context.Add(new HotSpotType { TypeName = "regular", Active = true });
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckOperations()
    {
        if (!_context.Operations.Any())
        {
            _context.Add(new Operation { OperationName = "AP Punto a Punto", Active = true });
            _context.Add(new Operation { OperationName = "AP Punto MultiPunto", Active = true });
            _context.Add(new Operation { OperationName = "AP Clientes", Active = true });
            _context.Add(new Operation { OperationName = "Estacion Punto a Punto", Active = true });
            _context.Add(new Operation { OperationName = "Estacion Cliente", Active = true });
            _context.Add(new Operation { OperationName = "Punto a Punto", Active = true });
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckSecurity()
    {
        if (!_context.Securities.Any())
        {
            _context.Add(new Security { SecurityName = "WAP", Active = true });
            _context.Add(new Security { SecurityName = "WAP2", Active = true });
            _context.Add(new Security { SecurityName = "WAP-WAP2", Active = true });
            await _context.SaveChangesAsync();
        }
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

    private async Task CheckCorporationAsync()
    {
        if (!_context.Corporations.Any())
        {
            Corporation corporation = new()
            {
                Name = "Nexxtplanet LLC",
                TypeDocument = "ITIN",
                NroDocument = "3445645645",
                Phone = "786",
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
                FullName = $"{firstName} {lastName}",
                Email = email,
                UserName = username,
                PhoneNumber = phone,
                JobPosition = "Administrador",
                UserFrom = "SeedDb",
                UserRoleDetails = new List<UserRoleDetails> { new UserRoleDetails { UserType = userType } },
                Active = true,
            };

            await _userHelper.AddUserAsync(user, "123456");
            await _userHelper.AddUserToRoleAsync(user, userType.ToString());

            //Para Confirmar automaticamente el Usuario y activar la cuenta
            string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            await _userHelper.ConfirmEmailAsync(user, token);
            await _userHelper.AddUserClaims(userType, username);
        }
        return user;
    }
}