using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.Services.Data;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.AppWpf.Views.EntitiesSystem.Usuario;
using Spix.HttpService;
using System.Collections.ObjectModel;
using UsuarioEntity = Spix.Domain.EntitesSoftSec.Usuario;
using UsuarioRoleEntity = Spix.Domain.EntitesSoftSec.UsuarioRole;

namespace Spix.AppWpf.ViewModels.EntitiesSystem.Usuario;

// Los roles de UN usuario. Es la ruta hija /usuarios/detailusuario/{id} de la web:
// el usuario arriba y su lista de roles abajo, con agregar y quitar.
//
// El listado pide los roles con el id del usuario en la consulta, por eso el endpoint
// no es fijo: se arma cuando ya se sabe de quien es la pantalla.
public partial class UsuarioRoleDetailViewModel : PagedListViewModel<UsuarioRoleEntity>
{
    private const string BaseUrl = "api/v1/usuarioRoles";
    private const string UsuarioUrl = "api/v1/usuarios";

    private readonly IRepository _repository;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;
    private readonly HttpResponseHandler _responseHandler;

    private Guid _usuarioId;

    protected override string Endpoint => $"{BaseUrl}?guidid={_usuarioId}";

    [ObservableProperty]
    private ObservableCollection<UsuarioRoleRow> _rows = new();

    [ObservableProperty]
    private string _usuarioName = string.Empty;

    [ObservableProperty]
    private string _usuarioDocument = string.Empty;

    [ObservableProperty]
    private string? _usuarioPhoto;

    // La pantalla no sabe volver: lo decide quien la abrio
    public event EventHandler? BackRequested;

    public UsuarioRoleDetailViewModel(
        IPagedEntityService<UsuarioRoleEntity> pagedEntityService,
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

    public async Task InitializeAsync(Guid id)
    {
        _usuarioId = id;

        await CargarUsuarioAsync();
        await LoadAsync(1);
    }

    protected override Task AfterLoadAsync()
    {
        Rows = new ObservableCollection<UsuarioRoleRow>(Items.Select(item => new UsuarioRoleRow(item)));

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task NewAsync()
    {
        var parametros = new Dictionary<string, object>
        {
            ["Id"] = _usuarioId
        };

        var result = await _modalService.ShowAsync<CreateUsuarioRoleDialogView>("Nuevo rol", parametros);
        if (!result.Succeeded)
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Guardado", "El rol fue guardado correctamente.");
    }

    [RelayCommand]
    private async Task DeleteAsync(UsuarioRoleRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Eliminar rol",
            "Esta accion no se puede deshacer.",
            "Eliminar");

        if (!confirmado)
        {
            return;
        }

        var response = await _repository.DeleteAsync($"{BaseUrl}/{fila.UsuarioRoleId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        await LoadAsync(CurrentPage);
        await _alertService.SuccessAsync("Eliminado", "El rol fue eliminado correctamente.");
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    // El encabezado de la pantalla: de quien son los roles que se estan viendo
    private async Task CargarUsuarioAsync()
    {
        var response = await _repository.GetAsync<UsuarioEntity>($"{UsuarioUrl}/{_usuarioId}");
        if (await _responseHandler.HandleErrorAsync(response))
        {
            return;
        }

        var usuario = response.Response;
        if (usuario is null)
        {
            return;
        }

        UsuarioName = $"{usuario.FirstName} {usuario.LastName}".Trim();
        UsuarioDocument = usuario.Nro_Document ?? string.Empty;
        UsuarioPhoto = usuario.ImageFullPath;
    }
}

// La fila de la tabla de roles: el XAML solo pinta el nombre ya resuelto.
public class UsuarioRoleRow
{
    public UsuarioRoleEntity Item { get; }

    public Guid UsuarioRoleId => Item.UsuarioRoleId;

    public string RoleName => Item.UserType.ToString();

    public UsuarioRoleRow(UsuarioRoleEntity item)
    {
        Item = item;
    }
}
