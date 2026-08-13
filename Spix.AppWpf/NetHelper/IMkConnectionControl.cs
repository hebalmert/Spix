using Spix.Domain.EntitiesNet;
using Spix.DomainLogic.MkDTOs;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppWpf.NetHelper;

// Define la validacion local de conectividad y autenticacion contra un servidor MikroTik.
public interface IMkConnectionControl
{
    Task<ActionResponse<MkConnectionResultDTO>> CheckConnectionAsync(Server server);
}
