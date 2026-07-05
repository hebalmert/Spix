using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitesSoftSec;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesData;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.Domain.EntitiesMK;
using Spix.Domain.EntitiesNet;
using Spix.Domain.EntitiesOper;
using Spix.Domain.EntitiesPayment;
using Spix.Domain.EntitiesSchedule;
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

    //EntitiesMK

    public DbSet<ConnectionMikrotikControl> ConnectionMikrotikControls => Set<ConnectionMikrotikControl>();
    public DbSet<QueueParent> QueueParents => Set<QueueParent>();
    public DbSet<QueueType> QueueTypes => Set<QueueType>();


    //EntitiesOper

    public DbSet<Contractor> Contractors => Set<Contractor>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<Client> Clients => Set<Client>();


    //EntitiesContratos

    public DbSet<ContractIp> ContractIps => Set<ContractIp>();
    public DbSet<ContractServer> ContractServers => Set<ContractServer>();
    public DbSet<ContractPlan> ContractPlans => Set<ContractPlan>();
    public DbSet<ContractNode> ContractNodes => Set<ContractNode>();
    public DbSet<ContractMap> ContractMaps => Set<ContractMap>();
    public DbSet<ContractQue> ContractQues => Set<ContractQue>();
    public DbSet<ContractBind> ContractBinds => Set<ContractBind>();
    public DbSet<ContractClient> ContractClients => Set<ContractClient>();
    public DbSet<ContractIDPic> ContractIDPics => Set<ContractIDPic>();
    public DbSet<ContractMac> ContractMacs => Set<ContractMac>();

    //EntitiesSchedule
    public DbSet<ScheduleItem> ScheduleItems => Set<ScheduleItem>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<ServiceRequestDetail> ServiceRequestDetails => Set<ServiceRequestDetail>();
    public DbSet<ServiceRequestPic> ServiceRequestPics => Set<ServiceRequestPic>();


    //EntitiesBilling
    public DbSet<Sell> Sells => Set<Sell>();
    public DbSet<SellDetail> SellDetails => Set<SellDetail>();
    public DbSet<BillingNote> BillingNotes => Set<BillingNote>();
    public DbSet<BillingNoteOne> BillingNoteOnes => Set<BillingNoteOne>();


    //EntitiesPayment
    public DbSet<CxCBill> CxCBills => Set<CxCBill>();
    public DbSet<CxCBillDetail> CxCBillDetails => Set<CxCBillDetail>();
    public DbSet<PrePayment> PrePayments => Set<PrePayment>();
    public DbSet<PreExonerated> PreExonerateds => Set<PreExonerated>();



    //Esta parte nos permite tomar las configuraciones desde otra ubicacion, para mantener el codigo mas ordenado
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Para tomar los calores de ConfigEntities
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
