using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.Views.EntitiesContratos.ContractDocumentTemplate;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;
using System.Net.Http;
using TemplateEntity = Spix.Domain.EntitiesContratos.ContractDocumentTemplate;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractDocumentTemplate;

// Plantillas PDF: las mismas acciones de la pantalla de Blazor (/contractdocumenttemplates).
//
// De cada plantilla se puede ver el PDF original, colocarle los campos encima, probarla con
// datos de prueba, editar sus datos o eliminarla. El PDF lo arma SIEMPRE el servidor; aqui
// solo se muestra.
public partial class ContractDocumentTemplateIndexViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractdocuments/templates";
    private const int PageSize = 15;

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ObservableCollection<ContractDocumentTemplateRow> _items = new();

    [ObservableProperty]
    private string _filter = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _message = string.Empty;

    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);

    public ContractDocumentTemplateIndexViewModel(
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

    partial void OnMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasMessage));
    }

    public async Task InitializeAsync()
    {
        await LoadAsync();
    }

    public async Task LoadAsync(int page = 1)
    {
        IsLoading = true;
        Message = string.Empty;

        try
        {
            var url = $"{BaseUrl}?page={page}&recordsnumber={PageSize}";

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter.Trim())}";
            }

            var response = await _repository.GetAsync<List<TemplateEntity>>(url);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            var registros = response.Response ?? new List<TemplateEntity>();

            Items = new ObservableCollection<ContractDocumentTemplateRow>(
                registros.Select(x => new ContractDocumentTemplateRow(x)));

            CurrentPage = page;
            TotalPages = LeerPaginas(response.HttpResponseMessage);
        }
        catch (Exception excepcion)
        {
            Items = new ObservableCollection<ContractDocumentTemplateRow>();
            TotalPages = 0;
            Message = excepcion.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        Filter = string.Empty;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAsync(CurrentPage);
    }

    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages || page == CurrentPage)
        {
            return;
        }

        await LoadAsync(page);
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var resultado = await _modalService.ShowAsync<CreateContractDocumentTemplateDialogView>("Nueva plantilla PDF");
        if (!resultado.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Creado", "La plantilla fue creada correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ContractDocumentTemplateRow? item)
    {
        if (item is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = item.ContractDocumentTemplateId
        };

        var resultado = await _modalService.ShowAsync<EditContractDocumentTemplateDialogView>("Editar plantilla PDF", parametros);
        if (!resultado.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "La plantilla fue actualizada correctamente.");
    }

    // El PDF original de la plantilla, tal como esta guardado
    [RelayCommand]
    private async Task ViewPdfAsync(ContractDocumentTemplateRow? item)
    {
        if (item is null || !item.HasFile)
        {
            return;
        }

        await MostrarPdfAsync("Plantilla PDF", item.FileFullPath!);
    }

    // El editor visual de coordenadas: se elige el campo y se hace clic sobre el PDF
    [RelayCommand]
    private async Task FieldsAsync(ContractDocumentTemplateRow? item)
    {
        if (item is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = item.ContractDocumentTemplateId
        };

        var resultado = await _modalService.ShowAsync<ContractDocumentTemplateFieldsDialogView>("Colocar campos", parametros);
        if (!resultado.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "Los campos fueron guardados correctamente.");
    }

    // El servidor llena la plantilla con datos de prueba y devuelve el PDF resultante
    [RelayCommand]
    private async Task TestAsync(ContractDocumentTemplateRow? item)
    {
        if (item is null)
        {
            return;
        }

        IsLoading = true;
        string? ruta;

        try
        {
            var response = await _repository.PostAsync<object, ContractDocumentTestDTO>(
                $"{BaseUrl}/test/{item.ContractDocumentTemplateId}",
                new { });

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            ruta = response.Response?.FileFullPath;
            if (string.IsNullOrWhiteSpace(ruta))
            {
                await _alertService.WarningAsync("Test", "El servidor no devolvio el PDF de prueba.");
                return;
            }
        }
        finally
        {
            IsLoading = false;
        }

        //El visor se abre con el listado ya despejado: si no, el velo de carga se queda
        //puesto detras del modal hasta que el usuario lo cierre
        await MostrarPdfAsync("Test plantilla PDF", ruta);
    }

    [RelayCommand]
    private async Task DeleteAsync(ContractDocumentTemplateRow? item)
    {
        if (item is null)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Eliminar plantilla",
            $"Se eliminara la plantilla {item.Name}. Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{BaseUrl}/{item.ContractDocumentTemplateId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "La plantilla fue eliminada correctamente.");
    }

    private async Task MostrarPdfAsync(string titulo, string url)
    {
        var parametros = new Dictionary<string, object>
        {
            ["PdfUrl"] = url
        };

        await _modalService.ShowAsync<ContractDocumentTemplatePdfDialogView>(titulo, parametros);
    }

    // El Backend manda las paginas en la cabecera; si no llega o no es numero, no se rompe nada
    private static int LeerPaginas(HttpResponseMessage mensaje)
    {
        mensaje.Headers.TryGetValues("Totalpages", out var valores);

        return int.TryParse(valores?.FirstOrDefault(), out var paginas) ? Math.Max(0, paginas) : 0;
    }
}

// Una fila de la tabla, ya lista para pintar
public class ContractDocumentTemplateRow
{
    public TemplateEntity Item { get; }

    public Guid ContractDocumentTemplateId => Item.ContractDocumentTemplateId;

    public string? Name => Item.Name;

    public string DocumentTypeText => Item.DocumentType == ContractDocumentType.Contract
        ? "Contrato"
        : "Consent Datos";

    public int PageCount => Item.PageCount;

    public int FieldCount => Item.ContractDocumentTemplateFields?.Count ?? 0;

    public bool Active => Item.Active;

    public string? FileFullPath => Item.FileFullPath;

    public bool HasFile => !string.IsNullOrWhiteSpace(Item.FileFullPath);

    public ContractDocumentTemplateRow(TemplateEntity item)
    {
        Item = item;
    }
}
