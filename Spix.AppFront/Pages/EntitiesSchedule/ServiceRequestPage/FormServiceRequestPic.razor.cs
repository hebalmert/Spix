using Microsoft.AspNetCore.Components;
using Spix.Domain.EntitiesSchedule;

namespace Spix.AppFront.Pages.EntitiesSchedule.ServiceRequestPage;

public partial class FormServiceRequestPic
{
    [Parameter, EditorRequired] public ServiceRequestPic Model { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsCompleted { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private bool ShowBefore = true;
    private int CurrentPhotoIndex;
    private string FrontKey => ShowBefore ? "before1" : "after1";
    private string BackKey => ShowBefore ? "before2" : "after2";
    private List<PhotoItem> Photos => new()
    {
        new("Antes 1", Model.ImageBefore1FullPath),
        new("Antes 2", Model.ImageBefore2FullPath),
        new("Despues 1", Model.ImageAfter1FullPath),
        new("Despues 2", Model.ImageAfter2FullPath)
    };
    private PhotoItem CurrentPhoto => Photos[CurrentPhotoIndex];

    private void ShowBeforePics() => ShowBefore = true;
    private void ShowAfterPics() => ShowBefore = false;
    private void PreviousPhoto() => CurrentPhotoIndex = CurrentPhotoIndex == 0 ? Photos.Count - 1 : CurrentPhotoIndex - 1;
    private void NextPhoto() => CurrentPhotoIndex = CurrentPhotoIndex == Photos.Count - 1 ? 0 : CurrentPhotoIndex + 1;
    private void SelectPhoto(int index) => CurrentPhotoIndex = index;

    private void Before1Selected(string imageBase64) => Model.ImgBefore1Base64 = imageBase64;
    private void Before2Selected(string imageBase64) => Model.ImgBefore2Base64 = imageBase64;
    private void After1Selected(string imageBase64) => Model.ImgAfter1Base64 = imageBase64;
    private void After2Selected(string imageBase64) => Model.ImgAfter2Base64 = imageBase64;

    private record PhotoItem(string Title, string? ImageUrl);
}
