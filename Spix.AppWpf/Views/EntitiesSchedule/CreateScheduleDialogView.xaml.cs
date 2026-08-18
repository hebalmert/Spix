using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesSchedule;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesSchedule;

// Inicializa una agenda nueva con el dia seleccionado dentro del calendario.
public partial class CreateScheduleDialogView : UserControl, ISharedModalContent
{
    private readonly CreateScheduleDialogViewModel _viewModel;
    private DateTime _selectedDate = DateTime.Today;
    private bool _loaded;

    public CreateScheduleDialogView(CreateScheduleDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters == null || !parameters.TryGetValue("SelectedDate", out object? value))
        {
            return;
        }

        DateTime.TryParse(
            value?.ToString(),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out _selectedDate);
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await _viewModel.InitializeForCreateAsync(_selectedDate);
    }
}
