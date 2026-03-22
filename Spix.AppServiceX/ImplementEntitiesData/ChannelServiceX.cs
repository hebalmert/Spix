using Spix.AppService.InterfacesEntitiesData;
using Spix.AppServiceX.InterfacesEntitiesData;
using Spix.Domain.EntitiesData;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;

namespace Spix.UnitOfWork.ImplementEntitiesData;

public class ChannelServiceX : IChannelServiceX
{
    private readonly IChannelService _channelService;

    public ChannelServiceX(IChannelService channelService)
    {
        _channelService = channelService;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync() => await _channelService.ComboAsync();

    public async Task<ActionResponse<IEnumerable<Channel>>> GetAsync(PaginationDTO pagination) => await _channelService.GetAsync(pagination);

    public async Task<ActionResponse<Channel>> GetAsync(int id) => await _channelService.GetAsync(id);

    public async Task<ActionResponse<Channel>> UpdateAsync(Channel modelo) => await _channelService.UpdateAsync(modelo);

    public async Task<ActionResponse<Channel>> AddAsync(Channel modelo) => await _channelService.AddAsync(modelo);

    public async Task<ActionResponse<bool>> DeleteAsync(int id) => await _channelService.DeleteAsync(id);
}