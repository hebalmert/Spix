namespace Spix.xNetwork.IpHelper;

//Reserva y suelta las IP de red (IpNetwork) que usan los nodos y los servidores.
//No guarda ni maneja la transaccion: eso lo hace el servicio que la llama.
public interface IIpControl
{
    //Marca la IP como asignada al equipo. Si antes tenia otra, la suelta.
    //Devuelve false si la IP no es de la corporacion, esta inactiva, excluida o la usa otro equipo.
    Task<bool> AssignAsync(Guid ipNetworkId, Guid? previousIpNetworkId, string description, int corporationId);

    //Deja la IP libre otra vez
    Task ReleaseAsync(Guid ipNetworkId, int corporationId);
}
