using System.Windows;
using System.Windows.Controls;

namespace Spix.AppWpf.SharedComponents;

// Muestra un spinner local mientras una vista obtiene datos desde el Backend.
public partial class SharedLoading : UserControl
{
    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
        nameof(IsLoading),
        typeof(bool),
        typeof(SharedLoading),
        new PropertyMetadata(false));

    public SharedLoading()
    {
        InitializeComponent();
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
}
