using Spix.AppService.InterfacesEmails;
using Spix.AppServiceX.InterfacesEmails;
using Spix.Domain.EntitiesEmails;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppServiceX.ImplementEmails;

public class EmailProviderSettingServiceX : IEmailProviderSettingServiceX
{
    private readonly IEmailProviderSettingService _emailProviderSettingService;

    public EmailProviderSettingServiceX(IEmailProviderSettingService emailProviderSettingService)
    {
        _emailProviderSettingService = emailProviderSettingService;
    }

    public async Task<ActionResponse<IEnumerable<EmailProviderSetting>>> GetAsync(PaginationDTO pagination, string username)
        => await _emailProviderSettingService.GetAsync(pagination, username);

    public async Task<ActionResponse<EmailProviderSetting>> GetAsync(Guid id)
        => await _emailProviderSettingService.GetAsync(id);

    public async Task<ActionResponse<EmailProviderSetting>> AddAsync(EmailProviderSetting modelo, string username)
        => await _emailProviderSettingService.AddAsync(modelo, username);

    public async Task<ActionResponse<EmailProviderSetting>> UpdateAsync(EmailProviderSetting modelo, string username)
        => await _emailProviderSettingService.UpdateAsync(modelo, username);

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
        => await _emailProviderSettingService.DeleteAsync(id, username);

    public async Task<ActionResponse<bool>> TestAsync(Guid id, string username)
        => await _emailProviderSettingService.TestAsync(id, username);
}
