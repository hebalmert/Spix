using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.InterfaceContratos.InterfaceContractControl;

// Le entrega al ESCRITORIO los datos que necesita para hablar el mismo con el MikroTik.
//
// Es de SOLO LECTURA y solo lo usa el WPF: no crea, no modifica y no toca el equipo. Vive
// aparte de ContractQueService y ContractBindService para no meterle mano a lo que ya
// funciona en produccion.
public interface IContractMkSetupService
{
    Task<ActionResponse<ContractQueSetupDTO>> GetQueSetupAsync(Guid contractClientId, string username);

    Task<ActionResponse<ContractBindSetupDTO>> GetBindSetupAsync(Guid contractClientId, string username);

    //Guardan lo que el escritorio YA escribio en el equipo: aqui no se toca el MikroTik
    Task<ActionResponse<ContractQue>> SaveQueAsync(ContractQueSaveDTO datos, string username);

    Task<ActionResponse<bool>> RemoveQueAsync(ContractQueRemoveDTO datos, string username);

    Task<ActionResponse<ContractBind>> SaveBindAsync(ContractBind modelo, string username);

    Task<ActionResponse<bool>> RemoveBindAsync(Guid id, string username);

    //Para quitar la Queue y para editar o quitar el IpBinding
    Task<ActionResponse<ContractQueRemoveSetupDTO>> GetQueRemoveSetupAsync(Guid contractQueId, string username);

    Task<ActionResponse<ContractMkConnectionDTO>> GetConnectionAsync(Guid contractClientId, string username);
}
