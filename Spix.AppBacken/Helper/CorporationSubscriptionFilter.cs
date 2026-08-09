using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spix.AppServiceX.InterfacesSaaS;
using System.Security.Claims;

namespace Spix.AppBack.Helper;

public class CorporationSubscriptionFilter : IAsyncActionFilter
{
    private readonly ISaasSubscriptionServiceX _saasSubscriptionService;

    public CorporationSubscriptionFilter(ISaasSubscriptionServiceX saasSubscriptionService)
    {
        _saasSubscriptionService = saasSubscriptionService;
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

        var response = await _saasSubscriptionService.GetAccessAsync(corporationId);
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
}
