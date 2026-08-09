using Spix.AppService.InterfacesSaaS;
using Spix.AppServiceX.InterfacesSaaS;
using Spix.DomainLogic.EntitiesSaaSDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementSaaS;

public class SaasSubscriptionServiceX : ISaasSubscriptionServiceX
{
    private readonly ISaasSubscriptionService _saasSubscriptionService;

    public SaasSubscriptionServiceX(ISaasSubscriptionService saasSubscriptionService)
    {
        _saasSubscriptionService = saasSubscriptionService;
    }

    public async Task<ActionResponse<IEnumerable<PublicSoftPlanDTO>>> GetPublicPlansAsync()
        => await _saasSubscriptionService.GetPublicPlansAsync();

    public async Task<ActionResponse<SubscriptionAccessDTO>> StartTrialAsync(StartTrialRequestDTO request, string frontUrl)
        => await _saasSubscriptionService.StartTrialAsync(request, frontUrl);

    public async Task<ActionResponse<SubscriptionAccessDTO>> GetAccessAsync(int corporationId)
        => await _saasSubscriptionService.GetAccessAsync(corporationId);

    public async Task<ActionResponse<SubscriptionCheckoutDTO>> CreateCheckoutAsync(int corporationId,
        string username, SubscriptionCheckoutRequestDTO request)
        => await _saasSubscriptionService.CreateCheckoutAsync(corporationId, username, request);

    public async Task<ActionResponse<MercadoPagoPlatformSettingDTO>> GetMercadoPagoSettingAsync()
        => await _saasSubscriptionService.GetMercadoPagoSettingAsync();

    public async Task<ActionResponse<MercadoPagoPlatformSettingDTO>> SaveMercadoPagoSettingAsync(
        MercadoPagoPlatformSettingDTO setting, string username)
        => await _saasSubscriptionService.SaveMercadoPagoSettingAsync(setting, username);

    public async Task<ActionResponse<bool>> SyncMercadoPagoSubscriptionAsync(string? notificationType,
        string? preapprovalId, string? signature, string? requestId)
    {
        return await _saasSubscriptionService.SyncMercadoPagoSubscriptionAsync(notificationType, preapprovalId,
            signature, requestId);
    }

    public async Task<ActionResponse<bool>> SyncWompiSubscriptionAsync(WompiEventDTO eventDto)
    {
        return await _saasSubscriptionService.SyncWompiSubscriptionAsync(eventDto);
    }
}
