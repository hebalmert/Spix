using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesData;
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
    public DbSet<SoftPlan> SoftPlans => Set<SoftPlan>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Corporation> Corporations => Set<Corporation>();

    //EntitiesData

    public DbSet<FrecuencyType> FrecuencyTypes => Set<FrecuencyType>();
    public DbSet<Frecuency> Frecuencies => Set<Frecuency>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<Security> Securities => Set<Security>();
    public DbSet<HotSpotType> HotSpotTypes => Set<HotSpotType>();
    public DbSet<ChainType> ChainTypes => Set<ChainType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Para tomar los calores de ConfigEntities
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}