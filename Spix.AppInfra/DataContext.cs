using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesData;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.Domain.EntitiesNet;
using Spix.Domain.EntitiesOper;
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


    //EntitiesData

    public DbSet<FrecuencyType> FrecuencyTypes => Set<FrecuencyType>();
    public DbSet<Frecuency> Frecuencies => Set<Frecuency>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<Security> Securities => Set<Security>();
    public DbSet<HotSpotType> HotSpotTypes => Set<HotSpotType>();
    public DbSet<ChainType> ChainTypes => Set<ChainType>();

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


    //EntitiesInven

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<ProductStorage> ProductStorages => Set<ProductStorage>();
    public DbSet<ProductStock> ProductStocks => Set<ProductStock>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseDetail> PurchaseDetails => Set<PurchaseDetail>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<TransferDetails> TransferDetails => Set<TransferDetails>();
    public DbSet<Cargue> Cargues => Set<Cargue>();
    public DbSet<CargueDetail> CargueDetails => Set<CargueDetail>();


    //EntitiesNet

    public DbSet<IpNetwork> IpNetworks => Set<IpNetwork>();
    public DbSet<IpNet> IpNets => Set<IpNet>();
    public DbSet<Node> Nodes => Set<Node>();
    public DbSet<Server> Servers => Set<Server>();


    //EntitiesOper

    public DbSet<Contractor> Contractors => Set<Contractor>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<Client> Clients => Set<Client>();

    //EntitiesContratos

    public DbSet<ContractIp> ContractIps => Set<ContractIp>();
    public DbSet<ContractServer> ContractServers => Set<ContractServer>();
    public DbSet<ContractPlan> ContractPlans => Set<ContractPlan>();
    public DbSet<ContractNode> ContractNodes => Set<ContractNode>();
    public DbSet<ContractQue> ContractQues => Set<ContractQue>();
    public DbSet<ContractClient> ContractClients => Set<ContractClient>();


    //Esta parte nos permite tomar las configuraciones desde otra ubicacion, para mantener el codigo mas ordenado
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Para tomar los calores de ConfigEntities
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
