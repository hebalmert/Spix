using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesGen.DocumentType;

// Inicializa un tipo de documento activo y lo guarda mediante POST.
public partial class CreateDocumentTypeDialogViewModel : DocumentTypeFormViewModel
{
    public CreateDocumentTypeDialogViewModel(
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
