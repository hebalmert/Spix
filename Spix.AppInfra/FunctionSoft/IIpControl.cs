using Microsoft.EntityFrameworkCore.Storage;
using Spix.Core.EntitiesNet;
using Spix.DomainLogic.SpixResponse;

namespace Spix.AppInfra.FunctionSoft;

public interface IIpControl
{
    //Guid id = IpNetworkId, string Descrip = nodename, IDbContextTransaction para eredar el RollBack
    Task<ActionResponse<IpNetwork>> SelectIpWhenAdd(Guid id, string Descrip, IDbContextTransaction transaction);

    //Guid id = IpNetworkId, Guid IdNode = el NodeId para buscar si ipNetworkId en tabla es = al id que es el que viene en el modelo,
    //string Descrip = nodename, IDbContextTransaction para eredar el RollBack
    Task<ActionResponse<IpNetwork>> SelectIpWhenUpdate(Guid id, Guid IdNode, string Descrip, IDbContextTransaction transaction);

    //Guid id = IpNetworkId, Guid IdNode = el NodeId para buscar si ipNetworkId en tabla es = al id que es el que viene en el modelo,
    //string Descrip = nodename, IDbContextTransaction para eredar el RollBack
    Task<ActionResponse<IpNetwork>> SelectIpWhenUpdateServer(Guid id, Guid IdServer, string Descrip, IDbContextTransaction transaction);

    //Guid id = es el IpNetworkId que se eliminara y se dejara disponible en IpNetwork
    Task<ActionResponse<IpNetwork>> SelectIpToDelete(Guid id, IDbContextTransaction transaction);
}