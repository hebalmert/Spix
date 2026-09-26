using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.HttpService;
using UsuarioEntity = Spix.Domain.EntitesSoftSec.Usuario;

namespace Spix.AppWpf.ViewModels.EntitiesSystem.Usuario;

// Lo que comparten Crear y Editar usuario: los mismos campos del formulario de la web,
// la foto y las validaciones. El cascaron de cada modal solo decide si guarda con POST
// o con PUT.
public abstract partial class UsuarioFormViewModel : CrudFormViewModel<UsuarioEntity>
{
    protected override string BaseUrl => "api/v1/usuarios";

    // En edicion el usuario de login NO se cambia: es la misma regla de la web
    public virtual bool IsEditControl => false;

    protected UsuarioFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override UsuarioEntity CreateEntity()
    {
        return new UsuarioEntity
        {
            Active = true
        };
    }

    // Los mismos campos obligatorios que marca la entidad con DataAnnotations
    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.FirstName)) return "Debes ingresar el nombre.";
        if (string.IsNullOrWhiteSpace(Entity.LastName)) return "Debes ingresar el apellido.";
        if (string.IsNullOrWhiteSpace(Entity.Nro_Document)) return "Debes ingresar el documento.";
        if (string.IsNullOrWhiteSpace(Entity.PhoneNumber)) return "Debes ingresar el telefono.";
        if (string.IsNullOrWhiteSpace(Entity.Address)) return "Debes ingresar la direccion.";
        if (string.IsNullOrWhiteSpace(Entity.Email)) return "Debes ingresar el correo.";
        if (string.IsNullOrWhiteSpace(Entity.UserName)) return "Debes ingresar el usuario.";
        if (Entity.UserName.Trim().Length < 6) return "El usuario debe tener al menos 6 caracteres.";
        if (string.IsNullOrWhiteSpace(Entity.Job)) return "Debes ingresar el cargo.";

        return null;
    }

    // La foto llega en el mismo Base64 venga del disco o de la camara
    public void SetPhoto(string base64)
    {
        if (string.IsNullOrWhiteSpace(base64))
        {
            return;
        }

        Entity.ImgBase64 = base64;
    }
}

public partial class CreateUsuarioDialogViewModel : UsuarioFormViewModel
{
    public CreateUsuarioDialogViewModel(
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

public partial class EditUsuarioDialogViewModel : UsuarioFormViewModel
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    public override bool IsEditControl => true;

    [ObservableProperty]
    private bool _isSendingEmail;

    public EditUsuarioDialogViewModel(
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

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(true);

    // Reenviar el correo de activacion, igual que en la web: solo con el usuario activo
    [RelayCommand]
    private async Task ResendActivationEmailAsync()
    {
        if (Entity.UsuarioId == Guid.Empty || IsSendingEmail)
        {
            return;
        }

        IsSendingEmail = true;

        try
        {
            var response = await _repository.PostAsync($"{BaseUrl}/{Entity.UsuarioId}/re-email", new { });
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _alertService.SuccessAsync("Re-Email", "Correo de activacion enviado correctamente.");
        }
        catch (Exception exception)
        {
            await _alertService.ErrorAsync("Error de conexion", exception.Message);
        }
        finally
        {
            IsSendingEmail = false;
        }
    }
}
