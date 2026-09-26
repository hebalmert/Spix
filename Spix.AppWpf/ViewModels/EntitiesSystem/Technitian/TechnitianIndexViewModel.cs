using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesSystem.Technitian;
using Spix.Domain.EntitiesOper;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesSystem.Technitian;

// Lista los tecnicos con la misma paginacion, auditoria, reenvio de activacion y CRUD de Blazor.
public partial class TechnitianIndexViewModel : PagedListViewModel<Technician>
{
    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    protected override string Endpoint => "api/v1/technitians";

    //La tabla pinta estas filas, no las entidades: el nombre completo y la fecha ya vienen
    //resueltos para que el XAML no tenga que calcular nada.
    [ObservableProperty]
    private ObservableCollection<TechnitianRow> _rows = new();

    public TechnitianIndexViewModel(
        IPagedEntityService<Technician> pagedEntityService,
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
        Rows = new ObservableCollection<TechnitianRow>(Items.Select(x => new TechnitianRow(x)));

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        ModalResult result = await _modalService.ShowAsync<CreateTechnitianDialogView>("Crear tecnico");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El tecnico fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(TechnitianRow? row)
    {
        if (row is null)
        {
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            ["Id"] = row.TechnicianId
        };

        ModalResult result = await _modalService.ShowAsync<EditTechnitianDialogView>("Editar tecnico", parameters);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El tecnico fue actualizado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(TechnitianRow? row)
    {
        if (row is null)
        {
            return;
        }

        bool confirmed = await _alertService.ConfirmAsync(
            "Eliminar tecnico",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmed)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{Endpoint}/{row.TechnicianId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El tecnico fue eliminado correctamente.");
    }

    // Solo tiene sentido con el tecnico activo: inactivo no tiene cuenta que activar
    [RelayCommand]
    private async Task ResendActivationEmailAsync(TechnitianRow? row)
    {
        if (row is null || !row.Active)
        {
            return;
        }

        var response = await _repository.PostAsync($"{Endpoint}/{row.TechnicianId}/re-email", new { });
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await _alertService.SuccessAsync("Re-Email", "Correo de activacion enviado correctamente.");
    }

    // Rastro del registro: por ahora el sistema solo guarda cuando se creo.
    // Va en un boton para no gastar una columna de la tabla.
    [RelayCommand]
    private async Task AuditAsync(TechnitianRow? row)
    {
        if (row is null)
        {
            return;
        }

        await _alertService.WarningAsync("Auditoria del registro", $"Creado: {row.CreatedText}");
    }
}

// La fila de la tabla: texto y formato resueltos una sola vez, fuera del XAML.
public class TechnitianRow
{
    public Technician Item { get; }

    public Guid TechnicianId => Item.TechnicianId;

    public string FullName => $"{Item.FirstName} {Item.LastName}".Trim();

    public string? UserName => Item.UserName;

    public string? PhoneNumber => Item.PhoneNumber;

    public string? Email => Item.Email;

    public bool Active => Item.Active;

    public string? ImageFullPath => Item.ImageFullPath;

    //Un tecnico viejo puede no tener fecha guardada: la ventana de auditoria no queda vacia
    public string CreatedText => Item.DateCreated.HasValue
        ? Item.DateCreated.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
        : "-";

    public TechnitianRow(Technician item)
    {
        Item = item;
    }
}
