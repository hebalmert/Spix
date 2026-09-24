using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.HttpService;
using TaxEntity = Spix.Domain.EntitiesGen.Tax;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Tax;

// Los campos del impuesto: los mismos de Blazor (nombre, tasa y activo).
public abstract partial class TaxFormViewModel : CrudFormViewModel<TaxEntity>
{
    protected override string BaseUrl => "api/v1/taxes";

    protected TaxFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override TaxEntity CreateEntity()
    {
        return new TaxEntity
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(Entity.TaxName))
        {
            return "Debes ingresar el nombre del impuesto.";
        }

        if (Entity.Rate < 0)
        {
            return "La tasa no puede ser negativa.";
        }

        return null;
    }

    public async Task LoadForEditAsync(Guid id)
    {
        await LoadAsync(id);
    }
}

public partial class CreateTaxDialogViewModel : TaxFormViewModel
{
    public CreateTaxDialogViewModel(
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

public partial class EditTaxDialogViewModel : TaxFormViewModel
{
    public EditTaxDialogViewModel(
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
