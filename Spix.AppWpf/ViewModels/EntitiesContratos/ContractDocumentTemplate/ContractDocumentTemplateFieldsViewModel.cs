using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;
using TemplateEntity = Spix.Domain.EntitiesContratos.ContractDocumentTemplate;
using TemplateFieldEntity = Spix.Domain.EntitiesContratos.ContractDocumentTemplateField;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractDocumentTemplate;

// Editor visual de coordenadas: se elige el campo, se hace clic en el PDF y se arrastra.
// Los cambios quedan en memoria y se guardan todos juntos con Guardar, igual que en Blazor.
//
// El dibujo del PDF NO vive aqui: lo hace pdf.js dentro del navegador embebido. Este
// ViewModel manda la lista de campos y recibe las acciones del usuario; la pantalla es la
// que habla con el navegador.
public partial class ContractDocumentTemplateFieldsViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractdocuments/templates";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    // El PDF que la pantalla tiene que dibujar (el original o la vista previa)
    public event EventHandler<PdfEditorDocument>? DocumentReady;

    // Los campos cambiaron: la pantalla se los vuelve a mandar al editor
    public event EventHandler? FieldsChanged;

    // Se eligio un campo en la lista: el PDF se mueve hasta el
    public event EventHandler<string>? ScrollToFieldRequested;

    [ObservableProperty]
    private string _templateName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<FieldTypeOption> _fieldTypes = new();

    [ObservableProperty]
    private ObservableCollection<TemplateFieldRow> _placedFields = new();

    [ObservableProperty]
    private ObservableCollection<int> _pages = new();

    [ObservableProperty]
    private TemplateFieldRow? _selectedField;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isPreview;

    // Mientras el servidor arma la vista previa el boton no se puede volver a pulsar
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ShowPreviewCommand))]
    private bool _isBusyPreview;

    [ObservableProperty]
    private string _placingText = string.Empty;

    private Guid _templateId;
    private string? _originalBase64;
    private int _pageCount;
    private ContractDocumentFieldType? _placingType;
    private bool _isDirty;

    // Lo que la pantalla necesita saber sin hacer cuentas en el marcado
    public bool IsEditorMode => !IsPreview;

    public bool CanPlace => !IsPreview;

    public bool HasPlacing => !string.IsNullOrWhiteSpace(PlacingText);

    public bool HasSelectedField => SelectedField is not null && !IsPreview;

    public bool IsSignatureSelected => HasSelectedField && SelectedField!.IsSignature;

    public bool IsTextSelected => HasSelectedField && !SelectedField!.IsSignature;

    public bool MissingSignature => !PlacedFields.Any(x => x.IsSignature);

    public string PlacedCountText => $"Campos colocados ({PlacedFields.Count})";

    public string PageCountText => $"{_pageCount} pagina(s)";

    public ContractDocumentTemplateFieldsViewModel(
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

    partial void OnIsPreviewChanged(bool value)
    {
        OnPropertyChanged(nameof(IsEditorMode));
        OnPropertyChanged(nameof(CanPlace));
        OnPropertyChanged(nameof(HasSelectedField));
        OnPropertyChanged(nameof(IsSignatureSelected));
        OnPropertyChanged(nameof(IsTextSelected));
    }

    partial void OnPlacingTextChanged(string value)
    {
        OnPropertyChanged(nameof(HasPlacing));
    }

    partial void OnSelectedFieldChanged(TemplateFieldRow? value)
    {
        OnPropertyChanged(nameof(HasSelectedField));
        OnPropertyChanged(nameof(IsSignatureSelected));
        OnPropertyChanged(nameof(IsTextSelected));
    }

    // Trae la plantilla con sus campos y el PDF original para dibujarlo
    public async Task InitializeAsync(Guid id)
    {
        _templateId = id;
        IsLoading = true;

        try
        {
            var plantilla = await _repository.GetAsync<TemplateEntity>($"{BaseUrl}/{id}");
            if (await _responseHandler.HandleErrorAsync(plantilla))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            var modelo = plantilla.Response ?? new TemplateEntity();
            TemplateName = modelo.Name ?? string.Empty;

            PlacedFields = new ObservableCollection<TemplateFieldRow>(
                (modelo.ContractDocumentTemplateFields ?? new List<TemplateFieldEntity>())
                    .OrderBy(x => x.PageNumber)
                    .ThenBy(x => x.PositionY)
                    .Select(Envolver));

            var pdf = await _repository.GetAsync<ContractDocumentPdfDTO>($"{BaseUrl}/{id}/pdf");
            if (await _responseHandler.HandleErrorAsync(pdf))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            _originalBase64 = pdf.Response?.FileBase64;
            _pageCount = Math.Max(1, pdf.Response?.PageCount ?? 1);
            Pages = new ObservableCollection<int>(Enumerable.Range(1, _pageCount));

            FieldTypes = new ObservableCollection<FieldTypeOption>(
                Enum.GetValues<ContractDocumentFieldType>().Select(x => new FieldTypeOption(x, NombreDe(x))));

            OnPropertyChanged(nameof(PageCountText));
            Refrescar();
        }
        catch (Exception excepcion)
        {
            await _alertService.ErrorAsync("Error de conexion", excepcion.Message);
            await _modalService.CloseAsync(ModalResult.Cancel());
            return;
        }
        finally
        {
            IsLoading = false;
        }

        PedirDibujo(_originalBase64, soloLectura: false);
    }

    // Lo que la pantalla le manda al editor cada vez que algo cambia
    public EditorFieldsSnapshot BuildEditorFields()
    {
        var campos = PlacedFields
            .Select(x => new EditorField(
                x.Key.ToString(),
                (int)x.FieldType,
                x.TypeName,
                x.PageNumber,
                x.PositionX,
                x.PositionY,
                x.Width,
                x.Height,
                x.FontSize))
            .ToList();

        return new EditorFieldsSnapshot(
            campos,
            SelectedField?.Key.ToString(),
            (int?)_placingType ?? 0);
    }

    // Si el PDF no se puede dibujar se avisa con el motivo, sin tumbar la aplicacion
    public async Task AvisarAsync(string mensaje)
    {
        await _alertService.ErrorAsync("PDF", mensaje);
    }

    // Acciones que llegan del PDF (pdfFieldEditor.js, por postMessage)

    public void PlaceField(int pageNumber, double positionX, double positionY)
    {
        if (_placingType is null)
        {
            return;
        }

        var esFirma = _placingType == ContractDocumentFieldType.Signature;

        var campo = new TemplateFieldEntity
        {
            ContractDocumentTemplateFieldId = Guid.NewGuid(),
            ContractDocumentTemplateId = _templateId,
            FieldType = _placingType.Value,
            PageNumber = pageNumber,
            PositionX = EnPuntos(positionX),
            PositionY = EnPuntos(positionY),
            Width = esFirma ? 180 : null,
            Height = esFirma ? 60 : null,
            FontSize = 12
        };

        var fila = Envolver(campo);
        PlacedFields.Add(fila);

        SelectedField = fila;
        _placingType = null;
        PlacingText = string.Empty;
        _isDirty = true;

        MarcarTipos();
        Refrescar();
    }

    public void MoveField(string key, double positionX, double positionY, double? width, double? height)
    {
        var fila = Buscar(key);
        if (fila is null)
        {
            return;
        }

        fila.Aplicar(
            EnPuntos(positionX),
            EnPuntos(positionY),
            width.HasValue ? EnPuntos(width.Value) : null,
            height.HasValue ? EnPuntos(height.Value) : null);

        SelectedField = fila;
        _isDirty = true;

        Refrescar();
    }

    public void SelectField(string key)
    {
        SelectedField = Buscar(key);
        Refrescar();
    }

    // Acciones del panel

    [RelayCommand]
    private void StartPlacing(FieldTypeOption? option)
    {
        if (option is null || IsPreview)
        {
            return;
        }

        _placingType = _placingType == option.Value ? null : option.Value;
        PlacingText = _placingType is null
            ? string.Empty
            : $"Haga clic en el PDF para colocar: {NombreDe(_placingType.Value)}";

        MarcarTipos();
        Refrescar();
    }

    [RelayCommand]
    private void SelectRow(TemplateFieldRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        SelectedField = fila;
        Refrescar();

        if (!IsPreview)
        {
            ScrollToFieldRequested?.Invoke(this, fila.Key.ToString());
        }
    }

    [RelayCommand]
    private void RemoveField(TemplateFieldRow? fila)
    {
        if (fila is null || IsPreview)
        {
            return;
        }

        PlacedFields.Remove(fila);

        if (SelectedField == fila)
        {
            SelectedField = null;
        }

        _isDirty = true;
        Refrescar();
    }

    [RelayCommand]
    private void ShowEditor()
    {
        if (!IsPreview || string.IsNullOrWhiteSpace(_originalBase64))
        {
            return;
        }

        IsPreview = false;
        PedirDibujo(_originalBase64, soloLectura: false);
    }

    private bool PuedeVerPrueba()
    {
        return !IsBusyPreview;
    }

    // El servidor llena el PDF con datos de prueba y lo devuelve para mirarlo
    [RelayCommand(CanExecute = nameof(PuedeVerPrueba))]
    private async Task ShowPreviewAsync()
    {
        IsBusyPreview = true;

        try
        {
            var response = await _repository.PostAsync<List<TemplateFieldEntity>, ContractDocumentPdfDTO>(
                $"{BaseUrl}/{_templateId}/preview",
                CamposLimpios());

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            IsPreview = true;
            _placingType = null;
            PlacingText = string.Empty;
            MarcarTipos();

            PedirDibujo(response.Response?.FileBase64, soloLectura: true);
        }
        finally
        {
            IsBusyPreview = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (MissingSignature)
        {
            var seguir = await _alertService.ConfirmAsync(
                "Falta la firma",
                "La plantilla no tiene campo de firma y el cliente no podra firmar. Desea guardar de todas formas?",
                "Guardar");

            if (!seguir)
            {
                return;
            }
        }

        IsSaving = true;

        try
        {
            var response = await _repository.PutAsync($"{BaseUrl}/{_templateId}/fields", CamposLimpios());
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            _isDirty = false;
            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        if (_isDirty)
        {
            var salir = await _alertService.ConfirmAsync(
                "Cambios sin guardar",
                "Si sale ahora se pierden los campos que movio o agrego. Desea salir?",
                "Salir");

            if (!salir)
            {
                return;
            }
        }

        await _modalService.CloseAsync(ModalResult.Cancel());
    }

    // Utilidades

    private TemplateFieldRow Envolver(TemplateFieldEntity campo)
    {
        var fila = new TemplateFieldRow(campo, NombreDe(campo.FieldType));

        //Lo que el usuario escribe en el detalle tambien mueve el recuadro del PDF
        fila.Changed = () =>
        {
            _isDirty = true;
            Refrescar();
        };

        return fila;
    }

    private TemplateFieldRow? Buscar(string key)
    {
        return Guid.TryParse(key, out var id)
            ? PlacedFields.FirstOrDefault(x => x.Key == id)
            : null;
    }

    private void MarcarTipos()
    {
        foreach (var tipo in FieldTypes)
        {
            tipo.IsActive = _placingType == tipo.Value;
        }
    }

    private void PedirDibujo(string? base64, bool soloLectura)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return;
        }

        DocumentReady?.Invoke(this, new PdfEditorDocument(base64, soloLectura));
    }

    // Avisa a la pantalla y refresca lo que se ve en el panel
    private void Refrescar()
    {
        OnPropertyChanged(nameof(PlacedCountText));
        OnPropertyChanged(nameof(MissingSignature));

        foreach (var fila in PlacedFields)
        {
            fila.IsSelected = fila == SelectedField;
        }

        FieldsChanged?.Invoke(this, EventArgs.Empty);
    }

    private List<TemplateFieldEntity> CamposLimpios()
    {
        return PlacedFields
            .Select(x => new TemplateFieldEntity
            {
                ContractDocumentTemplateFieldId = x.Key,
                ContractDocumentTemplateId = _templateId,
                FieldType = x.FieldType,
                PageNumber = x.PageNumber,
                PositionX = x.PositionX,
                PositionY = x.PositionY,
                Width = x.Width,
                Height = x.Height,
                FontSize = x.FontSize
            })
            .ToList();
    }

    private static decimal EnPuntos(double valor)
    {
        return Math.Round((decimal)Math.Max(0, valor), 2);
    }

    private static string NombreDe(ContractDocumentFieldType fieldType)
    {
        return fieldType switch
        {
            ContractDocumentFieldType.FullName => "Nombre",
            ContractDocumentFieldType.Document => "Documento",
            ContractDocumentFieldType.Phone => "Telefono",
            ContractDocumentFieldType.Date => "Fecha",
            ContractDocumentFieldType.Signature => "Firma",
            ContractDocumentFieldType.Address => "Direccion",
            ContractDocumentFieldType.Email => "Correo",
            ContractDocumentFieldType.PrintName => "Nombre imprenta",
            _ => fieldType.ToString()
        };
    }
}

