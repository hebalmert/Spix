using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spix.AppServiceX.InterfacesSaaS;

namespace Spix.AppBacken.Controllers.v1.EntitiesSaaS;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mercadopago-webhook")]
[AllowAnonymous]
[ApiController]
public class MercadoPagoWebhookController : ControllerBase
{
    private readonly ISaasSubscriptionServiceX _saasSubscriptionService;

    public MercadoPagoWebhookController(ISaasSubscriptionServiceX saasSubscriptionService)
    {
        _saasSubscriptionService = saasSubscriptionService;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync()
    {
        string? notificationType = Request.Query["type"].FirstOrDefault()
            ?? Request.Query["topic"].FirstOrDefault();
        string? preapprovalId = Request.Query["data.id"].FirstOrDefault()
            ?? Request.Query["id"].FirstOrDefault();
        string? signature = Request.Headers["x-signature"].FirstOrDefault();
        string? requestId = Request.Headers["x-request-id"].FirstOrDefault();

        var response = await _saasSubscriptionService.SyncMercadoPagoSubscriptionAsync(
            notificationType, preapprovalId, signature, requestId);
        return response.WasSuccess ? Ok() : Unauthorized(response.Message);
    }
}
