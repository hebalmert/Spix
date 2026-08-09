using Spix.AppService.InterfacesSaaS;
using Spix.AppServiceX.InterfacesSaaS;
using Spix.DomainLogic.EntitiesSaaSDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementSaaS;

public class SystemSettingServiceX : ISystemSettingServiceX
{
    private readonly ISystemSettingService _service;

    public SystemSettingServiceX(ISystemSettingService service)
    {
        _service = service;
    }

    public Task<ActionResponse<IEnumerable<SecretSettingDTO>>> GetSystemAsync()
    {
        return _service.GetSystemAsync();
    }

    public Task<ActionResponse<IEnumerable<SecretSettingDTO>>> GetPaymentAsync()
    {
        return _service.GetPaymentAsync();
    }

    public Task<ActionResponse<bool>> SaveSystemAsync(
        IEnumerable<SecretSettingDTO> items,
        string username)
    {
        return _service.SaveSystemAsync(items, username);
    }

    public Task<ActionResponse<bool>> SavePaymentAsync(
        IEnumerable<SecretSettingDTO> items,
        string username)
    {
        return _service.SavePaymentAsync(items, username);
    }
}
