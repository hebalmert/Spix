using CommunityToolkit.Mvvm.ComponentModel;
using Spix.AppWpf.SharedServices;
using Spix.DomainLogic.ItemsGeneric;
using Spix.HttpService;
using System.Collections.ObjectModel;

namespace Spix.AppWpf.ViewModels.EntitiesInven.Transfer;

// Los equipos que viajaron en una linea del traslado.
//
// Sale del historico (TransferDetailSerials), no del serial: el serial solo sabe en que
// bodega esta AHORA, asi que si despues se vuelve a mover, este traslado sigue mostrando
// lo que movio en su momento.
//
// Va por v2 porque es la API del escritorio.
public partial class TransferDetailSerialsViewModel : ObservableObject
{
    private readonly IRepository _repository;
    private readonly HttpResponseHandler _responseHandler;

    [ObservableProperty]
    private ObservableCollection<GuidItemModel> _serials = new();

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    //Para el aviso de "no movio seriales": solo cuando ya se consulto y vino vacio
    [ObservableProperty]
    private bool _isEmpty;

    public TransferDetailSerialsViewModel(IRepository repository, HttpResponseHandler responseHandler)
    {
        _repository = repository;
        _responseHandler = responseHandler;
    }

    public async Task LoadAsync(Guid transferDetailsId)
    {
        IsLoading = true;

        try
        {
            var response = await _repository.GetAsync<List<GuidItemModel>>(
                $"api/v2/transferDetails/serials/moved/{transferDetailsId}");

            if (await _responseHandler.HandleErrorAsync(response))
            {
                return;
            }

            Serials = new ObservableCollection<GuidItemModel>(
                response.Response ?? new List<GuidItemModel>());

            IsEmpty = Serials.Count == 0;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
