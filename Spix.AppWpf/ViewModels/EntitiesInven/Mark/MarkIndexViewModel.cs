using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesInven.Mark;
using Spix.HttpService;
using System.Collections.ObjectModel;
using MarkModelEntity = Spix.Domain.EntitiesGen.MarkModel;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Mark;

// Replica el indice expandible de Marcas y Modelos construido en Blazor.
public partial class MarkIndexViewModel : PagedListViewModel<MarkRowViewModel>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/marks";

    public MarkIndexViewModel(
        IPagedEntityService<MarkRowViewModel> pagedEntityService,
        IRepository repository,
        ModalService modalService,
        AlertService alertService,
        HttpResponseHandler responseHandler)
        : base(pagedEntityService)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
    }

    [RelayCommand]
    private async Task ToggleModelsAsync(MarkRowViewModel? mark)
    {
        if (mark is null) return;
        if (mark.IsExpanded)
        {
            mark.IsExpanded = false;
            return;
        }

        foreach (var item in Items) item.IsExpanded = false;
        mark.IsExpanded = true;
        await LoadModelsAsync(mark);
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateMarkDialogView>("Crear marca");
        if (!result.Succeeded) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La marca fue guardada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(MarkRowViewModel? mark)
    {
        if (mark is null) return;
        var result = await _modalService.ShowAsync<EditMarkDialogView>(
            "Editar marca",
            new Dictionary<string, object> { ["Id"] = mark.MarkId });
        if (!result.Succeeded) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "La marca fue actualizada correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(MarkRowViewModel? mark)
    {
        if (mark is null) return;
        var confirmed = await _alertService.ConfirmAsync("Eliminar marca", "Esta accion no se puede deshacer.", "Eliminar");
        if (!confirmed) return;
        var response = await _repository.DeleteAsync($"api/v1/marks/{mark.MarkId}");
        if (await _responseHandler.HandleErrorAsync(response)) return;
        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La marca fue eliminada correctamente.");
    }

    [RelayCommand]
    private async Task NewModelAsync(MarkRowViewModel? mark)
    {
        if (mark is null) return;
        var result = await _modalService.ShowAsync<CreateMarkModelDialogView>(
            "Crear modelo",
            new Dictionary<string, object> { ["MarkId"] = mark.MarkId });
        if (!result.Succeeded) return;
        await ReloadMarkAsync(mark.MarkId);
        await _alertService.SuccessAsync("Guardado", "El modelo fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditModelAsync(MarkModelEntity? model)
    {
        if (model is null) return;
        var result = await _modalService.ShowAsync<EditMarkModelDialogView>(
            "Editar modelo",
            new Dictionary<string, object> { ["Id"] = model.MarkModelId });
        if (!result.Succeeded) return;
        await ReloadMarkAsync(model.MarkId);
        await _alertService.SuccessAsync("Actualizado", "El modelo fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteModelAsync(MarkModelEntity? model)
    {
        if (model is null) return;
        var confirmed = await _alertService.ConfirmAsync("Eliminar modelo", "Esta accion no se puede deshacer.", "Eliminar");
        if (!confirmed) return;
        var response = await _repository.DeleteAsync($"api/v1/marksmodels/{model.MarkModelId}");
        if (await _responseHandler.HandleErrorAsync(response)) return;
        await ReloadMarkAsync(model.MarkId);
        await _alertService.SuccessAsync("Eliminado", "El modelo fue eliminado correctamente.");
    }

    private async Task LoadModelsAsync(MarkRowViewModel mark)
    {
        mark.IsModelsLoading = true;
        mark.ModelsMessage = string.Empty;
        try
        {
            var response = await _repository.GetAsync<List<MarkModelEntity>>(
                $"api/v1/marksmodels?guidId={mark.MarkId}&page=1&recordsnumber=100");
            if (await _responseHandler.HandleErrorAsync(response)) return;
            mark.Models = new ObservableCollection<MarkModelEntity>(response.Response ?? new List<MarkModelEntity>());
            if (mark.Models.Count == 0) mark.ModelsMessage = "No hay modelos registrados para esta marca.";
        }
        catch (Exception exception)
        {
            mark.ModelsMessage = exception.Message;
        }
        finally
        {
            mark.IsModelsLoading = false;
        }
    }

    private async Task ReloadMarkAsync(Guid markId)
    {
        var mark = Items.FirstOrDefault(item => item.MarkId == markId);
        if (mark is null)
        {
            await LoadAsync(CurrentPage);
            return;
        }

        mark.IsExpanded = true;
        await LoadModelsAsync(mark);
    }
}
