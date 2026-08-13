using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.HttpService;
using ServiceCategoryEntity = Spix.Domain.EntitiesGen.ServiceCategory;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Service;

// Comparte el CRUD de Categoria Servicio entre Create y Edit.
public abstract class ServiceCategoryFormViewModel : CrudFormViewModel<ServiceCategoryEntity>
{
    protected override string BaseUrl
    {
        get
        {
            return "api/v1/servicecategories";
        }
    }

    protected ServiceCategoryFormViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override ServiceCategoryEntity CreateEntity()
    {
        return new ServiceCategoryEntity
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        return string.IsNullOrWhiteSpace(Entity.Name)
            ? "Debes ingresar el nombre de la categoria de servicios."
            : null;
    }
}

public partial class CreateServiceCategoryDialogViewModel : ServiceCategoryFormViewModel
{
    public CreateServiceCategoryDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditServiceCategoryDialogViewModel : ServiceCategoryFormViewModel
{
    public EditServiceCategoryDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}
