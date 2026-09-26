using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesGen;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ContractorEntity = Spix.Domain.EntitiesOper.Contractor;

namespace Spix.AppWpf.ViewModels.EntitiesSystem.Contractor;

// Comparte carga de combos, foto, reglas y guardado para crear y editar contratistas.
public abstract partial class ContractorFormViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractors";
    private const string ComboDocumentUrl = "api/v1/combosData/ComboDocumentType";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ContractorEntity _entity = new();

    [ObservableProperty]
    private ObservableCollection<DocumentType> _documentTypes = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    // En edicion el usuario de login NO se cambia: el backend rechaza el cambio porque
    // Identity busca la cuenta por ese nombre y quedaria huerfana.
    [ObservableProperty]
    private bool _isEditControl;

    public bool CanCreateAccount => Entity.Active;

    partial void OnEntityChanged(ContractorEntity value)
    {
        OnPropertyChanged(nameof(CanCreateAccount));
    }

    protected ContractorFormViewModel(
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

    // Un contratista nuevo nace activo y con cuenta, igual que CreateContractor de Blazor
    public async Task InitializeForCreateAsync()
    {
        IsLoading = true;

        try
        {
            IsEditControl = false;

            Entity = new ContractorEntity
            {
                Active = true,
                CreateAccount = true
            };

            await LoadDocumentTypesAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Recupera el contratista individual y mantiene su fotografia mientras se edita
    public async Task InitializeForEditAsync(Guid id)
    {
        IsLoading = true;

        try
        {
            IsEditControl = true;

            var response = await _repository.GetAsync<ContractorEntity>($"{BaseUrl}/{id}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            Entity = response.Response ?? new ContractorEntity();
            await LoadDocumentTypesAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task SaveChangesAsync(bool isEdit)
    {
        if (!TryValidate(out string? message))
        {
            await _alertService.WarningAsync("Campo requerido", message!);
            return;
        }

        // Regla de negocio de la web: un contratista inactivo no puede tener cuenta
        if (!Entity.Active)
        {
            Entity.CreateAccount = false;
        }

        // Las navegaciones no viajan: el backend las vuelve a resolver y si van llenas
        // EF intenta insertarlas de nuevo.
        Entity.DocumentType = null;
        Entity.Corporation = null;
        Entity.ContractClients = null;
        Entity.ContractorAccountPayables = null;
        Entity.ContractorPayments = null;

        IsSaving = true;

        try
        {
            var response = isEdit
                ? await _repository.PutAsync(BaseUrl, Entity)
                : await _repository.PostAsync(BaseUrl, Entity);

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

    // Mantiene la regla: un contratista inactivo no puede crear ni conservar cuenta
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

    // La lista llega armada del backend con su neutro: aqui solo se guarda para pintarla
    private async Task LoadDocumentTypesAsync()
    {
        var response = await _repository.GetAsync<List<DocumentType>>(ComboDocumentUrl);
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        DocumentTypes = new ObservableCollection<DocumentType>(response.Response ?? new List<DocumentType>());
    }

    private bool TryValidate(out string? message)
    {
        if (string.IsNullOrWhiteSpace(Entity.FirstName) || string.IsNullOrWhiteSpace(Entity.LastName))
        {
            message = "Debes ingresar el nombre y el apellido del contratista.";
            return false;
        }

        if (Entity.DocumentTypeId == Guid.Empty || string.IsNullOrWhiteSpace(Entity.Document))
        {
            message = "Debes seleccionar el tipo de documento e ingresar el documento.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Entity.PhoneNumber) || string.IsNullOrWhiteSpace(Entity.Address))
        {
            message = "Debes ingresar el telefono y la direccion del contratista.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Entity.Email) || string.IsNullOrWhiteSpace(Entity.UserName))
        {
            message = "Debes ingresar el correo y el usuario del contratista.";
            return false;
        }

        // El mismo rango que declara la entidad, para no gastar un viaje al backend
        if (Entity.Rate < 0 || Entity.Rate > 99)
        {
            message = "La comision debe estar entre 0 y 99.";
            return false;
        }

        message = null;
        return true;
    }
}

// Inicializa un contratista activo con cuenta habilitada, como CreateContractor de Blazor.
public partial class CreateContractorDialogViewModel : ContractorFormViewModel
{
    public CreateContractorDialogViewModel(
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

// Mantiene el contratista existente para edicion y reenvio individual de activacion.
public partial class EditContractorDialogViewModel : ContractorFormViewModel
{
    private const string BaseUrl = "api/v1/contractors";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private bool _isSendingEmail;

    public EditContractorDialogViewModel(
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

    // Reenvia activacion desde la edicion igual que EditContractor en Blazor
    [RelayCommand]
    private async Task ResendActivationEmailAsync()
    {
        if (Entity.ContractorId == Guid.Empty || IsSendingEmail)
        {
            return;
        }

        IsSendingEmail = true;

        try
        {
            var response = await _repository.PostAsync($"{BaseUrl}/{Entity.ContractorId}/re-email", new { });
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
