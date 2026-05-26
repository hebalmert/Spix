using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Spix.AppFront.GenericModel;
using Spix.AppFront.Helper;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using Spix.xLanguage.Resources;

namespace Spix.AppFront.Pages.EntitiesGen.MarkPage;

public partial class DetailedIndexMark
{
    [Inject] private IStringLocalizer<Resource> Localizer { get; set; } = null!;
    [Inject] private IRepository _repository { get; set; } = null!;
    [Inject] private NavigationManager _navigationManager { get; set; } = null!;
    [Inject] private ModalService _modalService { get; set; } = null!;
    [Inject] private SweetAlertService _sweetAlert { get; set; } = null!;
    [Inject] private HttpResponseHandler _responseHandler { get; set; } = null!;

    private string Filter { get; set; } = string.Empty;
    private int CurrentPage = 1;
    private int TotalPages;
    private int PageSize = 15;

    private const string baseUrlMarks = "api/v1/marks";
    private const string baseUrlMarkModels = "api/v1/marksmodels";

    public List<Mark>? Marks { get; set; }
    public Dictionary<Guid, List<MarkModel>> MarkModelsByMarkId { get; set; } = new();
    public Guid? ExpandedMarkId { get; set; } = null; // Acordeón: solo 1 Mark expandido
    public HashSet<Guid> LoadingMarkIds { get; set; } = new(); // Track de cargas en progreso

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadMarks();
        }
    }

    /// <summary>
    /// Alterna la expansión de un Mark para mostrar/ocultar sus modelos (Acordeón: solo 1 expandido)
    /// </summary>
    private async Task ToggleExpandedMark(Guid markId)
    {
        if (ExpandedMarkId == markId)
        {
            // Si ya está expandido, lo cerramos
            ExpandedMarkId = null;
        }
        else
        {
            // Cerramos el anterior y abrimos el nuevo
            ExpandedMarkId = markId;

            // Cargar modelos si no están cargados
            if (!MarkModelsByMarkId.ContainsKey(markId))
            {
                await LoadMarkModelsForMark(markId);
            }
        }
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Carga los Marks paginados
    /// </summary>
    private async Task LoadMarks(int page = 1)
    {
        var url = $"{baseUrlMarks}?page={page}&recordsnumber={PageSize}";
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&filter={Filter}";
        }

        var responseHttp = await _repository.GetAsync<List<Mark>>(url);
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandled)
        {
            _navigationManager.NavigateTo("/");
            return;
        }

        Marks = responseHttp.Response;
        TotalPages = int.Parse(responseHttp.HttpResponseMessage.Headers.GetValues("Totalpages").FirstOrDefault()!);
        ExpandedMarkId = null; // Cerrar acordeón al cambiar página
        MarkModelsByMarkId.Clear();
        LoadingMarkIds.Clear();

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Carga los MarkModels para un Mark específico
    /// </summary>
    private async Task LoadMarkModelsForMark(Guid markId)
    {
        LoadingMarkIds.Add(markId);
        await InvokeAsync(StateHasChanged);

        var url = $"{baseUrlMarkModels}?guidId={markId}";
        var responseHttp = await _repository.GetAsync<List<MarkModel>>(url);
        bool errorHandled = await _responseHandler.HandleErrorAsync(responseHttp);

        LoadingMarkIds.Remove(markId);

        if (errorHandled)
        {
            await InvokeAsync(StateHasChanged);
            return;
        }

        MarkModelsByMarkId[markId] = responseHttp.Response ?? new List<MarkModel>();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Maneja cambio de página
    /// </summary>
    private async Task SelectedPage(int page)
    {
        CurrentPage = page;
        await LoadMarks(page);
    }

    /// <summary>
    /// Aplica filtro y recarga la lista
    /// </summary>
    private async Task SetFilterValue(string value)
    {
        Filter = value;
        CurrentPage = 1;
        await LoadMarks();
    }

    /// <summary>
    /// Abre modal para crear un nuevo Mark
    /// </summary>
    private async Task ShowModalCreateMarkAsync()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Title", $"{Localizer[nameof(Resource.Create_Mark)]}" }
        };

        await _modalService.ShowAsync(typeof(CreateMark), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await LoadMarks(CurrentPage);
                await _sweetAlert.FireAsync(
                    Localizer[nameof(Resource.msg_SuccessTitle)],
                    Localizer[nameof(Resource.msg_SuccessMessage)],
                    SweetAlertIcon.Success
                );
            }
        });
    }

    /// <summary>
    /// Abre modal para editar un Mark existente
    /// </summary>
    private async Task ShowModalEditMarkAsync(Mark mark)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", mark.MarkId },
            { "Title", $"{Localizer[nameof(Resource.Edit_Mark)]}" }
        };

        await _modalService.ShowAsync(typeof(EditMark), parameters, async result =>
        {
            if (result.Succeeded)
            {
                await LoadMarks(CurrentPage);
                await _sweetAlert.FireAsync(
                    Localizer[nameof(Resource.msg_SuccessTitle)],
                    Localizer[nameof(Resource.msg_SuccessMessage)],
                    SweetAlertIcon.Success
                );
            }
        });
    }

    /// <summary>
    /// Abre modal para crear un nuevo MarkModel
    /// </summary>
    private async Task ShowModalCreateMarkModelAsync(Guid markId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", markId },
            { "Title", $"{Localizer[nameof(Resource.Create_Mark)]} - {Localizer[nameof(Resource.Model)]}" }
        };

        await _modalService.ShowAsync(typeof(CreateMarkModel), parameters, async result =>
        {
            if (result.Succeeded)
            {
                // Recarga modelos del Mark específico
                MarkModelsByMarkId.Remove(markId);
                await LoadMarkModelsForMark(markId);
                await _sweetAlert.FireAsync(
                    Localizer[nameof(Resource.msg_SuccessTitle)],
                    Localizer[nameof(Resource.msg_SuccessMessage)],
                    SweetAlertIcon.Success
                );
            }
        });
    }

    /// <summary>
    /// Abre modal para editar un MarkModel existente
    /// </summary>
    private async Task ShowModalEditMarkModelAsync(MarkModel model)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Id", model.MarkModelId },
            { "Title", $"{Localizer[nameof(Resource.Edit_Mark)]} - {Localizer[nameof(Resource.Model)]}" }
        };

        await _modalService.ShowAsync(typeof(EditMarkModel), parameters, async result =>
        {
            if (result.Succeeded)
            {
                // Recarga modelos del Mark específico
                MarkModelsByMarkId.Remove(model.MarkId);
                await LoadMarkModelsForMark(model.MarkId);
                await _sweetAlert.FireAsync(
                    Localizer[nameof(Resource.msg_SuccessTitle)],
                    Localizer[nameof(Resource.msg_SuccessMessage)],
                    SweetAlertIcon.Success
                );
            }
        });
    }

    /// <summary>
    /// Elimina un Mark
    /// </summary>
    private async Task DeleteMarkAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{baseUrlMarks}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        await _sweetAlert.FireAsync(
            Localizer[nameof(Resource.msg_DeleteConfirmationTitle)],
            Localizer[nameof(Resource.msg_DeleteConfirmationText)],
            SweetAlertIcon.Success
        );
        await LoadMarks(CurrentPage);
    }

    /// <summary>
    /// Elimina un MarkModel
    /// </summary>
    private async Task DeleteMarkModelAsync(Guid id)
    {
        var result = await _sweetAlert.FireAsync(new SweetAlertOptions
        {
            Title = Localizer[nameof(Resource.msg_DeleteTitle)],
            Text = Localizer[nameof(Resource.msg_DeleteMessage)],
            Icon = SweetAlertIcon.Question,
            ShowCancelButton = true,
            ConfirmButtonText = Localizer[nameof(Resource.msg_DeleteConfirmButton)],
            CancelButtonText = Localizer[nameof(Resource.ButtonCancel)]
        });

        if (result.IsDismissed || result.Value != "true")
            return;

        var responseHttp = await _repository.DeleteAsync($"{baseUrlMarkModels}/{id}");
        var errorHandler = await _responseHandler.HandleErrorAsync(responseHttp);
        if (errorHandler)
            return;

        // Encontrar el MarkId del modelo eliminado y recargar
        var markIdToReload = MarkModelsByMarkId
            .FirstOrDefault(x => x.Value.Any(m => m.MarkModelId == id)).Key;

        if (markIdToReload != Guid.Empty)
        {
            MarkModelsByMarkId.Remove(markIdToReload);
            await LoadMarkModelsForMark(markIdToReload);
        }

        await _sweetAlert.FireAsync(
            Localizer[nameof(Resource.msg_DeleteConfirmationTitle)],
            Localizer[nameof(Resource.msg_DeleteConfirmationText)],
            SweetAlertIcon.Success
        );
    }
}
