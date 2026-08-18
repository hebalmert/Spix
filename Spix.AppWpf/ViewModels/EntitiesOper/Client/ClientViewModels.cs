using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesOper.Client;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesOper;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using ClientEntity = Spix.Domain.EntitiesOper.Client;

namespace Spix.AppWpf.ViewModels.EntitiesOper.Client;

// Lista los clientes con la misma paginacion, correo de activacion y CRUD de Blazor.
public partial class ClientIndexViewModel : PagedListViewModel<ClientEntity>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/clients";

    public ClientIndexViewModel(
        IPagedEntityService<ClientEntity> pagedEntityService,
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
    private async Task NewAsync()
    {
        ModalResult result = await _modalService.ShowAsync<CreateClientDialogView>("Crear cliente");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El cliente fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ClientEntity? client)
    {
        if (client == null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = client.ClientId
        };

        ModalResult result = await _modalService.ShowAsync<EditClientDialogView>("Editar cliente", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El cliente fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(ClientEntity? client)
    {
        if (client == null)
        {
            return;
        }

        bool confirmed = await _alertService.ConfirmAsync(
            "Eliminar cliente",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{client.ClientId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El cliente fue eliminado correctamente.");
    }

    // Reutiliza el endpoint existente para reenviar una cuenta aun no activada.
    [RelayCommand]
    private async Task ResendActivationEmailAsync(ClientEntity? client)
    {
        if (client == null || !client.CreateAccount)
        {
            return;
        }

        var response = await _repository.PostAsync($"{Endpoint}/{client.ClientId}/re-email", new { });
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await _alertService.SuccessAsync("Re-Email", "Correo de activacion enviado correctamente.");
    }
}

// Comparte carga de documento, foto y guardado para crear y editar clientes.
public abstract partial class ClientFormViewModel : ObservableObject
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ClientEntity _entity = new();

    [ObservableProperty]
    private ObservableCollection<DocumentType> _documentTypes = new();

    [ObservableProperty]
    private BitmapImage? _previewImage;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    public bool CanCreateAccount => Entity.Active;

    partial void OnEntityChanged(ClientEntity value)
    {
        OnPropertyChanged(nameof(CanCreateAccount));
    }

    protected ClientFormViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
    }

    // Carga el mismo combo de tipos de documento que utiliza FormClient en Blazor.
    public async Task InitializeForCreateAsync()
    {
        IsLoading = true;

        try
        {
            Entity = new ClientEntity
            {
                Active = true,
                CreateAccount = true
            };

            PreviewImage = null;
            await LoadDocumentTypesAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Recupera el cliente individual y mantiene su fotografia mientras se edita.
    public async Task InitializeForEditAsync(Guid id)
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<ClientEntity>($"api/v1/clients/{id}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            Entity = response.Response ?? new ClientEntity();
            PreviewImage = CreatePreviewImage(Entity.ImageFullPath);
            await LoadDocumentTypesAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Deja el modelo listo para que el Backend conserve su misma logica de guardado.
    protected async Task SaveChangesAsync(bool isEdit)
    {
        if (!TryValidate(out string? message))
        {
            await _alertService.WarningAsync("Campo requerido", message!);
            return;
        }

        if (!Entity.Active)
        {
            Entity.CreateAccount = false;
        }

        Entity.DocumentType = null;
        Entity.Corporation = null;
        Entity.ContractClients = null;
        Entity.ContractSuspendedAudits = null;

        IsSaving = true;

        try
        {
            var response = isEdit
                ? await _repository.PutAsync("api/v1/clients", Entity)
                : await _repository.PostAsync("api/v1/clients", Entity);

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    // Convierte la foto elegida localmente al mismo Base64 que recibe el Backend web.
    public void SelectPhoto(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return;
        }

        byte[] bytes = File.ReadAllBytes(filePath);
        Entity.ImgBase64 = Convert.ToBase64String(bytes);
        PreviewImage = CreatePreviewImage(filePath);
    }

    // Mantiene la regla: un cliente inactivo no puede crear ni conservar cuenta.
    public void SetActive(bool active)
    {
        Entity.Active = active;
        if (!active)
        {
            Entity.CreateAccount = false;
        }

        OnPropertyChanged(nameof(CanCreateAccount));
        OnPropertyChanged(nameof(Entity));
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }

    private async Task LoadDocumentTypesAsync()
    {
        var response = await _repository.GetAsync<List<DocumentType>>(
            "api/v1/combosData/ComboDocumentType");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        DocumentTypes = new ObservableCollection<DocumentType>(response.Response ?? new List<DocumentType>());
        if (Entity.DocumentTypeId == Guid.Empty && DocumentTypes.Count > 0)
        {
            Entity.DocumentTypeId = DocumentTypes[0].DocumentTypeId;
            OnPropertyChanged(nameof(Entity));
        }
    }

    private static BitmapImage? CreatePreviewImage(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(source, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }

    private bool TryValidate(out string? message)
    {
        if (string.IsNullOrWhiteSpace(Entity.FirstName) || string.IsNullOrWhiteSpace(Entity.LastName))
        {
            message = "Debes ingresar el nombre y el apellido del cliente.";
            return false;
        }

        if (Entity.DocumentTypeId == Guid.Empty || string.IsNullOrWhiteSpace(Entity.Document))
        {
            message = "Debes seleccionar el tipo de documento e ingresar el documento.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Entity.PhoneNumber) || string.IsNullOrWhiteSpace(Entity.Address))
        {
            message = "Debes ingresar el telefono y la direccion del cliente.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Entity.Email) || string.IsNullOrWhiteSpace(Entity.UserName))
        {
            message = "Debes ingresar el correo y el usuario del cliente.";
            return false;
        }

        message = null;
        return true;
    }
}

// Inicializa un cliente activo con cuenta habilitada, como CreateClient de Blazor.
public partial class CreateClientDialogViewModel : ClientFormViewModel
{
    public CreateClientDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
        : base(repository, responseHandler, modalService, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

// Mantiene el mismo cliente existente para edicion y reenvio individual de activacion.
public partial class EditClientDialogViewModel : ClientFormViewModel
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private bool _isSendingEmail;

    public EditClientDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
        : base(repository, responseHandler, modalService, alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _alertService = alertService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }

    // Reenvia activacion desde la edicion igual que EditClient en Blazor.
    [RelayCommand]
    private async Task ResendActivationEmailAsync()
    {
        if (Entity.ClientId == Guid.Empty || IsSendingEmail)
        {
            return;
        }

        IsSendingEmail = true;

        try
        {
            var response = await _repository.PostAsync($"api/v1/clients/{Entity.ClientId}/re-email", new { });
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _alertService.SuccessAsync("Re-Email", "Correo de activacion enviado correctamente.");
        }
        finally
        {
            IsSendingEmail = false;
        }
    }
}
