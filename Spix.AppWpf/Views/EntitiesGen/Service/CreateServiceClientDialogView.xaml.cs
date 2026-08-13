using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Service;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Service;

// Crea un servicio y recibe la categoria seleccionada desde la vista de detalle.
public partial class CreateServiceClientDialogView : UserControl, ISharedModalContent
{
    private readonly CreateServiceClientDialogViewModel _viewModel;
    private Guid _serviceCategoryId;
    private bool _isLoaded;

    public CreateServiceClientDialogView(CreateServiceClientDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("ServiceCategoryId", out var value) == true && value is Guid serviceCategoryId)
        {
            _serviceCategoryId = serviceCategoryId;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _serviceCategoryId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        _viewModel.SetServiceCategory(_serviceCategoryId);
        await _viewModel.InitializeAsync();
    }
}
