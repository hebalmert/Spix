using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSchedule;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule;

// Recupera la agenda solicitada antes de permitir editarla o eliminarla.
public partial class EditScheduleDialogView : UserControl, ISharedModalContent
{
    private readonly EditScheduleDialogViewModel _viewModel;
    private Guid _id;
    private bool _loaded;

    public EditScheduleDialogView(EditScheduleDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters != null && parameters.TryGetValue("Id", out object? value) && value is Guid id)
        {
            _id = id;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded || _id == Guid.Empty)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeForEditAsync(_id);
    }
}
