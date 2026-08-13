using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.Shared;

// Reutiliza carga, guardado, cancelacion y manejo HTTP para formularios CRUD sencillos.
public abstract partial class CrudFormViewModel<T> : ObservableObject
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private T _entity;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isLoading;

    protected CrudFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
    {
        _repository = repository;
        _modalService = modalService;
        _responseHandler = responseHandler;
        _alertService = alertService;
        Entity = CreateEntity();
    }

    protected abstract string BaseUrl { get; }

    protected abstract T CreateEntity();

    protected abstract string? GetValidationMessage();

    public async Task LoadAsync(Guid id)
    {
        IsLoading = true;

        try
        {
            var responseHttp = await _repository.GetAsync<T>($"{BaseUrl}/{id}");

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            Entity = responseHttp.Response ?? CreateEntity();
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync("Error de conexion", exception.Message);
            await _modalService.CloseAsync(ModalResult.Cancel());
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task SaveChangesAsync(bool isEdit)
    {
        var validationMessage = GetValidationMessage();
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            await _alertService.WarningAsync("Campo requerido", validationMessage);
            return;
        }

        IsSaving = true;

        try
        {
            var responseHttp = isEdit
                ? await _repository.PutAsync(BaseUrl, Entity)
                : await _repository.PostAsync(BaseUrl, Entity);

            if (await _responseHandler.HandleErrorAsync(responseHttp))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync("Error de conexion", exception.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
