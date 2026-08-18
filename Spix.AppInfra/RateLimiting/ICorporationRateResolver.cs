namespace Spix.AppInfra.RateLimiting;

// Devuelve el limite de peticiones/minuto de una corporation (segun el SoftPlan de su plan).
// 0 = sin limite. Cacheado para no consultar la BD en cada request.
public interface ICorporationRateResolver
{
    int GetRatePerMinute(int corporationId);
}
