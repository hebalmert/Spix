using Spix.Domain.EntitiesEmails;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.AppService.InterfacesEmails;

public interface IEmailProviderSettingService
{
    Task<ActionResponse<IEnumerable<EmailProviderSetting>>> GetAsync(PaginationDTO pagination, string username);

    Task<ActionResponse<EmailProviderSetting>> GetAsync(Guid id);

    Task<ActionResponse<EmailProviderSetting>> AddAsync(EmailProviderSetting modelo, string username);

    Task<ActionResponse<EmailProviderSetting>> UpdateAsync(EmailProviderSetting modelo, string username);

    Task<ActionResponse<bool>> DeleteAsync(Guid id, string username);

    Task<ActionResponse<bool>> TestAsync(Guid id, string username);
}
