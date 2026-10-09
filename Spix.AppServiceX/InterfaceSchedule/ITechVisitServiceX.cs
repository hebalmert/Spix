using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.InterfaceSchedule;

public interface ITechVisitServiceX
{
    Task<ActionResponse<IEnumerable<TechVisitDto>>> GetMineAsync(string username);

    Task<ActionResponse<TechVisitDto>> GetAsync(Guid id, string username);

    Task<ActionResponse<TechVisitDto>> StartAsync(Guid id, string username);

    //Las acciones ya viven en el servicio de la web y son las mismas: la app no se
    //inventa otra regla, solo entra por su propia puerta
    Task<ActionResponse<ServiceRequestDto>> CaptureLocationAsync(Guid id, decimal latitude, decimal longitude, string username);

    Task<ActionResponse<ServiceRequestDto>> CloseAsync(Guid id, string? comment, string? recommendation, string username);

    Task<ActionResponse<ServiceRequestDto>> NoClientAsync(Guid id, decimal latitude, decimal longitude, string? comment, string username);

    //La foto del trabajo y el servicio realizado: dos de las tres condiciones del cierre
    Task<ActionResponse<ServiceRequestPhotoDto>> AddPhotoAsync(ServiceRequestPhotoDto dto, string username);

    Task<ActionResponse<ServiceRequestDetailDto>> AddDetailAsync(ServiceRequestDetailDto dto, string username);

    Task<ActionResponse<bool>> DeleteDetailAsync(Guid id, string username);
}
