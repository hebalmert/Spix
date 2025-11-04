using Mapster;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.Mappings;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.NewConfig<Manager, Manager>()
             .Ignore(dest => dest.Corporation!);

        config.NewConfig<Product, Product>()
             .Ignore(dest => dest.ProductCategory!)
             .Ignore(dest => dest.Tax!)
             .Ignore(dest => dest.Corporation!);

        config.NewConfig<ServiceClient, ServiceClient>()
             .Ignore(dest => dest.ServiceCategory!)
             .Ignore(dest => dest.Tax!)
             .Ignore(dest => dest.Corporation!);
    }
}