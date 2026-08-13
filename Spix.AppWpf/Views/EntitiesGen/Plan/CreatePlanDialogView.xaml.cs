using Spix.AppWpf.SharedComponents;
using Spix.AppWpf.ViewModels.EntitiesGen.Plan;
using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.Views.EntitiesGen.Plan;

// Crea un plan y recibe la categoria seleccionada desde la vista de detalle.
public partial class CreatePlanDialogView : UserControl, ISharedModalContent
{
    private readonly CreatePlanDialogViewModel _viewModel;
    private Guid _planCategoryId;
    private bool _isLoaded;

    public CreatePlanDialogView(CreatePlanDialogViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Loaded += LoadDialog;
    }

    public void SetParameters(IReadOnlyDictionary<string, object>? parameters)
    {
        if (parameters?.TryGetValue("PlanCategoryId", out var value) == true && value is Guid planCategoryId)
        {
            _planCategoryId = planCategoryId;
        }
    }

    private async void LoadDialog(object sender, RoutedEventArgs e)
    {
        if (_isLoaded || _planCategoryId == Guid.Empty)
        {
            return;
        }

        _isLoaded = true;
        _viewModel.SetPlanCategory(_planCategoryId);
        await _viewModel.InitializeAsync();
    }
}
