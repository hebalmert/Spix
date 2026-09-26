using Spix.Domain.EntitiesNet;
using Spix.AppWpf.Services.Network.Models;

namespace Spix.AppWpf.Services.Network;

// Define operaciones MikroTik que se ejecutan desde la red local del equipo Windows.
public interface ILocalMikrotikService
{
    Task<LocalMikrotikConnectionResult> CheckConnectionAsync(
        Server server,
        CancellationToken cancellationToken = default);

    // El tiempo de espera cubre TODA la operacion, no solo la conexion. Por eso es un
    // parametro: una orden suelta se resuelve en segundos, pero un lote de contratos en
    // una sola conexion necesita mas. Sin pasarlo se mantiene el de siempre.
    Task<LocalMikrotikCommandResult> ExecuteAsync(
        Server server,
        Action<Spix.xNetwork.MkHelper.MK> action,
        CancellationToken cancellationToken = default,
        TimeSpan? timeout = null);
}
