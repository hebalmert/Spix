using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesOper;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesContratos.ContracIDPicPage;

public partial class FormContractIDPic
{
    //Services

    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    //Parameters

    [Parameter, EditorRequired] public ContractIDPic ContractIDPic { get; set; } = null!;
    [Parameter, EditorRequired] public EventCallback OnSubmit { get; set; }
    [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
    [Parameter, EditorRequired] public bool IsEditControl { get; set; }
    [Parameter] public bool IsSaving { get; set; }

    private string? ImageUrlFront;
    private string? ImageUrlBack;

    protected override async Task OnInitializedAsync()
    {
        if (IsEditControl)
        {
            ImageUrlFront = ContractIDPic.ImageFrontFullPath;
            ImageUrlBack = ContractIDPic.ImageBackFullPath;
        }
    }

    private void ImageFrontSelected(string imagenBase64)
    {
        ContractIDPic.ImgFrontBase64 = imagenBase64;
        ImageUrlFront = null;
    }

    private void ImageBackSelected(string imagenBase64)
    {
        ContractIDPic.ImgBackBase64 = imagenBase64;
        ImageUrlBack = null;
    }
}