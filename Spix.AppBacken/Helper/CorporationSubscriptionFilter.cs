using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Spix.AppServiceX.InterfacesSaaS;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.EntitiesSaaSDTO;
using System.Security.Claims;

namespace Spix.AppBack.Helper;

public class CorporationSubscriptionFilter : IAsyncActionFilter
{
    //El estado de la suscripcion cambia una vez al mes, pero este filtro corre en CADA request.
    //Con 60s de cache quitamos 2 consultas a la BD por peticion y seguimos reaccionando rapido
    //a una renovacion o a un vencimiento.
    private const int CacheSeconds = 60;

    private readonly ISaasSubscriptionServiceX _saasSubscriptionService;
    private readonly IMemoryCache _cache;

    public CorporationSubscriptionFilter(ISaasSubscriptionServiceX saasSubscriptionService, IMemoryCache cache)
    {
        _saasSubscriptionService = saasSubscriptionService;
        _cache = cache;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ClaimsPrincipal user = context.HttpContext.User;
        bool isAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<Microsoft.AspNetCore.Authorization.IAllowAnonymous>().Any();
        bool allowsExpiredAccess = context.ActionDescriptor.EndpointMetadata.OfType<AllowExpiredSubscriptionAccessAttribute>().Any();
        if (isAnonymous || allowsExpiredAccess || user.Identity?.IsAuthenticated != true || user.IsInRole("Admin"))
        {
            await next();
            return;
        }

        string? corporationClaim = user.Claims.FirstOrDefault(x => x.Type == "CorporateId")?.Value;
        if (!int.TryParse(corporationClaim, out int corporationId) || corporationId <= 0)
        {
            await next();
            return;
        }

        ActionResponse<SubscriptionAccessDTO>? response = await GetAccessCachedAsync(corporationId);
        if (!response.WasSuccess || response.Result?.HasAccess != true)
        {
            context.Result = new ObjectResult(response.Message ?? response.Result?.Message ?? "La suscripcion requiere renovacion.")
            {
                StatusCode = StatusCodes.Status402PaymentRequired
            };
            return;
        }

        await next();
    }

    //Solo se cachean las respuestas exitosas: un error transitorio de la BD no debe dejar
    //bloqueada a la corporacion durante todo el minuto siguiente.
    private async Task<ActionResponse<SubscriptionAccessDTO>> GetAccessCachedAsync(int corporationId)
    {
        if (_cache.TryGetValue($"subaccess_{corporationId}", out ActionResponse<SubscriptionAccessDTO>? cached) && cached != null)
        {
            return cached;
        }

        ActionResponse<SubscriptionAccessDTO> response = await _saasSubscriptionService.GetAccessAsync(corporationId);
        if (response.WasSuccess)
        {
            _cache.Set($"subaccess_{corporationId}", response, TimeSpan.FromSeconds(CacheSeconds));
        }

        return response;
    }
}
