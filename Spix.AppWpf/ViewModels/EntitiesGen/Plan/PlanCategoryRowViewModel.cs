using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PlanCategoryEntity = Spix.Domain.EntitiesGen.PlanCategory;
using PlanEntity = Spix.Domain.EntitiesGen.Plan;

namespace Spix.AppWpf.ViewModels.EntitiesGen.Plan;

// Agrega el estado visual del acordeon sin alterar la entidad enviada por el Backend.
public class PlanCategoryRowViewModel : PlanCategoryEntity, INotifyPropertyChanged
{
    private bool _isExpanded;
    private bool _isPlansLoading;
    private ObservableCollection<PlanEntity> _plans = new();
    private string _plansMessage = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool IsPlansLoading
    {
        get => _isPlansLoading;
        set => SetProperty(ref _isPlansLoading, value);
    }

    public ObservableCollection<PlanEntity> Plans
    {
        get => _plans;
        set
        {
            if (!SetProperty(ref _plans, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasPlans));
        }
    }

    public string PlansMessage
    {
        get => _plansMessage;
        set
        {
            if (!SetProperty(ref _plansMessage, value))
            {
                return;
            }

            OnPropertyChanged(nameof(HasPlansMessage));
        }
    }

    public bool HasPlans => Plans.Count > 0;

    public bool HasPlansMessage => !string.IsNullOrWhiteSpace(PlansMessage);

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
