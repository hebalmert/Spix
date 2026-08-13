using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesInven.Mark;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesInven.Mark;

// Asigna la marca abierta al modelo nuevo antes de guardar.
public partial class CreateMarkModelDialogView : UserControl, ISharedModalContent
{
    private readonly CreateMarkModelDialogViewModel _viewModel;
    private Guid _markId;
    private bool _isLoaded;

    public CreateMarkModelDialogView(CreateMarkModelDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("MarkId", out var value) == true && value is Guid markId) _markId = markId;
    }

    private void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _markId == Guid.Empty) return;
        _isLoaded = true;
        _viewModel.SetMark(_markId);
    }
}
