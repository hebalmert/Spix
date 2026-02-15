using Mapster;


namespace Spix.AppInfra.Mappings;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        var config = TypeAdapterConfig.GlobalSettings;

        //sistema de Pruebas para Trabjar Mappers
        //config.NewConfig<QcGeneral, QcGeneral>()
        //    .Ignore(dest => dest.Study!)
        //    .Ignore(dest => dest.Corporation!);
    }
}