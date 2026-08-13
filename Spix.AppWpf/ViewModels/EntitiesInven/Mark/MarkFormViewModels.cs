using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.AppWpf.ViewModels.Shared;
using Spix.HttpService;
using MarkEntity = Spix.Domain.EntitiesGen.Mark;
using MarkModelEntity = Spix.Domain.EntitiesGen.MarkModel;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Mark;

// Comparte el CRUD de marcas para crear y editar sin duplicar llamadas al Backend.
public abstract class MarkFormViewModel : CrudFormViewModel<MarkEntity>
{
    protected override string BaseUrl => "api/v1/marks";

    protected MarkFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override MarkEntity CreateEntity()
    {
        return new MarkEntity { Active = true };
    }

    protected override string? GetValidationMessage()
    {
        return string.IsNullOrWhiteSpace(Entity.MarkName)
            ? "Debes ingresar el nombre de la marca."
            : null;
    }
}

// Comparte el CRUD de modelos, manteniendo la marca recibida desde el acordeon.
public abstract class MarkModelFormViewModel : CrudFormViewModel<MarkModelEntity>
{
    protected override string BaseUrl => "api/v1/marksmodels";

    protected MarkModelFormViewModel(
        IRepository repository,
        ModalService modalService,
        HttpResponseHandler responseHandler,
        AlertService alertService)
        : base(repository, modalService, responseHandler, alertService)
    {
    }

    protected override MarkModelEntity CreateEntity()
    {
        return new MarkModelEntity { Active = true };
    }

    protected override string? GetValidationMessage()
    {
        return string.IsNullOrWhiteSpace(Entity.MarkModelName)
            ? "Debes ingresar el nombre del modelo."
            : null;
    }
}

public partial class CreateMarkDialogViewModel : MarkFormViewModel
{
    public CreateMarkDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService) { }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(false);
}

public partial class EditMarkDialogViewModel : MarkFormViewModel
{
    public EditMarkDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService) { }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(true);
}

public partial class CreateMarkModelDialogViewModel : MarkModelFormViewModel
{
    public CreateMarkModelDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService) { }

    public void SetMark(Guid markId)
    {
        Entity.MarkId = markId;
    }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(false);
}

public partial class EditMarkModelDialogViewModel : MarkModelFormViewModel
{
    public EditMarkModelDialogViewModel(IRepository repository, ModalService modalService, HttpResponseHandler responseHandler, AlertService alertService)
        : base(repository, modalService, responseHandler, alertService) { }

    [RelayCommand]
    private async Task SaveAsync() => await SaveChangesAsync(true);
}
