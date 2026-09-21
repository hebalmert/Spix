using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.Domain.EntitiesSchedule;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.MyServiceRequestPage;

//Solo muestra. Todo lo que ve el cliente ya viene recortado desde el backend.
public partial class MyServiceRequestResult
{
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    [Parameter, EditorRequired] public MyServiceRequestItemDto Item { get; set; } = null!;

    private MyServiceRequestPhotoDto? Zoomed;

    private void Zoom(MyServiceRequestPhotoDto photo)
    {
        Zoomed = photo;
    }

    private void CloseZoom()
    {
        Zoomed = null;
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
