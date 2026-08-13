using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Cargue;

// Reutiliza el formulario de MAC para crear o editar detalles dentro de un cargue concreto.
public abstract partial class CargueDetailFormViewModel : CrudFormViewModel<CargueDetail>
{
    [ObservableProperty]
    private ObservableCollection<SerialStateType> _statuses = new(
        Enum.GetValues<SerialStateType>());

    protected override string BaseUrl => "api/v1/cargueDetails";

    protected CargueDetailFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override CargueDetail CreateEntity()
    {
        return new CargueDetail
        {
            Status = SerialStateType.Disponible
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.MacWlan))
        {
            return "Debes ingresar la MAC del equipo.";
        }

        return null;
    }

    // Asigna la recepcion que creo esta MAC antes de enviarla al Backend.
    public void SetCargueId(Guid cargueId)
    {
        Entity.CargueId = cargueId;
    }
}

public partial class CreateCargueDetailDialogViewModel : CargueDetailFormViewModel
{
    public CreateCargueDetailDialogViewModel(
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

public partial class EditCargueDetailDialogViewModel : CargueDetailFormViewModel
{
    public EditCargueDetailDialogViewModel(
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
