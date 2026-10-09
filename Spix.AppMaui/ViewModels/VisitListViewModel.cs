using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spix.AppMaui.Services;
using Spix.Domain.EntitiesSchedule;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppMaui.ViewModels;

//El servidor manda la jornada completa en una sola llamada; cada pestana se queda con
//su parte. Asi en la calle se baja una vez y se cambia de pestana sin red.
public abstract partial class VisitListViewModel : ObservableObject
{
    private const string BaseUrl = "api/v4/techvisits";

    private readonly IRepository _repository;
    private readonly ApiResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<VisitRow> _rows = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isEmpty;

    public abstract string Title { get; }

    public abstract string EmptyText { get; }

    protected VisitListViewModel(IRepository repository, ApiResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    //Que visitas le tocan a esta pestana
    protected abstract bool Entra(TechVisitDto visita);

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;

        try
        {
            var response = await _repository.GetAsync<List<TechVisitDto>>(BaseUrl);
            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            var lista = (response.Response ?? new List<TechVisitDto>())
                .Where(Entra)
                .Select(x => new VisitRow(x))
                .ToList();

            Rows = new ObservableCollection<VisitRow>(lista);
            IsEmpty = lista.Count == 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenAsync(VisitRow? fila)
    {
        if (fila is null)
        {
            return;
        }

        await Shell.Current.GoToAsync($"visit?id={fila.Item.ServiceRequestId}");
    }

    //Llamar al cliente es un toque, no copiar el numero a mano
    [RelayCommand]
    private void Call(VisitRow? fila)
    {
        if (fila is null || string.IsNullOrWhiteSpace(fila.Item.ContactPhone))
        {
            return;
        }

        try
        {
            PhoneDialer.Default.Open(fila.Item.ContactPhone);
        }
        catch
        {
            //El telefono no puede marcar: no vale la pena interrumpir al tecnico por esto
        }
    }

    //Abrir el mapa con la direccion del contrato
    [RelayCommand]
    private async Task MapAsync(VisitRow? fila)
    {
        if (fila?.Item.ContractLatitude is null || fila.Item.ContractLongitude is null)
        {
            return;
        }

        var destino = new Location((double)fila.Item.ContractLatitude.Value,
                                   (double)fila.Item.ContractLongitude.Value);

        await Map.Default.OpenAsync(destino, new MapLaunchOptions
        {
            Name = fila.Item.ClientFullName,
            NavigationMode = NavigationMode.Driving
        });
    }
}
