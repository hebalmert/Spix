using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.HttpService;
using RegisterEntity = Spix.Domain.EntitiesGen.Register;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Register;

// Los nueve consecutivos del formulario de Blazor: desde que numero arranca cada
// documento de la corporacion.
public abstract partial class RegisterFormViewModel : CrudFormViewModel<RegisterEntity>
{
    protected override string BaseUrl => "api/v1/registers";

    protected RegisterFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override RegisterEntity CreateEntity()
    {
        return new RegisterEntity();
    }

    protected override string? GetValidationMessage()
    {
        //Un consecutivo no puede ir hacia atras
        if (Entity.Contratos < 0 ||
            Entity.Solicitudes < 0 ||
            Entity.Cargue < 0 ||
            Entity.Egresos < 0 ||
            Entity.PagoContratista < 0 ||
            Entity.Adelantado < 0 ||
            Entity.Exonerado < 0 ||
            Entity.NotaCobro < 0 ||
            Entity.Factura < 0)
        {
            return "Los consecutivos no pueden ser negativos.";
        }

        return null;
    }

    public async Task LoadForEditAsync(Guid id)
    {
        await LoadAsync(id);
    }
}

public partial class CreateRegisterDialogViewModel : RegisterFormViewModel
{
    public CreateRegisterDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditRegisterDialogViewModel : RegisterFormViewModel
{
    public EditRegisterDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}
