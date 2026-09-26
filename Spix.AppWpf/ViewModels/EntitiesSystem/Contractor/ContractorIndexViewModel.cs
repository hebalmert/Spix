using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesSystem.Contractor;
using Spix.HttpService;
using System.Collections.ObjectModel;
using ContractorEntity = Spix.Domain.EntitiesOper.Contractor;

namespace Spix.AppWpf.ViewModels.EntitiesSystem.Contractor;

// Una fila del listado de contratistas, YA LISTA PARA PINTAR.
//
// El nombre completo, la fecha de creacion con formato y la decision de mostrar el sobre
// se resuelven aqui una sola vez. La pantalla no arma textos ni evalua condiciones.
public class ContractorRow
{
    public ContractorEntity Item { get; }

    public Guid ContractorId => Item.ContractorId;

    public string FullName => $"{Item.FirstName} {Item.LastName}".Trim();

    public string? UserName => Item.UserName;

    public string? PhoneNumber => Item.PhoneNumber;

    public string? Email => Item.Email;

    public string? ImageFullPath => Item.ImageFullPath;

    public bool Active => Item.Active;

    // El sobre solo tiene sentido si al contratista se le creo cuenta
    public bool CreateAccount => Item.CreateAccount;

    // El sistema solo guarda cuando se creo el registro; si no vino, no se inventa texto
    public string CreatedText => Item.DateCreated.HasValue
        ? Item.DateCreated.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
        : "-";

    public ContractorRow(ContractorEntity item)
    {
        Item = item;
    }
}

// Contratistas, replicado de la pantalla /contractors de la web.
//
// La fila lleva cuatro botones porque la web tiene cuatro: la auditoria y el reenvio del
// correo no gastan una columna de la tabla, viven en la fila.
public partial class ContractorIndexViewModel : PagedListViewModel<ContractorEntity>
{
    private const string BaseUrl = "api/v1/contractors";

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<ContractorRow> _rows = new();

    protected override string Endpoint => BaseUrl;

    public ContractorIndexViewModel(
        IPagedEntityService<ContractorEntity> pagedEntityService,
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

    // Cada carga arma las filas ya resueltas para la pantalla
    protected override Task AfterLoadAsync()
    {
        Rows = new ObservableCollection<ContractorRow>(Items.Select(x => new ContractorRow(x)));

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        ModalResult result = await _modalService.ShowAsync<CreateContractorDialogView>("Crear contratista");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El contratista fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(ContractorRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.ContractorId
        };

        ModalResult result = await _modalService.ShowAsync<EditContractorDialogView>("Editar contratista", parametros);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El contratista fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(ContractorRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        bool confirmed = await _alertService.ConfirmAsync(
            "Eliminar contratista",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{BaseUrl}/{fila.ContractorId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El contratista fue eliminado correctamente.");
    }

    // Reutiliza el endpoint existente para reenviar una cuenta aun no activada
    [RelayCommand]
    private async Task ResendActivationEmailAsync(ContractorRow? fila)
    {
        if (fila is null || !fila.CreateAccount)
        {
            return;
        }

        var response = await _repository.PostAsync($"{BaseUrl}/{fila.ContractorId}/re-email", new { });
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await _alertService.SuccessAsync("Re-Email", "Correo de activacion enviado correctamente.");
    }

    // Rastro del registro: por ahora el sistema solo guarda cuando se creo
    [RelayCommand]
    private async Task AuditAsync(ContractorRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        await _alertService.WarningAsync("Auditoria del registro", $"Creado: {fila.CreatedText}");
    }
}
