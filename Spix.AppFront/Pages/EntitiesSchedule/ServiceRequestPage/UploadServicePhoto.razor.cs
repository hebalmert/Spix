using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.ServiceRequestPage;

//Sube UNA foto de la visita. La orden se entera al cerrarse el modal y recarga su lista.
public partial class UploadServicePhoto
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private const string BaseUrl = "/api/v1/servicerequestphotos";

    [Parameter, EditorRequired] public Guid ServiceRequestId { get; set; }

    private ServicePhotoType PhotoType = ServicePhotoType.Before;
    private string? ImageBase64;
    private bool IsSaving;

    private void ImageSelected(string imageBase64)
    {
        ImageBase64 = imageBase64;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ImageBase64))
        {
            await _sweetAlert.FireAsync(Localizer["Photo_Add"], Localizer["Photo_Empty"], SweetAlertIcon.Warning);
            return;
        }

        var dto = new ServiceRequestPhotoDto
        {
            ServiceRequestId = ServiceRequestId,
            PhotoType = PhotoType,
            ImgBase64 = ImageBase64
        };

        IsSaving = true;
        await InvokeAsync(StateHasChanged);

        var responseHttp = await _repository.PostAsync<ServiceRequestPhotoDto, ServiceRequestPhotoDto>(BaseUrl, dto);

        IsSaving = false;

        if (await _responseHandler.HandleErrorAsync(responseHttp))
            return;

        await _modalService.CloseAsync(ModalResult.Ok());
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
