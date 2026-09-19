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
    public Guid? SelectedMarkId { get; set; }
    public HashSet<Guid> LoadingMarkIds { get; set; } = new();

    //Filtros del panel de modelos (se resuelven en pantalla, sin volver al servidor)
    private string ModelChip { get; set; } = "all";
    private string ModelFilter { get; set; } = string.Empty;

    private Mark? SelectedMark => Marks?.FirstOrDefault(x => x.MarkId == SelectedMarkId);

    private List<MarkModel> SelectedMarkModels =>
        SelectedMarkId is not null && MarkModelsByMarkId.TryGetValue(SelectedMarkId.Value, out var models)
            ? models
            : new List<MarkModel>();

    private int ActiveModelsCount => SelectedMarkModels.Count(x => x.Active);

    private int InactiveModelsCount => SelectedMarkModels.Count(x => !x.Active);

    private List<MarkModel> FilteredMarkModels
    {
        get
        {
            var list = SelectedMarkModels.AsEnumerable();

            list = ModelChip switch
            {
                "active" => list.Where(x => x.Active),
                "inactive" => list.Where(x => !x.Active),
                _ => list
            };

            return list.ToList();
        }
    }

    private void SetModelChip(string chip) => ModelChip = chip;

    //El texto va al servidor: asi busca en TODOS los modelos de la marca, no solo en los cargados
    private async Task SetModelFilter(string value)
    {
        ModelFilter = value;

        if (SelectedMarkId is not null)
        {
            await LoadMarkModelsForMark(SelectedMarkId.Value);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadMarks();
        }
    }

    /// <summary>
    /// Selecciona una marca y carga sus modelos si aun no estan en memoria
    /// </summary>
    private async Task SelectMarkAsync(Guid markId)
    {
        SelectedMarkId = markId;
        ModelChip = "all";
        ModelFilter = string.Empty;

        if (!MarkModelsByMarkId.ContainsKey(markId))
        {
            await LoadMarkModelsForMark(markId);
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

        MarkModelsByMarkId.Clear();
        LoadingMarkIds.Clear();

        //Se conserva la marca elegida si sigue en la lista; si no, se toma la primera
        var previous = SelectedMarkId;
        SelectedMarkId = Marks?.Any(x => x.MarkId == previous) == true
            ? previous
            : Marks?.FirstOrDefault()?.MarkId;

        await InvokeAsync(StateHasChanged);

        if (SelectedMarkId is not null)
        {
            await LoadMarkModelsForMark(SelectedMarkId.Value);
        }
    }

    /// <summary>
    /// Carga los MarkModels para un Mark especifico
    /// </summary>
    private async Task LoadMarkModelsForMark(Guid markId)
    {
        LoadingMarkIds.Add(markId);
        await InvokeAsync(StateHasChanged);

        var url = $"{baseUrlMarkModels}?guidId={markId}";
        if (!string.IsNullOrWhiteSpace(ModelFilter))
        {
            url += $"&filter={Uri.EscapeDataString(ModelFilter)}";
        }
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
    /// Maneja cambio de pagina
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
                // Recarga modelos del Mark especifico
                MarkModelsByMarkId.Remove(markId);
                SelectedMarkId = markId;
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
                // Recarga modelos del Mark especifico
                MarkModelsByMarkId.Remove(model.MarkId);
                SelectedMarkId = model.MarkId;
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

        if (SelectedMarkId == id)
        {
            SelectedMarkId = null;
        }

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
