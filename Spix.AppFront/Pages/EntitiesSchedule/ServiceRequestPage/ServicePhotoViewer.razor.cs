using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.Domain.EntitiesSchedule;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.ServiceRequestPage;

//Carrusel de las fotos de la visita. Solo muestra: no sube ni borra.
public partial class ServicePhotoViewer
{
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    [Parameter, EditorRequired] public List<ServiceRequestPhotoDto> Photos { get; set; } = new();

    //Con cual se abrio: la que toco el usuario
    [Parameter] public Guid StartId { get; set; }

    private int CurrentIndex;

    private ServiceRequestPhotoDto Current => Photos[CurrentIndex];

    protected override void OnInitialized()
    {
        var index = Photos.FindIndex(x => x.ServiceRequestPhotoId == StartId);
        CurrentIndex = index < 0 ? 0 : index;
    }

    private void Previous()
    {
        CurrentIndex = CurrentIndex == 0 ? Photos.Count - 1 : CurrentIndex - 1;
    }

    private void Next()
    {
        CurrentIndex = CurrentIndex == Photos.Count - 1 ? 0 : CurrentIndex + 1;
    }

    private async Task Return()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
