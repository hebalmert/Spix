using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.SecretProtection;
using Spix.AppInfra.Transactions;
using Spix.AppService.InterfacesSaaS;
using Spix.Domain.EntitiesSaaS;
using Spix.DomainLogic.Configuration;
using Spix.DomainLogic.EntitiesSaaSDTO;
using Spix.DomainLogic.ModelUtility;

namespace Spix.AppService.ImplementSaaS;

/// <summary>
/// Administra las credenciales globales del SaaS.
/// Los valores se cifran antes de ser almacenados y los secretos son solo escritura.
/// </summary>
public class SystemSettingService : ISystemSettingService
{
    private static readonly (string Key, string Label, string Category, bool IsSecret)[] SystemCatalog =
    {
        ("SendGrid:SendGridApiKey", "API Key", "SendGrid", true),
        ("SendGrid:SendGridFrom", "Sender email", "SendGrid", false),
        ("SendGrid:SendGridNombre", "Sender name", "SendGrid", false)
    };

    private static readonly (string Key, string Label, string Category, bool IsSecret)[] PaymentCatalog =
    {
        ("PaymentGateway:Active", "Active gateway", "Payment gateway", false),
        ("MercadoPago:AccessToken", "Access Token", "MercadoPago", true),
        ("MercadoPago:WebhookSecret", "Webhook secret", "MercadoPago", true),
        ("MercadoPago:CurrencyId", "Currency", "MercadoPago", false),
        ("MercadoPago:BackUrl", "Return URL", "MercadoPago", false),
        ("MercadoPago:NotificationUrl", "Webhook URL", "MercadoPago", false),
        ("Wompi:PublicKey", "Public key", "Wompi", false),
        ("Wompi:EventsSecret", "Events secret", "Wompi", true),
        ("Wompi:IntegritySecret", "Integrity secret", "Wompi", true),
        ("Wompi:CheckoutUrl", "Checkout URL", "Wompi", false),
        ("Wompi:RedirectUrl", "Return URL", "Wompi", false)
    };

    private readonly DataContext _context;
    private readonly ITransactionManager _transaction;
    private readonly ISecretProtector _protector;
    private readonly ISecretStore _secrets;
    private readonly HttpErrorHandler _errors;
    private readonly IStringLocalizer _localizer;

    public SystemSettingService(
        DataContext context,
        ITransactionManager transaction,
        ISecretProtector protector,
        ISecretStore secrets,
        HttpErrorHandler errors,
        IStringLocalizer localizer)
    {
        _context = context;
        _transaction = transaction;
        _protector = protector;
        _secrets = secrets;
        _errors = errors;
        _localizer = localizer;
    }

    public Task<ActionResponse<IEnumerable<SecretSettingDTO>>> GetSystemAsync()
    {
        return GetAsync(SystemCatalog);
    }

    public Task<ActionResponse<IEnumerable<SecretSettingDTO>>> GetPaymentAsync()
    {
        return GetAsync(PaymentCatalog);
    }

    public Task<ActionResponse<bool>> SaveSystemAsync(
        IEnumerable<SecretSettingDTO> items,
        string username)
    {
        return SaveAsync(items, username, SystemCatalog);
    }

    public Task<ActionResponse<bool>> SavePaymentAsync(
        IEnumerable<SecretSettingDTO> items,
        string username)
    {
        return SaveAsync(items, username, PaymentCatalog);
    }

    private async Task<ActionResponse<IEnumerable<SecretSettingDTO>>> GetAsync(
        IEnumerable<(string Key, string Label, string Category, bool IsSecret)> catalog)
    {
        try
        {
            var items = new List<SecretSettingDTO>();

            foreach (var entry in catalog)
            {
                string? value = _secrets.Get(entry.Key);

                if (entry.Key == "PaymentGateway:Active" &&
                    string.IsNullOrWhiteSpace(value))
                {
                    value = "MercadoPago";
                }

                items.Add(new SecretSettingDTO
                {
                    Key = entry.Key,
                    Label = entry.Label,
                    Category = entry.Category,
                    IsSecret = entry.IsSecret,
                    HasValue = !string.IsNullOrWhiteSpace(value),
                    Value = entry.IsSecret ? null : value
                });
            }

            return new ActionResponse<IEnumerable<SecretSettingDTO>>
            {
                WasSuccess = true,
                Result = items
            };
        }
        catch (Exception exception)
        {
            return await _errors.HandleErrorAsync<IEnumerable<SecretSettingDTO>>(exception);
        }
    }

    private async Task<ActionResponse<bool>> SaveAsync(
        IEnumerable<SecretSettingDTO> items,
        string username,
        IEnumerable<(string Key, string Label, string Category, bool IsSecret)> catalog)
    {
        try
        {
            SecretSettingDTO? activeGateway = items.FirstOrDefault(
                item => string.Equals(
                    item.Key,
                    "PaymentGateway:Active",
                    StringComparison.OrdinalIgnoreCase));

            if (activeGateway != null &&
                !string.Equals(activeGateway.Value, "MercadoPago", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(activeGateway.Value, "Wompi", StringComparison.OrdinalIgnoreCase))
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "The active payment gateway is invalid."
                };
            }

            var catalogByKey = catalog.ToDictionary(
                entry => entry.Key,
                StringComparer.OrdinalIgnoreCase);

            DateTime now = DateTime.UtcNow;

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Key) ||
                    !catalogByKey.TryGetValue(item.Key, out var metadata) ||
                    string.IsNullOrWhiteSpace(item.Value))
                {
                    continue;
                }

                string encryptedValue = _protector.Protect(item.Value.Trim())!;

                SystemSetting? current = await _context.SystemSettings
                    .FirstOrDefaultAsync(setting => setting.Key == metadata.Key);

                if (current == null)
                {
                    _context.SystemSettings.Add(new SystemSetting
                    {
                        Key = metadata.Key,
                        Value = encryptedValue,
                        Category = metadata.Category,
                        IsSecret = metadata.IsSecret,
                        UpdatedAt = now,
                        UpdatedBy = username
                    });

                    continue;
                }

                current.Value = encryptedValue;
                current.Category = metadata.Category;
                current.IsSecret = metadata.IsSecret;
                current.UpdatedAt = now;
                current.UpdatedBy = username;
            }

            await _transaction.SaveChangesAsync();
            _secrets.Invalidate();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception exception)
        {
            return await _errors.HandleErrorAsync<bool>(exception);
        }
    }
}
