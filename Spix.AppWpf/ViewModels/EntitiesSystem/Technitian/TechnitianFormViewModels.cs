using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesOper;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesSystem.Technitian;

// Comparte combo de documento, foto y validacion entre crear y editar, como FormTechnitian de Blazor.
public abstract partial class TechnitianFormViewModel : CrudFormViewModel<Technician>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<DocumentType> _documentTypes = new();

    protected override string BaseUrl => "api/v1/technitians";

    //El usuario de login no se cambia al editar: Identity lo busca por ese nombre y el
    //backend rechaza el cambio. Por eso el campo se bloquea en la pantalla de edicion.
    public abstract bool IsUserNameReadOnly { get; }

    protected TechnitianFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _alertService = alertService;
    }

    protected override Technician CreateEntity()
    {
        return new Technician
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.FirstName) || string.IsNullOrWhiteSpace(Entity.LastName)) return "Debes ingresar el nombre y el apellido del tecnico.";
        if (Entity.DocumentTypeId == Guid.Empty || string.IsNullOrWhiteSpace(Entity.Document)) return "Debes seleccionar el tipo de documento e ingresar el documento.";
        if (string.IsNullOrWhiteSpace(Entity.PhoneNumber) || string.IsNullOrWhiteSpace(Entity.Address)) return "Debes ingresar el telefono y la direccion del tecnico.";
        if (string.IsNullOrWhiteSpace(Entity.Email)) return "Debes ingresar el correo del tecnico.";
        if (string.IsNullOrWhiteSpace(Entity.UserName)) return "Debes ingresar el usuario del tecnico.";
        return null;
    }

    // Carga el mismo combo de tipos de documento que utiliza FormTechnitian en Blazor.
    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<DocumentType>>("api/v1/combosData/ComboDocumentType");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            DocumentTypes = new ObservableCollection<DocumentType>(response.Response ?? new List<DocumentType>());
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync("Error de conexion", exception.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // La foto que entrega el selector, ya en el Base64 que recibe el Backend.
    // Es el mismo dato venga del disco o de la camara: aqui no se distingue.
    public void SetPhoto(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return;
        }

        Entity.ImgBase64 = base64;
    }
}

// Tecnico nuevo: activo y con el primer tipo de documento ya elegido, como CreateTechnitian.
public partial class CreateTechnitianDialogViewModel : TechnitianFormViewModel
{
    public override bool IsUserNameReadOnly => false;

    public CreateTechnitianDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    // Sin tipo de documento elegido el guardado falla: se toma el primero que trae el combo.
    public async Task InitializeForCreateAsync()
    {
        await InitializeAsync();

        if (Entity.DocumentTypeId == Guid.Empty && DocumentTypes.Count > 0)
        {
            Entity.DocumentTypeId = DocumentTypes[0].DocumentTypeId;
            OnPropertyChanged(nameof(Entity));
        }
    }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(false);
}

// Tecnico existente: se edita y desde aqui tambien se reenvia su correo de activacion.
public partial class EditTechnitianDialogViewModel : TechnitianFormViewModel
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private bool _isSendingEmail;

    public override bool IsUserNameReadOnly => true;

    public EditTechnitianDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _alertService = alertService;
    }

    public async Task LoadForEditAsync(Guid id)
    {
        await InitializeAsync();
        await LoadAsync(id);
    }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(true);

    // Reenvia activacion desde la edicion igual que EditTechnitian en Blazor.
    [RelayCommand]
    private async Task ResendActivationEmailAsync()
    {
        if (Entity.TechnicianId == Guid.Empty || IsSendingEmail)
        {
            return;
        }

        IsSendingEmail = true;

        try
        {
            var response = await _repository.PostAsync($"{BaseUrl}/{Entity.TechnicianId}/re-email", new { });
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
