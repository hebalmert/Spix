using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppWpf.NetHelper;

// La comprobacion de conexion con un MikroTik, hecha DESDE ESTE EQUIPO.
//
// El escritorio es el agente local: se conecta por la red de la casa, no le pide al
// Backend que lo haga. Por eso esta clase vive aqui y no se comparte con la web, que no
// tiene otra forma que pasar por el servidor.
//
// Recibe los datos de conexion sueltos y no la entidad Server: el endpoint que entrega el
// servidor NO devuelve la relacion IpNetwork, asi que leer server.IpNetwork.Ip daba
// siempre nulo. La IP la trae el listado, que ya la muestra en pantalla.
public interface IMkConnectionControl
{
    Task<ActionResponse<MkConnectionResultDTO>> CheckConnectionAsync(
        string? ip,
        int apiPort,
        string? usuario,
        string? clave);
}
