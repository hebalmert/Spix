using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;
using TemplateEntity = Spix.Domain.EntitiesContratos.ContractDocumentTemplate;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractDocumentTemplate;

// Los mismos datos del formulario de Blazor: nombre, tipo, activo y el PDF.
// Los campos (nombre, documento, firma...) NO se editan aqui: van en el editor visual.
public abstract partial class ContractDocumentTemplateFormViewModel : CrudFormViewModel<TemplateEntity>
{
    private readonly AlertService _alertService;

    // El combo lo arma el ViewModel, no el marcado: el .xaml solo lo pinta
    [ObservableProperty]
    private ObservableCollection<DocumentTypeOption> _documentTypes = new();

    // El nombre del PDF elegido del disco, para que se vea que ya quedo cargado
    [ObservableProperty]
    private string _pdfName = string.Empty;

    protected override string BaseUrl => "api/v1/contractdocuments/templates";

    // En una plantilla nueva el PDF es obligatorio; al editar se conserva el que ya tiene
    protected abstract bool RequierePdf { get; }

    protected ContractDocumentTemplateFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
        _alertService = alertService;
    }

    protected override TemplateEntity CreateEntity()
    {
        return new TemplateEntity
        {
            Active = true,
            PageCount = 1,
            DocumentType = ContractDocumentType.ConsentData
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.Name)) return "Debes ingresar el nombre de la plantilla.";
        if (RequierePdf && string.IsNullOrWhiteSpace(Entity.FileBase64)) return "Debes seleccionar un PDF.";
        return null;
    }

    public Task InitializeAsync()
    {
        DocumentTypes = new ObservableCollection<DocumentTypeOption>
        {
            new(ContractDocumentType.ConsentData, "Consent Datos"),
            new(ContractDocumentType.Contract, "Contrato")
        };

        return Task.CompletedTask;
    }

    // El PDF que el usuario eligio del disco, ya en el Base64 que recibe el Backend
    public void SetPdf(string base64, string fileName)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return;
        }

        Entity.FileBase64 = base64;
        Entity.OriginalFileName = fileName;
        PdfName = fileName;

        OnPropertyChanged(nameof(Entity));
    }

    public async Task AvisarPdfAsync(string mensaje)
    {
        await _alertService.WarningAsync("PDF", mensaje);
    }

    // Trae la plantilla y deja fuera sus campos: en el PUT de la plantilla no viajan
    public async Task LoadForEditAsync(Guid id)
    {
        await LoadAsync(id);

        Entity.ContractDocumentTemplateFields = null;
        PdfName = Entity.OriginalFileName ?? string.Empty;

        OnPropertyChanged(nameof(Entity));
    }
}

// Una opcion del combo de tipo de documento
public class DocumentTypeOption
{
    public ContractDocumentType Value { get; }

    public string Name { get; }

    public DocumentTypeOption(ContractDocumentType value, string name)
    {
        Value = value;
        Name = name;
    }
}

public partial class CreateContractDocumentTemplateDialogViewModel : ContractDocumentTemplateFormViewModel
{
    protected override bool RequierePdf => true;

    public CreateContractDocumentTemplateDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(false);
}

public partial class EditContractDocumentTemplateDialogViewModel : ContractDocumentTemplateFormViewModel
{
    protected override bool RequierePdf => false;

    public EditContractDocumentTemplateDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(true);
}
