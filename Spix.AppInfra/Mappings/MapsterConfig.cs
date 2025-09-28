using Mapster;
using Spix.Domain.Entities;

namespace Spix.AppInfra.Mappings;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        config.NewConfig<Manager, Manager>()
             .Ignore(dest => dest.Corporation!);
    }
}