// Un tipo de campo del panel de la izquierda
public partial class FieldTypeOption : ObservableObject
{
    [ObservableProperty]
    private bool _isActive;

    public ContractDocumentFieldType Value { get; }

    public string Name { get; }

    public FieldTypeOption(ContractDocumentFieldType value, string name)
    {
        Value = value;
        Name = name;
    }
}

// Un campo ya colocado sobre el PDF
public partial class TemplateFieldRow : ObservableObject
{
    [ObservableProperty]
    private int _pageNumber;

    [ObservableProperty]
    private decimal _positionX;

    [ObservableProperty]
    private decimal _positionY;

    [ObservableProperty]
    private decimal? _width;

    [ObservableProperty]
    private decimal? _height;

    [ObservableProperty]
    private int _fontSize;

    [ObservableProperty]
    private bool _isSelected;

    //Mientras se acomodan las coordenadas que llegan del PDF no se avisa por cada una
    private bool _silencio;

    public Action? Changed { get; set; }

    public Guid Key { get; }

    public ContractDocumentFieldType FieldType { get; }

    public string TypeName { get; }

    public bool IsSignature => FieldType == ContractDocumentFieldType.Signature;

    public string PageText => $"Pag {PageNumber}";

    public TemplateFieldRow(TemplateFieldEntity campo, string typeName)
    {
        Key = campo.ContractDocumentTemplateFieldId;
        FieldType = campo.FieldType;
        TypeName = typeName;

        _pageNumber = campo.PageNumber;
        _positionX = campo.PositionX;
        _positionY = campo.PositionY;
        _width = campo.Width;
        _height = campo.Height;
        _fontSize = campo.FontSize <= 0 ? 12 : campo.FontSize;
    }

