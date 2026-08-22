using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Spix.AppBack.LoadCountries;
using Spix.AppInfra;
using Spix.AppInfra.UserHelper;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesData;
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
        LogDatabaseTarget();
        await RunSeedStepAsync("Roles", CheckRolesAsync);
        await RunSeedStepAsync("Countries", CheckCountries);
        await RunSeedStepAsync("SoftPlans", CheckSoftPlan);
        await RunSeedStepAsync("Corporation", CheckCorporationAsync);
        await RunSeedStepAsync("Frecuencies", CheckFrecuencies);
        await RunSeedStepAsync("Channels", CheckChannel);
        await RunSeedStepAsync("HotSpotTypes", CheckHotSpotTypes);
        await RunSeedStepAsync("Operations", CheckOperations);
        await RunSeedStepAsync("Securities", CheckSecurity);
        await RunSeedStepAsync("AdminUser", () => CheckUserAsync("Nexxtplanet", "TrialPro", "hebalmert", "nexxtplanet.soft@gmail.com", "+1 786 503", UserType.Admin));
        await LogSeedCountsAsync();
    }

    private void LogDatabaseTarget()
    {
        var connection = _context.Database.GetDbConnection();
        Console.WriteLine($"Seed DB Target: Server={connection.DataSource}; Database={connection.Database}");
    }

    private async Task LogSeedCountsAsync()
    {
        Console.WriteLine($"Seed Counts: Roles={await _context.Roles.CountAsync()}; Users={await _context.Users.CountAsync()}; Countries={await _context.Countries.CountAsync()}; States={await _context.States.CountAsync()}; Cities={await _context.Cities.CountAsync()}; SoftPlans={await _context.SoftPlans.CountAsync()}; Corporations={await _context.Corporations.CountAsync()}");
    }

    private static async Task RunSeedStepAsync(string stepName, Func<Task> seedStep)
    {
        Console.WriteLine($"Seed: {stepName} iniciado");
        try
        {
            await seedStep();
            Console.WriteLine($"Seed: {stepName} OK");
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Seed fallo en {stepName}: {ex.Message}", ex);
        }
    }

    private async Task CheckSoftPlan()
    {
        List<SoftPlan> softPlans = await _context.SoftPlans
            .OrderBy(x => x.SoftPlanId)
            .ToListAsync();

        if (softPlans.Count == 0)
        {
            _context.SoftPlans.AddRange(CreateCommercialSoftPlans());
            await _context.SaveChangesAsync();
            return;
        }

        if (!HasLegacyDemoSoftPlans(softPlans))
        {
            return;
        }

        List<SoftPlan> commercialPlans = CreateCommercialSoftPlans();
        for (int index = 0; index < softPlans.Count; index++)
        {
            ApplyCommercialPlan(softPlans[index], commercialPlans[index]);
        }

        _context.SoftPlans.Add(commercialPlans[3]);
        await _context.SaveChangesAsync();
    }

    private static bool HasLegacyDemoSoftPlans(List<SoftPlan> softPlans)
    {
        return softPlans.Count == 3
            && softPlans[0].Name == "Plan 1 Mes"
            && softPlans[0].Price == 50
            && softPlans[0].ClientsCount == 2
            && softPlans[1].Name == "Plan 6 Mes"
            && softPlans[1].Price == 300
            && softPlans[1].ClientsCount == 10
            && softPlans[2].Name == "Plan 12 Mes"
            && softPlans[2].Price == 600
            && softPlans[2].ClientsCount == 100;
    }

    private static List<SoftPlan> CreateCommercialSoftPlans()
    {
        return new List<SoftPlan>
        {
            new SoftPlan
            {
                Name = "Inicio",
                Price = 79900,
                AnnualPrice = 799000,
                Meses = 1,
                ClientsCount = 75,
                DisplayOrder = 1,
                PublicDescription = "Para ISP que inicia su operacion organizada.",
                Active = true
            },
            new SoftPlan
            {
                Name = "Crecimiento",
                Price = 149900,
                AnnualPrice = 1499000,
                Meses = 1,
                ClientsCount = 250,
                DisplayOrder = 2,
                PublicDescription = "Para equipos que consolidan clientes y red.",
                IsRecommended = true,
                Active = true
            },
            new SoftPlan
            {
                Name = "Profesional",
                Price = 249900,
                AnnualPrice = 2499000,
                Meses = 1,
                ClientsCount = 600,
                DisplayOrder = 3,
                PublicDescription = "Para una operacion ISP en expansion.",
                Active = true
            },
            new SoftPlan
            {
                Name = "Empresa",
                Price = 399900,
                AnnualPrice = 3999000,
                Meses = 1,
                ClientsCount = 1500,
                DisplayOrder = 4,
                PublicDescription = "Para ISP con operacion consolidada.",
                Active = true
            }
        };
    }

    private static void ApplyCommercialPlan(SoftPlan target, SoftPlan source)
    {
        target.Name = source.Name;
        target.Price = source.Price;
        target.AnnualPrice = source.AnnualPrice;
        target.Meses = source.Meses;
        target.ClientsCount = source.ClientsCount;
        target.DisplayOrder = source.DisplayOrder;
        target.PublicDescription = source.PublicDescription;
        target.IsRecommended = source.IsRecommended;
        target.Active = source.Active;
    }


    private async Task CheckCorporationAsync()
    {
        if (!_context.Corporations.Any())
        {
            Country country = await _context.Countries.FirstOrDefaultAsync(x => x.Name == "United States")
                ?? await _context.Countries.FirstAsync();

            SoftPlan softPlan = await _context.SoftPlans.OrderByDescending(x => x.Meses).FirstAsync();

            Corporation corporation = new()
            {
                Name = "Nexxtplanet LLC",
                NroDocument = "3445645645",
                Phone = "786 503 4489",
                Address = "Street 45",
                CountryId = country.CountryId,
                SoftPlanId = softPlan.SoftPlanId,
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

            var createResult = await _userHelper.AddUserAsync(user, "hebert1234");
            if (!createResult.Succeeded)
            {
                string errors = string.Join(" | ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new ApplicationException($"No se pudo crear el usuario seed '{username}': {errors}");
            }

            await _userHelper.AddUserToRoleAsync(user, userType.ToString());

            //Para Confirmar automaticamente el Usuario y activar la cuenta
            string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            var confirmResult = await _userHelper.ConfirmEmailAsync(user, token);
            if (!confirmResult.Succeeded)
            {
                string errors = string.Join(" | ", confirmResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new ApplicationException($"No se pudo confirmar el email del usuario seed '{username}': {errors}");
            }

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
        await _userHelper.CheckRoleAsync(UserType.Client.ToString());
        await _userHelper.CheckRoleAsync(UserType.WarehouseLead.ToString());
    }


    private async Task CheckCountries()
    {
        //El catalogo geografico se siembra desde un archivo del propio proyecto y NO desde el API
        //externo: la cuenta de countrystatecity.in permite 100 peticiones al dia y este catalogo
        //necesita cerca de 500 (una por cada estado de cada pais), asi que quedaba siempre a medias.
        //El archivo viaja con el publish, se siembra igual en cualquier maquina y no depende de la red.
        string rutaSemilla = Path.Combine(AppContext.BaseDirectory, "Data", "SeedData", "countries-seed.json");

        if (!File.Exists(rutaSemilla))
        {
            Console.WriteLine($"Seed Countries: no se encontro el archivo {rutaSemilla}");
            await EnsureRequiredCountriesAsync();
            return;
        }

        string contenido = await File.ReadAllTextAsync(rutaSemilla);
        List<CountrySeedDTO> paisesSemilla = JsonSerializer.Deserialize<List<CountrySeedDTO>>(
            contenido, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CountrySeedDTO>();

        //Con miles de ciudades por pais, dejar el detector de cambios encendido hace la siembra
        //lentisima. Se apaga y se usa Add/AddRange explicito, que sigue registrando el grafo.
        bool detectarCambios = _context.ChangeTracker.AutoDetectChangesEnabled;
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            foreach (CountrySeedDTO paisSemilla in paisesSemilla)
            {
                Country? country = await _context.Countries
                    .Include(c => c.States)
                    .FirstOrDefaultAsync(c => c.Name == paisSemilla.Name);

                //Ya tiene estados: esta completo y no se toca.
                if (country != null && country.States != null && country.States.Count > 0)
                {
                    continue;
                }

                //Un pais que quedo creado VACIO (por EnsureRequiredCountriesAsync) se completa aqui,
                //en vez de darlo por hecho y dejarlo para siempre sin estados ni ciudades.
                List<State> estadosNuevos = paisSemilla.States
                    .Select(estadoSemilla => new State
                    {
                        Name = estadoSemilla.Name,
                        Cities = estadoSemilla.Cities.Select(nombreCiudad => new City { Name = nombreCiudad }).ToList()
                    })
                    .ToList();

                if (country == null)
                {
                    _context.Countries.Add(new Country { Name = paisSemilla.Name, States = estadosNuevos });
                }
                else
                {
                    estadosNuevos.ForEach(estado => estado.CountryId = country.CountryId);
                    _context.States.AddRange(estadosNuevos);
                }

                await _context.SaveChangesAsync();

                int totalCiudades = estadosNuevos.Sum(estado => estado.Cities!.Count);
                Console.WriteLine($"Seed Countries: {paisSemilla.Name} -> {estadosNuevos.Count} estados, {totalCiudades} ciudades");
            }
        }
        finally
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = detectarCambios;
        }

        await EnsureRequiredCountriesAsync();
    }

    //Estructura del archivo countries-seed.json
    private class CountrySeedDTO
    {
        public string Name { get; set; } = null!;

        public List<StateSeedDTO> States { get; set; } = new();
    }

    private class StateSeedDTO
    {
        public string Name { get; set; } = null!;

        public List<string> Cities { get; set; } = new();
    }

    private async Task EnsureRequiredCountriesAsync()
    {
        string[] requiredCountries =
        {
            "Colombia",
            "United States",
            "Peru",
            "Venezuela",
            "Ecuador",
            "Chile",
            "Mexico",
            "United Kingdom",
            "Spain"
        };

        foreach (string countryName in requiredCountries)
        {
            bool exists = await _context.Countries
                .AnyAsync(country => country.Name == countryName);

            if (exists)
            {
                continue;
            }

            _context.Countries.Add(new Country
            {
                Name = countryName
            });
        }

        await _context.SaveChangesAsync();
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
}
