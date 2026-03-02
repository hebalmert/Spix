using Mapster;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesInven;
using Spix.Domain.EntitiesNet;
using Spix.Domain.EntitiesOper;


namespace Spix.AppInfra.Mappings;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.NewConfig<Node, Node>()
            .Ignore(dest => dest.Corporation!)
            .Ignore(dest => dest.IpNetwork!)
            .Ignore(dest => dest.Operation!)
            .Ignore(dest => dest.Mark!)
            .Ignore(dest => dest.MarkModel!)
            .Ignore(dest => dest.Zone!)
            .Ignore(dest => dest.FrecuencyType!)
            .Ignore(dest => dest.Frecuency!)
            .Ignore(dest => dest.Channel!)
            .Ignore(dest => dest.Security!);

        config.NewConfig<Server, Server>()
            .Ignore(dest => dest.Corporation!)
            .Ignore(dest => dest.IpNetwork!)
            .Ignore(dest => dest.Mark!)
            .Ignore(dest => dest.MarkModel!)
            .Ignore(dest => dest.Zone!);

        config.NewConfig<Client, Client>()
            .Ignore(dest => dest.Corporation!)
            .Ignore(dest => dest.DocumentType!);

        config.NewConfig<Contractor, Contractor>()
            .Ignore(dest => dest.Corporation!)
            .Ignore(dest => dest.DocumentType!);

        config.NewConfig<ContractClient, ContractClient>()
            .Ignore(dest => dest.Corporation!)
            .Ignore(dest => dest.Client!)
            .Ignore(dest => dest.ServiceClient!)
            .Ignore(dest => dest.Zone!)
            .Ignore(dest => dest.ServiceCategory!);

        config.NewConfig<Supplier, Supplier>()
            .Ignore(dest => dest.Corporation!)
            .Ignore(dest => dest.DocumentType!)
            .Ignore(dest => dest.State!)
            .Ignore(dest => dest.City!);

        config.NewConfig<ProductStorage, ProductStorage>()
            .Ignore(dest => dest.Corporation!)
            .Ignore(dest => dest.State!)
            .Ignore(dest => dest.City!);

        config.NewConfig<Purchase, Purchase>()
            .Ignore(dest => dest.Corporation!)
            .Ignore(dest => dest.PurchaseDetails!)
            .Ignore(dest => dest.Supplier!)
            .Ignore(dest => dest.ProductStorage!);

        config.NewConfig<Product, Product>()
            .Ignore(dest => dest.Corporation!)
            .Ignore(dest => dest.ProductCategory!)
            .Ignore(dest => dest.Tax!);

        config.NewConfig<Transfer, Transfer>()
            .Ignore(dest => dest.Corporation!)
            .Ignore(dest => dest.User!);

        config.NewConfig<Cargue, Cargue>()
            .Ignore(dest => dest.Corporation!)
            .Ignore(dest => dest.Product!)
            .Ignore(dest => dest.PurchaseDetail!)
            .Ignore(dest => dest.Purchase!);

        config.NewConfig<CargueDetail, CargueDetail>()
            .Ignore(dest => dest.Cargue!);
    }
}