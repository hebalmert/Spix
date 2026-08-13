using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Serial;

// Presenta los estados permitidos para actualizar una MAC sin alterar su cargue de origen.
public partial class EditSerialDialogViewModel : CrudFormViewModel<CargueDetail>
{
    [ObservableProperty]
    private ObservableCollection<SerialStateType> _statuses = new(
        Enum.GetValues<SerialStateType>());

    protected override string BaseUrl => "api/v1/cargueDetails";

    public EditSerialDialogViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override CargueDetail CreateEntity()
    {
        return new CargueDetail();
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.MacWlan))
        {
            return "Debes ingresar la MAC del equipo.";
        }

        return null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}
