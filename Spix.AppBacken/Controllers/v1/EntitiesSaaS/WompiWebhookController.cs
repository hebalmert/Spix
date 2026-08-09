using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spix.AppServiceX.InterfacesSaaS;
using Spix.DomainLogic.EntitiesSaaSDTO;

namespace Spix.AppBacken.Controllers.v1.EntitiesSaaS;

/// <summary>
/// Punto público que Wompi utiliza para confirmar los pagos de suscripción.
/// La vigencia se renueva únicamente después de validar este evento.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/wompi-webhook")]
[AllowAnonymous]
[ApiController]
public class WompiWebhookController : ControllerBase
{
    private readonly ISaasSubscriptionServiceX _subscriptionService;

    public WompiWebhookController(ISaasSubscriptionServiceX subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] WompiEventDTO eventDto)
    {
        var response = await _subscriptionService.SyncWompiSubscriptionAsync(eventDto);

        return response.WasSuccess
            ? Ok()
            : Unauthorized(response.Message);
    }
}
