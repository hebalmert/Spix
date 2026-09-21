using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesSchedule.ServiceRequestPage;

//Cierra la solicitud sin visita. Lo que se escribe aqui es lo unico que el cliente
//va a ver de la resolucion, por eso son dos campos y no un texto suelto.
public partial class ResolveByPhone
{
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;

    private const string BaseUrl = "/api/v1/servicerequests";

    [Parameter, EditorRequired] public Guid ServiceRequestId { get; set; }

    private string? Comment;
    private string? Recommendation;
    private bool IsSaving;

    private void CommentChanged(ChangeEventArgs e)
    {
        Comment = e.Value?.ToString();
    }

    private void RecommendationChanged(ChangeEventArgs e)
    {
        Recommendation = e.Value?.ToString();
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Comment))
        {
            await _sweetAlert.FireAsync(
                Localizer["Request_ByPhone"],
                Localizer["Request_PhoneNeedsComment"],
                SweetAlertIcon.Warning);
            return;
        }

        IsSaving = true;
        await InvokeAsync(StateHasChanged);

        var url = $"{BaseUrl}/{ServiceRequestId}/resolvebyphone" +
                  $"?comment={Uri.EscapeDataString(Comment.Trim())}" +
                  $"&recommendation={Uri.EscapeDataString(Recommendation?.Trim() ?? string.Empty)}";

        var responseHttp = await _repository.PostAsync<object, ServiceRequestDto>(url, new { });

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