    // Lo que llega cuando el usuario arrastra el recuadro en el PDF
    public void Aplicar(decimal positionX, decimal positionY, decimal? width, decimal? height)
    {
        _silencio = true;

        PositionX = positionX;
        PositionY = positionY;

        if (width.HasValue)
        {
            Width = width;
        }

        if (height.HasValue)
        {
            Height = height;
        }

        _silencio = false;
    }

    partial void OnPageNumberChanged(int value)
    {
        OnPropertyChanged(nameof(PageText));
        Avisar();
    }

    partial void OnPositionXChanged(decimal value) => Avisar();

    partial void OnPositionYChanged(decimal value) => Avisar();

    partial void OnWidthChanged(decimal? value) => Avisar();

    partial void OnHeightChanged(decimal? value) => Avisar();

    partial void OnFontSizeChanged(int value) => Avisar();

    private void Avisar()
    {
        if (_silencio)
        {
            return;
        }

        Changed?.Invoke();
    }
}

// El PDF que la pantalla tiene que dibujar
public class PdfEditorDocument
{
    public string Base64 { get; }

    public bool ReadOnly { get; }

    public PdfEditorDocument(string base64, bool readOnly)
    {
        Base64 = base64;
        ReadOnly = readOnly;
    }
}

// Lo que el editor JS necesita de cada campo
public record EditorField(
    string Key,
    int FieldType,
    string Label,
    int PageNumber,
    decimal PositionX,
    decimal PositionY,
    decimal? Width,
    decimal? Height,
    int FontSize);

// La foto completa que se le manda al editor: campos, cual esta elegido y que se va a colocar
public record EditorFieldsSnapshot(
    IReadOnlyList<EditorField> Fields,
    string? SelectedKey,
    int PlacingType);
