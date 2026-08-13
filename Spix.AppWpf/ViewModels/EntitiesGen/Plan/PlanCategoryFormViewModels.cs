using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.HttpService;
using PlanCategoryEntity = Spix.Domain.EntitiesGen.PlanCategory;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Plan;

// Comparte el CRUD de Categoria Plan entre Create y Edit.
public abstract class PlanCategoryFormViewModel : CrudFormViewModel<PlanCategoryEntity>
{
    protected override string BaseUrl
    {
        get
        {
            return "api/v1/plancategories";
        }
    }

    protected PlanCategoryFormViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override PlanCategoryEntity CreateEntity()
    {
        return new PlanCategoryEntity
        {
            Active = true
        };
    }

    protected override string? GetValidationMessage()
    {
        return string.IsNullOrWhiteSpace(Entity.PlanCategoryName)
            ? "Debes ingresar el nombre de la categoria de planes."
            : null;
    }
}

public partial class CreatePlanCategoryDialogViewModel : PlanCategoryFormViewModel
{
    public CreatePlanCategoryDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(false);
    }
}

public partial class EditPlanCategoryDialogViewModel : PlanCategoryFormViewModel
{
    public EditPlanCategoryDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        await SaveChangesAsync(true);
    }
}
