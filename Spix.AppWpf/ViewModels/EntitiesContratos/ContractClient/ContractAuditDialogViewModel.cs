using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppWpf.SharedServices;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesContratos.ContractClient;

// La bitacora de un contrato: todo lo que le ha pasado, en una sola linea de tiempo.
//
// No sale de la tabla de contratos: los modulos la van anotando en ContractAudit segun
// ocurren las cosas —creacion, cambios de estado, firmas, suspensiones, exoneraciones—,
// asi que aqui se lee un solo endpoint y ya viene todo junto y en orden.
public partial class ContractAuditDialogViewModel : ObservableObject
{
    private const string BaseUrl = "api/v1/contractaudit";

    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;
    private readonly ModalService _modalService;
    private readonly LanguageService _languageService;

    [ObservableProperty]
    private ContractAuditListDTO? _data;

    [ObservableProperty]
    private ObservableCollection<ContractAuditRow> _events = new();

    [ObservableProperty]
    private bool _isLoading;

    public bool HasData => Data is not null;

    public bool HasEvents => Events.Count > 0;

    public string Header => Data is null
        ? string.Empty
        : $"#{Data.ControlContrato} · {Data.ClientName}";

    public string SubHeader => Data is null
        ? string.Empty
        : $"{Data.ClientDocument} · {Data.ContractAddress}";

    public ContractAuditDialogViewModel(
        IRepository repository,
        HttpResponseHandler responseHandler,
        ModalService modalService,
        LanguageService languageService)
    {
        _repository = repository;
        _responseHandler = responseHandler;
        _modalService = modalService;
        _languageService = languageService;
    }

    public async Task InitializeAsync(Guid contractClientId)
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<ContractAuditListDTO>($"{BaseUrl}/{contractClientId}");
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Data = response.Response;

            Events = new ObservableCollection<ContractAuditRow>(
                (Data?.Events ?? new List<ContractAuditDTO>())
                    .Select(x => new ContractAuditRow(x, _languageService)));

            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(HasEvents));
            OnPropertyChanged(nameof(Header));
            OnPropertyChanged(nameof(SubHeader));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await _modalService.CloseAsync(ModalResult.Cancel());
    }
}

// Un paso de la linea de tiempo, ya listo para pintar
public class ContractAuditRow
{
    public string When { get; }

    public string What { get; }

    public string? Detail { get; }

    public string Who { get; }

    public bool HasDetail => !string.IsNullOrWhiteSpace(Detail);

    public ContractAuditRow(ContractAuditDTO item, LanguageService idioma)
    {
        When = item.DateEvent.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

        //El nombre del evento sale del archivo de traducciones compartido
        What = idioma.Text($"ContractEvent_{item.EventType}");

        Detail = item.Detail;

        Who = string.IsNullOrWhiteSpace(item.SourceIp)
            ? item.UserByName ?? string.Empty
            : $"{item.UserByName} · {item.SourceIp}";
    }
}
