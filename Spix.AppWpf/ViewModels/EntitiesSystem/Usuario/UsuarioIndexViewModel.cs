using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesSystem.Usuario;
using Spix.HttpService;
using System.Collections.ObjectModel;
using UsuarioEntity = Spix.Domain.EntitesSoftSec.Usuario;

namespace Spix.AppWpf.ViewModels.EntitiesSystem.Usuario;

// Lista los usuarios del sistema paginados y concentra sus acciones: crear, editar,
// reenviar el correo de activacion, administrar los roles y eliminar.
//
// Los roles NO caben en el listado: en la web viven en /usuarios/detailusuario/{id},
// y aqui son una pantalla aparte que se abre desde la fila.
public partial class UsuarioIndexViewModel : PagedListViewModel<UsuarioEntity>
{
    private const string BaseUrl = "api/v1/usuarios";

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;
    private readonly NavigationService _navigationService;

    protected override string Endpoint => BaseUrl;

    // La tabla pinta filas ya resueltas: nombre completo, foto y conteo de roles.
    [ObservableProperty]
    private ObservableCollection<UsuarioRow> _rows = new();

    public UsuarioIndexViewModel(
        IPagedEntityService<UsuarioEntity> pagedEntityService,
        IRepository repository,
        ModalService modalService,
        AlertService alertService,
        HttpResponseHandler responseHandler,
        NavigationService navigationService)
        : base(pagedEntityService)
    {
        _repository = repository;
        _modalService = modalService;
        _alertService = alertService;
        _responseHandler = responseHandler;
        _navigationService = navigationService;
    }

    protected override Task AfterLoadAsync()
    {
        Rows = new ObservableCollection<UsuarioRow>(Items.Select(item => new UsuarioRow(item)));

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var result = await _modalService.ShowAsync<CreateUsuarioDialogView>("Crear usuario");
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El usuario fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task EditAsync(UsuarioRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var parametros = new Dictionary<string, object>
        {
            ["Id"] = fila.UsuarioId
        };

        var result = await _modalService.ShowAsync<EditUsuarioDialogView>("Editar usuario", parametros);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Actualizado", "El usuario fue actualizado correctamente.");
    }

    // Los roles son una PANTALLA: la misma ruta hija de la web, con su propio listado
    [RelayCommand]
    private void OpenRoles(UsuarioRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        _navigationService.Show<UsuarioRoleDetailView>(
            "Usuarios",
            $"Sistema / Roles de {fila.FullName}",
            vista =>
            {
                vista.Prepare(fila.UsuarioId);

                vista.BackRequested += (_, _) => VolverAlListado();
            });
    }

    private void VolverAlListado()
    {
        _navigationService.Show<UsuarioIndexView>("Usuarios", "Sistema / Usuarios");
    }

    // Solo tiene sentido con el usuario activo: inactivo no puede activar su cuenta
    [RelayCommand]
    private async Task ResendEmailAsync(UsuarioRow? fila)
    {
        if (fila is null || !fila.Active)
        {
            return;
        }

        var response = await _repository.PostAsync($"{BaseUrl}/{fila.UsuarioId}/re-email", new { });
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await _alertService.SuccessAsync("Re-Email", "Correo de activacion enviado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(UsuarioRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Eliminar usuario",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{BaseUrl}/{fila.UsuarioId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El usuario fue eliminado correctamente.");
    }
}

// La fila de la tabla: deja resuelto el texto para que el XAML no calcule nada.
public class UsuarioRow
{
    public UsuarioEntity Item { get; }

    public Guid UsuarioId => Item.UsuarioId;

    public string FullName => $"{Item.FirstName} {Item.LastName}".Trim();

    public string? Document => Item.Nro_Document;

    public string? Photo => Item.ImageFullPath;

    // El backend lo calcula con la coleccion de roles del usuario
    public int TotalRoles => Item.TotalRoles;

    public bool Active => Item.Active;

    public UsuarioRow(UsuarioEntity item)
    {
        Item = item;
    }
}
