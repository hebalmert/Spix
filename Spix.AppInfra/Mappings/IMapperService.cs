namespace Spix.AppInfra.Mappings;

public interface IMapperService
{
    TTarget Map<TSource, TTarget>(TSource source);
}