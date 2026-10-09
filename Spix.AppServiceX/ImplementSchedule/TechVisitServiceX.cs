using Spix.AppService.InterfaceSchedule;
using Spix.AppServiceX.InterfaceSchedule;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppServiceX.ImplementSchedule;

public class TechVisitServiceX : ITechVisitServiceX
{
    private readonly ITechVisitService _techVisitService;
    private readonly IServiceRequestService _serviceRequestService;
    private readonly IServiceRequestPhotoService _photoService;
    private readonly IServiceRequestDetailService _detailService;

    public TechVisitServiceX(ITechVisitService techVisitService,
        IServiceRequestService serviceRequestService,
        IServiceRequestPhotoService photoService,
        IServiceRequestDetailService detailService)
    {
        _techVisitService = techVisitService;
        _serviceRequestService = serviceRequestService;
        _photoService = photoService;
        _detailService = detailService;
    }

    public async Task<ActionResponse<IEnumerable<TechVisitDto>>> GetMineAsync(string username) =>
        await _techVisitService.GetMineAsync(username);

    public async Task<ActionResponse<TechVisitDto>> GetAsync(Guid id, string username) =>
        await _techVisitService.GetAsync(id, username);

    public async Task<ActionResponse<TechVisitDto>> StartAsync(Guid id, string username) =>
        await _techVisitService.StartAsync(id, username);

    public async Task<ActionResponse<ServiceRequestDto>> CaptureLocationAsync(Guid id, decimal latitude, decimal longitude, string username) =>
        await _serviceRequestService.CaptureLocationAsync(id, latitude, longitude, username);

    public async Task<ActionResponse<ServiceRequestDto>> CloseAsync(Guid id, string? comment, string? recommendation, string username) =>
        await _serviceRequestService.CloseAsync(id, comment, recommendation, username);

    public async Task<ActionResponse<ServiceRequestDto>> NoClientAsync(Guid id, decimal latitude, decimal longitude, string? comment, string username) =>
        await _serviceRequestService.NoClientAsync(id, latitude, longitude, comment, username);

    public async Task<ActionResponse<ServiceRequestPhotoDto>> AddPhotoAsync(ServiceRequestPhotoDto dto, string username) =>
        await _photoService.AddPhotoAsync(dto, username);

    public async Task<ActionResponse<ServiceRequestDetailDto>> AddDetailAsync(ServiceRequestDetailDto dto, string username) =>
        await _detailService.AddDetailAsync(dto, username);

    public async Task<ActionResponse<bool>> DeleteDetailAsync(Guid id, string username) =>
        await _detailService.DeleteDetailAsync(id, username);
}
