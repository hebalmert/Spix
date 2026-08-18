using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Spix.AppInfra.RateLimiting;

// Singleton: resuelve el rate por corporation desde el SoftPlan, con cache de 60s.
public class CorporationRateResolver : ICorporationRateResolver
{
    private const int CacheSeconds = 60;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;

    public CorporationRateResolver(IServiceScopeFactory scopeFactory, IMemoryCache cache)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
    }

    public int GetRatePerMinute(int corporationId)
    {
        return _cache.GetOrCreate($"rate_{corporationId}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheSeconds);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();

            // Rate del SoftPlan al que pertenece la corporation
            return db.Corporations
                .Where(c => c.CorporationId == corporationId)
                .Select(c => c.SoftPlan!.RateLimitPerMinute)
                .FirstOrDefault();
        });
    }
}
