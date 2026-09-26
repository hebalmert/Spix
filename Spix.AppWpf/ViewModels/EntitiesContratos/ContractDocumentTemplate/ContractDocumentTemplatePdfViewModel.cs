using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractDocumentTemplate;

// El visor del PDF de la plantilla: solo se mira, no se edita.
// El PDF lo arma el servidor y aqui se abre por su direccion en el navegador embebido.
public partial class ContractDocumentTemplatePdfViewModel : ObservableObject
{
    private readonly ModalService _modalService;

    [ObservableProperty]
    private bool _isLoading;

    public ContractDocumentTemplatePdfViewModel(ModalService modalService)
    {
        _modalService = modalService;
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
