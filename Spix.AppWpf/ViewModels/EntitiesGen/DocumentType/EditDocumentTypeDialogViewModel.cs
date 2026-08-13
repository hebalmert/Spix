using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesGen.DocumentType;

// Carga un tipo de documento existente y lo actualiza mediante PUT.
public partial class EditDocumentTypeDialogViewModel : DocumentTypeFormViewModel
{
    public EditDocumentTypeDialogViewModel(
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
