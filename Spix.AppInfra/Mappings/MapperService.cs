using MapsterMapper;

namespace Spix.AppInfra.Mappings;

public class MapperService : IMapperService
{
    private readonly IMapper _mapper;

    public MapperService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public TTarget Map<TSource, TTarget>(TSource source)
    {
        return _mapper.Map<TTarget>(source!);
    }
}