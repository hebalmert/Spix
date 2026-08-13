using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.HttpService;
using EstratoSocialEntity = Spix.Domain.EntitiesGen.EstratoSocial;

namespace Spix.AppWpf.ViewModels.EntitiesGen.EstratoSocial;

// Comparte el CRUD de Estrato Social entre Create y Edit.
public abstract class EstratoSocialFormViewModel : CrudFormViewModel<EstratoSocialEntity>
{
    protected override string BaseUrl
    {
        get
        {
            return "api/v1/estratossociales";
        }
    }

    protected EstratoSocialFormViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override EstratoSocialEntity CreateEntity()
    {
        return new EstratoSocialEntity();
    }

    protected override string? GetValidationMessage()
    {
        return string.IsNullOrWhiteSpace(Entity.EstratoSocialName)
            ? "Debes ingresar el nombre del estrato social."
            : null;
    }
}

public partial class CreateEstratoSocialDialogViewModel : EstratoSocialFormViewModel
{
    public CreateEstratoSocialDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditEstratoSocialDialogViewModel : EstratoSocialFormViewModel
{
    public EditEstratoSocialDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}
