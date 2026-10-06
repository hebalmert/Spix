using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractControl;

// La MAC del equipo de un contrato, en cascada: Categoria equipo -> Equipo -> MAC.
public partial class ContractMacDialogView : UserControl, ISharedModalContent
{
    private readonly ContractMacDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public ContractMacDialogView(ContractMacDialogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters is not null && parameters.TryGetValue("ContractClientId", out var valor) && valor is Guid id)
        {
            _id = id;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeAsync(_id);
    }

    //El evento manda, no el enlace: es lo unico que llega DESPUES de que el usuario elige,
    //y por eso puede limpiar el combo de abajo y pedir su lista nueva.
    private async void CategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Elegido(e) is PieceOption opcion)
        {
            await _viewModel.ChangeCategoryAsync(opcion.Value);
        }
    }

    private async void ProductSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Elegido(e) is PieceOption opcion)
        {
            await _viewModel.ChangeProductAsync(opcion.Value);
        }
    }

    private void MacSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (Elegido(e) is PieceOption opcion)
        {
            _viewModel.ChangeMac(opcion.Value);
        }
    }

    //Al reemplazar la lista el combo dispara su evento con la seleccion vacia: eso no es una
    //eleccion del usuario y no debe borrar nada.
    private static PieceOption? Elegido(SelectionChangedEventArgs e)
    {
        return e.AddedItems.Count > 0 ? e.AddedItems[0] as PieceOption : null;
    }
}
