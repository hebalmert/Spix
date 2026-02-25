using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using System.Reflection;

namespace Spix.AppInfra;

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    //EntitiesSoftSec

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioRole> UsuarioRoles => Set<UsuarioRole>();

    //Manejo de UserRoles por Usuario

    public DbSet<UserRoleDetails> UserRoleDetails => Set<UserRoleDetails>();


    //Entities

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<SoftPlan> SoftPlans => Set<SoftPlan>();
    public DbSet<Corporation> Corporations => Set<Corporation>();


    //EntitiesGen
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<Mark> Marks => Set<Mark>();
    public DbSet<MarkModel> MarkModels => Set<MarkModel>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<PlanCategory> PlanCategories => Set<PlanCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Register> Registers => Set<Register>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServiceClient> ServiceClients => Set<ServiceClient>();
    public DbSet<Tax> Taxes => Set<Tax>();
    public DbSet<Zone> Zones => Set<Zone>();

    


    //Esta parte nos permite tomar las configuraciones desde otra ubicacion, para mantener el codigo mas ordenado
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Para tomar los calores de ConfigEntities
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
