using Spix.DomainLogic.EntitiesSaaSDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfacesSaaS;

public interface ISaasSubscriptionServiceX
{
    Task<ActionResponse<IEnumerable<PublicSoftPlanDTO>>> GetPublicPlansAsync();

    Task<ActionResponse<SubscriptionAccessDTO>> StartTrialAsync(StartTrialRequestDTO request, string frontUrl);

    Task<ActionResponse<SubscriptionAccessDTO>> GetAccessAsync(int corporationId);

    Task<ActionResponse<SubscriptionCheckoutDTO>> CreateCheckoutAsync(int corporationId,
        string username, SubscriptionCheckoutRequestDTO request);

    Task<ActionResponse<MercadoPagoPlatformSettingDTO>> GetMercadoPagoSettingAsync();

    Task<ActionResponse<MercadoPagoPlatformSettingDTO>> SaveMercadoPagoSettingAsync(
        MercadoPagoPlatformSettingDTO setting, string username);

    Task<ActionResponse<bool>> SyncMercadoPagoSubscriptionAsync(string? notificationType,
        string? preapprovalId, string? signature, string? requestId);
}
