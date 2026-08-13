using Microsoft.Extensions.DependencyInjection;
using Spix.AppWpf.SharedComponents;
using System.Windows;

namespace Spix.AppWpf.SharedServices;

// Centraliza mensajes de exito, error y confirmacion para todos los modulos desktop.
public class AlertService
{
    private readonly IServiceProvider _serviceProvider;

    public AlertService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task SuccessAsync(string title, string message)
    {
        return ShowAsync(title, message, AlertType.Success, "OK", false);
    }

    public Task ErrorAsync(string title, string message)
    {
        return ShowAsync(title, message, AlertType.Error, "OK", false);
    }

    public Task WarningAsync(string title, string message)
    {
        return ShowAsync(title, message, AlertType.Warning, "OK", false);
    }

    public async Task<bool> ConfirmAsync(
        string title,
        string message,
        string confirmText = "Confirmar")
    {
        return await ShowAsync(
            title,
            message,
            AlertType.Question,
            confirmText,
            true);
    }

    private Task<bool> ShowAsync(
        string title,
        string message,
        AlertType type,
        string confirmText,
        bool showCancel)
    {
        var alertWindow = _serviceProvider.GetRequiredService<SharedAlertWindow>();
        alertWindow.Owner = GetActiveWindow();
        alertWindow.Configure(title, message, type, confirmText, showCancel);
        var result = alertWindow.ShowDialog() == true;
        return Task.FromResult(result);
    }

    private static Window? GetActiveWindow()
    {
        return Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive)
            ?? Application.Current.MainWindow;
    }
}
