using Spix.DomainLogic.EntitiesSaaSDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfacesSaaS;

public interface ISystemSettingServiceX
{
    Task<ActionResponse<IEnumerable<SecretSettingDTO>>> GetSystemAsync();

    Task<ActionResponse<IEnumerable<SecretSettingDTO>>> GetPaymentAsync();

    Task<ActionResponse<bool>> SaveSystemAsync(
        IEnumerable<SecretSettingDTO> items,
        string username);

    Task<ActionResponse<bool>> SavePaymentAsync(
        IEnumerable<SecretSettingDTO> items,
        string username);
}
