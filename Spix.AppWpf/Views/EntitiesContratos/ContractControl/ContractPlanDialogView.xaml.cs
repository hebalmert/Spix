using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesContratos.ContractControl;
using Spix.Domain.EntitiesGen;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesContratos.ContractControl;

// El plan del cliente. Al elegir la categoria se bajan SUS planes.
//
// Va con SelectionChanged y enlace de una via, no con TwoWay: en TwoWay el enlace escribe
// el valor antes de que llegue el evento y la cascada nunca carga.
public partial class ContractPlanDialogView : UserControl, ISharedModalContent
{
    private readonly ContractPlanDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public ContractPlanDialogView(ContractPlanDialogViewModel viewModel)
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

    private async void CategorySelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is PlanCategory categoria)
        {
            await _viewModel.ChangeCategoryAsync(categoria.PlanCategoryId);
        }
    }
}
