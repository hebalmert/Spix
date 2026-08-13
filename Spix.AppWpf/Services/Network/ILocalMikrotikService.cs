using Spix.Domain.EntitiesNet;
using Spix.AppWpf.Services.Network.Models;

namespace Spix.AppWpf.Services.Network;

// Define operaciones MikroTik que se ejecutan desde la red local del equipo Windows.
public interface ILocalMikrotikService
{
    Task<LocalMikrotikConnectionResult> CheckConnectionAsync(
        Server server,
        CancellationToken cancellationToken = default);

    Task<LocalMikrotikCommandResult> ExecuteAsync(
        Server server,
        Action<Spix.xNetwork.MkHelper.MK> action,
        CancellationToken cancellationToken = default);
}
