using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Spix.AppWpf.SharedComponents;

// Reutiliza la busqueda y limpieza de filtros en todos los indices paginados.
public partial class SharedSearchFilter : UserControl
{
    public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
        nameof(SearchText),
        typeof(string),
        typeof(SharedSearchFilter),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SearchCommandProperty = DependencyProperty.Register(
        nameof(SearchCommand),
        typeof(ICommand),
        typeof(SharedSearchFilter));

    public static readonly DependencyProperty ClearCommandProperty = DependencyProperty.Register(
        nameof(ClearCommand),
        typeof(ICommand),
        typeof(SharedSearchFilter));

    public SharedSearchFilter()
    {
        InitializeComponent();
    }

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public ICommand? ClearCommand
    {
        get => (ICommand?)GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    // La pantalla la llama con Ctrl+F para poner el cursor en la caja
    public void FocusSearch()
    {
        SearchBox.Focus();
        SearchBox.SelectAll();
    }

    // Enter busca: no hay que soltar el teclado para ir hasta el boton
    private void SearchBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (SearchCommand?.CanExecute(null) == true)
        {
            SearchCommand.Execute(null);
        }

        e.Handled = true;
    }
}
