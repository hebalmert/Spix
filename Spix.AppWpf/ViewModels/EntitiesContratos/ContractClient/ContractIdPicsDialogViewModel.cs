using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;

// Las dos fotos del documento del cliente: el frente y el reverso.
//
// Es el MISMO modal para crear y para editar, igual que en la web: si el contrato ya
// tiene fotos llega su id y se actualizan, y si no, se crean. El boton del listado es uno
// solo por eso.
public partial class ContractIdPicsDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractidpics";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ContractIDPic _entity = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    private bool _esEdicion;

    public ContractIdPicsDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        AlertService alertService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _alertService = alertService;
    }

    public async Task InitializeAsync(Guid contractClientId, Guid? idPics)
    {
        IsLoading = true;

        try
        {
            _esEdicion = idPics.HasValue && idPics.Value != Guid.Empty;

            if (!_esEdicion)
            {
                Entity = new ContractIDPic { ContractClientId = contractClientId };
                return;
            }

            var response = await _repository.GetAsync<ContractIDPic>($"{BaseUrl}/{idPics!.Value}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                await _modalService.CloseAsync(ModalResult.Cancel());
                return;
            }

            Entity = response.Response ?? new ContractIDPic { ContractClientId = contractClientId };
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Las entrega el selector, ya en el Base64 que espera el Backend
    public void SetFront(string base64)
    {
        Entity.ImgFrontBase64 = base64;
    }

    public void SetBack(string base64)
    {
        Entity.ImgBackBase64 = base64;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        //Al crear hacen falta las dos caras; al editar se puede cambiar solo una
        if (!_esEdicion &&
            (string.IsNullOrWhiteSpace(Entity.ImgFrontBase64) || string.IsNullOrWhiteSpace(Entity.ImgBackBase64)))
        {
            await _alertService.WarningAsync(
                "Documento del cliente",
                "Debe cargar las dos caras del documento: el frente y el reverso.");
            return;
        }

        IsSaving = true;

        try
        {
            var response = _esEdicion
                ? await _repository.PutAsync(BaseUrl, Entity)
                : await _repository.PostAsync(BaseUrl, Entity);

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            await _modalService.CloseAsync(ModalResult.Ok());
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}
