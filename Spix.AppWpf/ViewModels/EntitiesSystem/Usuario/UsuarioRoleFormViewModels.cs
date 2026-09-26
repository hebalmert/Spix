using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.HttpService;
using System.Collections.ObjectModel;
using UsuarioRoleEntity = Spix.Domain.EntitesSoftSec.UsuarioRole;

namespace Spix.AppWpf.ViewModels.EntitiesSystem.Usuario;

// El formulario de un rol: un solo combo. La lista llega ARMADA del backend, con el
// "Seleccione un rol" traducido en la posicion 0; aqui no se filtra ni se ordena nada.
public abstract partial class UsuarioRoleFormViewModel : CrudFormViewModel<UsuarioRoleEntity>
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly AlertService _alertService;

    protected override string BaseUrl => "api/v1/usuarioRoles";

    [ObservableProperty]
    private ObservableCollection<IntNameModel> _roles = new();

    [ObservableProperty]
    private int _selectedRole;

    protected UsuarioRoleFormViewModel(
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

    protected override UsuarioRoleEntity CreateEntity()
    {
        return new UsuarioRoleEntity();
    }

    // El neutro del combo vale 0 y no es un rol valido
    protected override string? GetValidationMessage()
    {
        if (SelectedRole == 0) return "Debe seleccionar un rol.";

        return null;
    }

    // El combo mueve la entidad: asi el guardado no tiene que traducir nada
    partial void OnSelectedRoleChanged(int value)
    {
        Entity.UserType = (UserType)value;
    }

    // De que usuario es el rol: lo dice la pantalla que abrio el modal
    public void SetUsuario(Guid usuarioId)
    {
        Entity.UsuarioId = usuarioId;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<IntNameModel>>($"{BaseUrl}/loadCombo");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Roles = new ObservableCollection<IntNameModel>(response.Response ?? new List<IntNameModel>());
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
}

public partial class CreateUsuarioRoleDialogViewModel : UsuarioRoleFormViewModel
{
    public CreateUsuarioRoleDialogViewModel(
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
