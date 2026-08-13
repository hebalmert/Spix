using FontAwesome.Net.Generators;
using Spix.AppWpf.SharedServices;
using System.Windows;
using System.Windows.Media;

namespace Spix.AppWpf.SharedComponents;

// Presenta mensajes y confirmaciones con una apariencia uniforme inspirada en SweetAlert.
public partial class SharedAlertWindow : Window
{
    public SharedAlertWindow()
    {
        InitializeComponent();
    }

    public void Configure(
        string title,
        string message,
        AlertType type,
        string confirmText,
        bool showCancel)
    {
        AlertTitleText.Text = title;
        AlertMessageText.Text = message;
        ConfirmButton.Content = confirmText;
        CancelButton.Visibility = showCancel
            ? Visibility.Visible
            : Visibility.Collapsed;

        ConfigureIcon(type);
    }

    private void ConfigureIcon(AlertType type)
    {
        switch (type)
        {
            case AlertType.Success:
                AlertIcon.Icon = FontAwesomeIcon.CircleCheck;
                AlertIcon.Foreground = CreateBrush("#51D88A");
                break;

            case AlertType.Warning:
                AlertIcon.Icon = FontAwesomeIcon.CircleExclamation;
                AlertIcon.Foreground = CreateBrush("#F5B461");
                break;

            case AlertType.Question:
                AlertIcon.Icon = FontAwesomeIcon.CircleQuestion;
                AlertIcon.Foreground = CreateBrush("#22C1E8");
                break;

            default:
                AlertIcon.Icon = FontAwesomeIcon.CircleXmark;
                AlertIcon.Foreground = CreateBrush("#FF718D");
                break;
        }
    }

    private void ConfirmClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private static Brush CreateBrush(string color)
    {
        return (Brush)new BrushConverter().ConvertFromString(color)!;
    }
}
