using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EnumTypes;
using Spix.HttpService;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;

// El Consentimiento y el Contrato: el mismo modal con otra plantilla, igual que la web.
//
// Al abrirlo el SERVIDOR genera el PDF (no se arma aqui) y devuelve su direccion. Si
// todavia no esta firmado, debajo aparece el espacio para firmar; la firma se manda en
// Base64 y el servidor la estampa en el documento.
public partial class ContractDocumentDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractdocuments";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly AlertService _alertService;

    [ObservableProperty]
    private ContractSignedDocument? _document;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaving;

    public bool HasDocument => !string.IsNullOrWhiteSpace(Document?.FileFullPath);

    // Firmado ya no se vuelve a firmar: solo se lee
    public bool CanSign => Document is not null && !Document.Signed;

    public string? FileUrl => Document?.FileFullPath;

    // La pantalla pinta el PDF cuando esta listo
    public event EventHandler<string>? DocumentReady;

    public ContractDocumentDialogViewModel(
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

    public async Task InitializeAsync(Guid contractClientId, ContractDocumentType tipo)
    {
        IsLoading = true;

        try
        {
            //El PDF lo arma el servidor con la plantilla que corresponda
            var response = await _repository.PostAsync<object, ContractSignedDocument>(
                $"{BaseUrl}/generate/{contractClientId}/type/{tipo}", new { });

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Document = response.Response;

            OnPropertyChanged(nameof(HasDocument));
            OnPropertyChanged(nameof(CanSign));
            OnPropertyChanged(nameof(FileUrl));

            if (!string.IsNullOrWhiteSpace(FileUrl))
            {
                DocumentReady?.Invoke(this, FileUrl);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    // La firma la entrega la pantalla, ya convertida a Base64
    public async Task SignAsync(string? base64)
    {
        if (Document is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(base64))
        {
            await _alertService.WarningAsync("Firma", "Debe firmar en el recuadro antes de guardar.");
            return;
        }

        var confirmado = await _alertService.ConfirmAsync(
            "Firma",
            "Desea guardar la firma del documento?",
            "Guardar");

        if (!confirmado)
        {
            return;
        }

        IsSaving = true;

        try
        {
            Document.SignatureBase64 = base64;

            var response = await _repository.PostAsync<ContractSignedDocument, ContractSignedDocument>(
                $"{BaseUrl}/sign", Document);

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Document = response.Response;

            await _alertService.SuccessAsync("Firma", "El documento fue firmado correctamente.");

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
