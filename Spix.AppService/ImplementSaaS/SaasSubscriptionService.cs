using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Payments;
using Spix.AppInfra.SecretProtection;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesSaaS;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesSaaS;
using Spix.DomainLogic.Configuration;
using Spix.DomainLogic.EntitiesSaaSDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.SettingModels;
using Spix.xNotification.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Spix.AppService.ImplementSaaS;

public class SaasSubscriptionService : ISaasSubscriptionService
{
    private const int TrialDays = 14;

    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IEmailHelper _emailHelper;
    private readonly ISecretProtector _secretProtector;
    private readonly ISecretStore _secretStore;
    private readonly IConfiguration _configuration;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public SaasSubscriptionService(DataContext context, ITransactionManager transactionManager,
        IUserHelper userHelper, IEmailHelper emailHelper, ISecretProtector secretProtector,
        ISecretStore secretStore,
        IConfiguration configuration, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _emailHelper = emailHelper;
        _secretProtector = secretProtector;
        _secretStore = secretStore;
        _configuration = configuration;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<PublicSoftPlanDTO>>> GetPublicPlansAsync()
    {
        try
        {
            List<PublicSoftPlanDTO> plans = await _context.SoftPlans
                .AsNoTracking()
                .Where(x => x.Active)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ClientsCount)
                .Select(x => new PublicSoftPlanDTO
                {
                    SoftPlanId = x.SoftPlanId,
                    Name = x.Name!,
                    MonthlyPrice = x.Price,
                    AnnualPrice = x.AnnualPrice ?? (x.Price * 10),
                    ContractLimit = x.ClientsCount,
                    PublicDescription = x.PublicDescription,
                    IsRecommended = x.IsRecommended
                })
                .ToListAsync();

            return new ActionResponse<IEnumerable<PublicSoftPlanDTO>>
            {
                WasSuccess = true,
                Result = plans
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<PublicSoftPlanDTO>>(ex);
        }
    }

    public async Task<ActionResponse<SubscriptionAccessDTO>> StartTrialAsync(StartTrialRequestDTO request, string frontUrl)
    {
        if (await _context.Corporations.AnyAsync(x => x.NroDocument == request.CorporationDocument || x.Name == request.CorporationName))
        {
            return FailureAccess("Ya existe una corporacion registrada con este nombre o documento.");
        }

        User? existingUser = await _userHelper.GetUserByUserNameAsync(request.UserName);
        if (existingUser != null)
        {
            return FailureAccess("El nombre de usuario ya esta en uso.");
        }

        existingUser = await _userHelper.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return FailureAccess("El correo ya esta en uso.");
        }

        SoftPlan? plan = await _context.SoftPlans.FirstOrDefaultAsync(x => x.SoftPlanId == request.SoftPlanId && x.Active);
        if (plan == null)
        {
            return FailureAccess("El plan seleccionado no esta disponible.");
        }

        Country? country = await _context.Countries.FirstOrDefaultAsync(x => x.Name == "Colombia")
            ?? await _context.Countries.OrderBy(x => x.CountryId).FirstOrDefaultAsync();
        if (country == null)
        {
            return FailureAccess("No existe un pais configurado para crear la corporacion.");
        }

        DateTime nowUtc = DateTime.UtcNow;
        Corporation corporation = new()
        {
            Name = request.CorporationName.Trim(),
            NroDocument = request.CorporationDocument.Trim(),
            Phone = request.CorporationPhone.Trim(),
            Address = request.CorporationAddress.Trim(),
            CountryId = country.CountryId,
            SoftPlanId = plan.SoftPlanId,
            DateStart = nowUtc,
            DateEnd = nowUtc.AddDays(TrialDays),
            Active = true
        };

        Manager manager = new()
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            NroDocument = request.Document.Trim(),
            PhoneNumber = request.Phone.Trim(),
            Address = request.CorporationAddress.Trim(),
            Email = request.Email.Trim(),
            UserName = request.UserName.Trim(),
            Job = "Administrador",
            UserType = UserType.Administrator,
            Active = true
        };

        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.Corporations.Add(corporation);
            await _transactionManager.SaveChangesAsync();

            manager.CorporationId = corporation.CorporationId;
            _context.Managers.Add(manager);

            CorporationSubscription subscription = new()
            {
                CorporationId = corporation.CorporationId,
                SoftPlanId = plan.SoftPlanId,
                Cycle = SubscriptionCycle.Monthly,
                Status = CorporationSubscriptionStatus.Trial,
                DateCreatedUtc = nowUtc,
                TrialStartsUtc = nowUtc,
                TrialEndsUtc = nowUtc.AddDays(TrialDays),
                CurrentPeriodStartsUtc = nowUtc,
                CurrentPeriodEndsUtc = nowUtc.AddDays(TrialDays),
                ExternalReference = $"spix-trial-{corporation.CorporationId}-{Guid.NewGuid():N}",
                UserModifiedByName = manager.UserName
            };
            _context.CorporationSubscriptions.Add(subscription);
            await _transactionManager.SaveChangesAsync();

            User user = await _userHelper.AddUserUsuarioAsync(manager.FirstName, manager.LastName,
                manager.UserName, manager.Email, manager.PhoneNumber, manager.Address, manager.Job,
                corporation.CorporationId, string.Empty, "SaaS trial", true, UserType.Administrator);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return FailureAccess("No se pudo crear el usuario administrador.");
            }

            await _transactionManager.CommitTransactionAsync();

            Response emailResponse = await SendActivationEmailAsync(user, frontUrl);
            string message = emailResponse.IsSuccess
                ? "Tu prueba de 14 dias fue creada. Revisa tu correo para activar la cuenta."
                : "La prueba fue creada. El correo de activacion debe reenviarse desde SaaS Manager.";

            return new ActionResponse<SubscriptionAccessDTO>
            {
                WasSuccess = true,
                Result = new SubscriptionAccessDTO
                {
                    HasAccess = true,
                    IsTrial = true,
                    CorporationId = corporation.CorporationId,
                    SoftPlanId = plan.SoftPlanId,
                    SoftPlanName = plan.Name,
                    Status = CorporationSubscriptionStatus.Trial,
                    ValidUntilUtc = subscription.TrialEndsUtc,
                    DaysRemaining = TrialDays,
                    Message = message
                }
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<SubscriptionAccessDTO>(ex);
        }
    }

    public async Task<ActionResponse<SubscriptionAccessDTO>> GetAccessAsync(int corporationId)
    {
        try
        {
            Corporation? corporation = await _context.Corporations
                .Include(x => x.SoftPlan)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CorporationId == corporationId);
            if (corporation == null)
            {
                return FailureAccess("La corporacion no fue encontrada.");
            }

            CorporationSubscription? subscription = await _context.CorporationSubscriptions
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId)
                .OrderByDescending(x => x.DateCreatedUtc)
                .FirstOrDefaultAsync();
            if (subscription == null)
            {
                bool legacyAccess = corporation.Active && corporation.DateEnd.Date >= DateTime.UtcNow.Date;
                return new ActionResponse<SubscriptionAccessDTO>
                {
                    WasSuccess = true,
                    Result = new SubscriptionAccessDTO
                    {
                        HasAccess = legacyAccess,
                        CorporationId = corporation.CorporationId,
                        SoftPlanId = corporation.SoftPlanId,
                        SoftPlanName = corporation.SoftPlan?.Name,
                        ValidUntilUtc = corporation.DateEnd,
                        DaysRemaining = Math.Max(0, (int)Math.Ceiling((corporation.DateEnd.Date - DateTime.UtcNow.Date).TotalDays)),
                        Message = legacyAccess ? null : "La suscripcion de la corporacion requiere renovacion."
                    }
                };
            }

            DateTime? validUntilUtc = subscription.Status == CorporationSubscriptionStatus.Trial
                ? subscription.TrialEndsUtc
                : subscription.CurrentPeriodEndsUtc;
            bool activeStatus = subscription.Status == CorporationSubscriptionStatus.Trial ||
                subscription.Status == CorporationSubscriptionStatus.Active;
            bool hasAccess = corporation.Active && activeStatus && validUntilUtc >= DateTime.UtcNow;

            return new ActionResponse<SubscriptionAccessDTO>
            {
                WasSuccess = true,
                Result = new SubscriptionAccessDTO
                {
                    HasAccess = hasAccess,
                    IsTrial = subscription.Status == CorporationSubscriptionStatus.Trial,
                    CorporationId = corporation.CorporationId,
                    SoftPlanId = subscription.SoftPlanId,
                    SoftPlanName = corporation.SoftPlan?.Name,
                    Status = subscription.Status,
                    ValidUntilUtc = validUntilUtc,
                    DaysRemaining = validUntilUtc.HasValue
                        ? Math.Max(0, (int)Math.Ceiling((validUntilUtc.Value - DateTime.UtcNow).TotalDays))
                        : 0,
                    Message = hasAccess ? null : "La prueba o suscripcion vencio. Selecciona un plan para continuar."
                }
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<SubscriptionAccessDTO>(ex);
        }
    }

    public async Task<ActionResponse<SubscriptionCheckoutDTO>> CreateCheckoutAsync(int corporationId,
        string username, SubscriptionCheckoutRequestDTO request)
    {
        string activeGateway = _secretStore.Get("PaymentGateway:Active", "MercadoPago")
            ?? "MercadoPago";

        if (!string.Equals(activeGateway, "MercadoPago", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(activeGateway, "Wompi", StringComparison.OrdinalIgnoreCase))
        {
            return FailureCheckout("No hay una pasarela de pago activa válida.");
        }

        if (string.Equals(activeGateway, "Wompi", StringComparison.OrdinalIgnoreCase))
        {
            return await CreateWompiCheckoutAsync(corporationId, username, request);
        }

        SoftPlan? plan = await _context.SoftPlans.FirstOrDefaultAsync(x => x.SoftPlanId == request.SoftPlanId && x.Active);
        if (plan == null)
        {
            return FailureCheckout("El plan seleccionado no esta disponible.");
        }

        Corporation? corporation = await _context.Corporations.FirstOrDefaultAsync(x => x.CorporationId == corporationId);
        if (corporation == null)
        {
            return FailureCheckout("La corporacion no fue encontrada.");
        }

        User? user = await _userHelper.GetUserByUserNameAsync(username);
        if (user == null)
        {
            return FailureCheckout("No fue posible identificar el usuario de la suscripcion.");
        }

        MercadoPagoSettings mercadoPago = _secretStore.Bind<MercadoPagoSettings>("MercadoPago");
        MercadoPagoPlatformSetting? setting = await _context.MercadoPagoPlatformSettings
            .OrderByDescending(x => x.DateModifiedUtc)
            .FirstOrDefaultAsync(x => x.Active);
        if (setting == null && string.IsNullOrWhiteSpace(mercadoPago.AccessToken))
        {
            return FailureCheckout("Mercado Pago no esta configurado o activo en SaaS.");
        }

        string accessToken = mercadoPago.AccessToken
            ?? _secretProtector.Unprotect(setting!.AccessTokenEncrypted);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return FailureCheckout("La llave privada de Mercado Pago no esta disponible.");
        }

        decimal amount = request.Cycle == SubscriptionCycle.Annual
            ? plan.AnnualPrice ?? (plan.Price * 10)
            : plan.Price;
        DateTime nowUtc = DateTime.UtcNow;
        CorporationSubscription subscription = new()
        {
            CorporationId = corporation.CorporationId,
            SoftPlanId = plan.SoftPlanId,
            Cycle = request.Cycle,
            Status = CorporationSubscriptionStatus.PendingPayment,
            DateCreatedUtc = nowUtc,
            Gateway = "MercadoPago",
            Amount = amount,
            Currency = "COP",
            ExternalReference = $"spix-sub-{corporation.CorporationId}-{Guid.NewGuid():N}",
            UserModifiedByName = username
        };

        try
        {
            using HttpClient client = new()
            {
                BaseAddress = new Uri("https://api.mercadopago.com")
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            if (accessToken.StartsWith("TEST-", StringComparison.OrdinalIgnoreCase))
            {
                client.DefaultRequestHeaders.Add("X-scope", "stage");
            }

            object payload = new
            {
                reason = $"Spix - {plan.Name}",
                external_reference = subscription.ExternalReference,
                payer_email = user.Email,
                back_url = mercadoPago.BackUrl ?? _configuration["UrlFrontend"],
                notification_url = mercadoPago.NotificationUrl ?? setting?.WebhookUrl,
                auto_recurring = new
                {
                    frequency = request.Cycle == SubscriptionCycle.Annual ? 12 : 1,
                    frequency_type = "months",
                    transaction_amount = amount,
                    currency_id = mercadoPago.CurrencyId ?? "COP"
                }
            };
            string json = JsonSerializer.Serialize(payload);
            using StringContent content = new(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync("/preapproval", content);
            string responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return FailureCheckout("Mercado Pago no pudo iniciar la suscripcion. " + responseBody);
            }

            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;
            string? checkoutUrl = TryGetString(root, "init_point") ?? TryGetString(root, "sandbox_init_point");
            if (string.IsNullOrWhiteSpace(checkoutUrl))
            {
                return FailureCheckout("Mercado Pago no devolvio la URL de pago.");
            }

            subscription.MercadoPagoPreapprovalId = TryGetString(root, "id");
            subscription.CheckoutUrl = checkoutUrl;
            _context.CorporationSubscriptions.Add(subscription);
            corporation.SoftPlanId = plan.SoftPlanId;
            await _context.SaveChangesAsync();

            return new ActionResponse<SubscriptionCheckoutDTO>
            {
                WasSuccess = true,
                Result = new SubscriptionCheckoutDTO
                {
                    CheckoutUrl = checkoutUrl,
                    CorporationSubscriptionId = subscription.CorporationSubscriptionId
                }
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<SubscriptionCheckoutDTO>(ex);
        }
    }

    public async Task<ActionResponse<MercadoPagoPlatformSettingDTO>> GetMercadoPagoSettingAsync()
    {
        try
        {
            MercadoPagoPlatformSetting? setting = await _context.MercadoPagoPlatformSettings
                .AsNoTracking()
                .OrderByDescending(x => x.DateModifiedUtc)
                .FirstOrDefaultAsync();
            if (setting == null)
            {
                return new ActionResponse<MercadoPagoPlatformSettingDTO>
                {
                    WasSuccess = true,
                    Result = new MercadoPagoPlatformSettingDTO { Active = false }
                };
            }

            return new ActionResponse<MercadoPagoPlatformSettingDTO>
            {
                WasSuccess = true,
                Result = ToMercadoPagoSettingDto(setting)
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<MercadoPagoPlatformSettingDTO>(ex);
        }
    }

    public async Task<ActionResponse<MercadoPagoPlatformSettingDTO>> SaveMercadoPagoSettingAsync(
        MercadoPagoPlatformSettingDTO setting, string username)
    {
        try
        {
            MercadoPagoPlatformSetting? current = await _context.MercadoPagoPlatformSettings
                .OrderByDescending(x => x.DateModifiedUtc)
                .FirstOrDefaultAsync();
            if (current == null)
            {
                if (string.IsNullOrWhiteSpace(setting.PublicKey) || string.IsNullOrWhiteSpace(setting.AccessToken))
                {
                    return FailureSetting("La llave publica y el access token son obligatorios.");
                }

                current = new MercadoPagoPlatformSetting();
                _context.MercadoPagoPlatformSettings.Add(current);
            }

            current.Name = string.IsNullOrWhiteSpace(setting.Name) ? "Mercado Pago Colombia" : setting.Name.Trim();
            current.WebhookUrl = setting.WebhookUrl?.Trim();
            current.Active = setting.Active;
            current.DateModifiedUtc = DateTime.UtcNow;
            current.UserModifiedByName = username;

            if (!string.IsNullOrWhiteSpace(setting.PublicKey))
            {
                current.PublicKeyEncrypted = _secretProtector.Protect(setting.PublicKey.Trim());
            }
            if (!string.IsNullOrWhiteSpace(setting.AccessToken))
            {
                current.AccessTokenEncrypted = _secretProtector.Protect(setting.AccessToken.Trim());
            }
            if (!string.IsNullOrWhiteSpace(setting.WebhookSecret))
            {
                current.WebhookSecretEncrypted = _secretProtector.Protect(setting.WebhookSecret.Trim());
            }

            await _context.SaveChangesAsync();

            return new ActionResponse<MercadoPagoPlatformSettingDTO>
            {
                WasSuccess = true,
                Result = ToMercadoPagoSettingDto(current)
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<MercadoPagoPlatformSettingDTO>(ex);
        }
    }

    public async Task<ActionResponse<bool>> SyncMercadoPagoSubscriptionAsync(string? notificationType,
        string? preapprovalId, string? signature, string? requestId)
    {
        try
        {
            MercadoPagoSettings mercadoPago = _secretStore.Bind<MercadoPagoSettings>("MercadoPago");
            MercadoPagoPlatformSetting? setting = await _context.MercadoPagoPlatformSettings
                .AsNoTracking()
                .OrderByDescending(x => x.DateModifiedUtc)
                .FirstOrDefaultAsync(x => x.Active);
            if (setting == null && string.IsNullOrWhiteSpace(mercadoPago.WebhookSecret))
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Mercado Pago no esta configurado o activo en SaaS."
                };
            }

            string? webhookSecret = mercadoPago.WebhookSecret;

            if (string.IsNullOrWhiteSpace(webhookSecret) &&
                setting != null &&
                !string.IsNullOrWhiteSpace(setting.WebhookSecretEncrypted))
            {
                webhookSecret = _secretProtector.Unprotect(setting.WebhookSecretEncrypted);
            }
            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "El secreto de webhook de Mercado Pago no esta configurado."
                };
            }

            if (!MercadoPagoSignature.Verify(signature, requestId, preapprovalId, webhookSecret))
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "La firma del webhook de Mercado Pago no es valida."
                };
            }

            if (string.IsNullOrWhiteSpace(preapprovalId))
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = true,
                    Result = true
                };
            }

            string normalizedNotificationType = notificationType?.Trim().ToLowerInvariant() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(normalizedNotificationType) &&
                normalizedNotificationType != "subscription_preapproval" &&
                normalizedNotificationType != "preapproval")
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = true,
                    Result = true
                };
            }

            CorporationSubscription? subscription = await _context.CorporationSubscriptions
                .Include(x => x.Corporation)
                .FirstOrDefaultAsync(x => x.MercadoPagoPreapprovalId == preapprovalId);
            if (subscription == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = true,
                    Result = true
                };
            }

            string? accessToken = mercadoPago.AccessToken;

            if (string.IsNullOrWhiteSpace(accessToken) &&
                setting != null &&
                !string.IsNullOrWhiteSpace(setting.AccessTokenEncrypted))
            {
                accessToken = _secretProtector.Unprotect(setting.AccessTokenEncrypted);
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "El access token de Mercado Pago no está configurado."
                };
            }

            using HttpClient client = new()
            {
                BaseAddress = new Uri("https://api.mercadopago.com")
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            if (accessToken.StartsWith("TEST-", StringComparison.OrdinalIgnoreCase))
            {
                client.DefaultRequestHeaders.Add("X-scope", "stage");
            }
            HttpResponseMessage response = await client.GetAsync($"/preapproval/{preapprovalId}");
            if (!response.IsSuccessStatusCode)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "No fue posible verificar la suscripcion directamente con Mercado Pago."
                };
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;
            string? remoteReference = TryGetString(root, "external_reference");
            if (!string.Equals(subscription.ExternalReference, remoteReference, StringComparison.Ordinal))
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "La suscripcion recibida no corresponde a Spix."
                };
            }

            string? remoteStatus = TryGetString(root, "status");
            DateTime nowUtc = DateTime.UtcNow;
            if (string.Equals(remoteStatus, "authorized", StringComparison.OrdinalIgnoreCase))
            {
                subscription.Status = CorporationSubscriptionStatus.Active;
                subscription.CurrentPeriodStartsUtc = nowUtc;
                subscription.CurrentPeriodEndsUtc = subscription.Cycle == SubscriptionCycle.Annual
                    ? nowUtc.AddYears(1)
                    : nowUtc.AddMonths(1);
                if (subscription.Corporation != null)
                {
                    subscription.Corporation.Active = true;
                    subscription.Corporation.SoftPlanId = subscription.SoftPlanId;
                    subscription.Corporation.DateEnd = subscription.CurrentPeriodEndsUtc.Value;
                }
            }
            else if (string.Equals(remoteStatus, "cancelled", StringComparison.OrdinalIgnoreCase))
            {
                subscription.Status = CorporationSubscriptionStatus.Cancelled;
            }

            await _context.SaveChangesAsync();
            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    public async Task<ActionResponse<bool>> SyncWompiSubscriptionAsync(WompiEventDTO eventDto)
    {
        try
        {
            WompiSettings wompi = _secretStore.Bind<WompiSettings>("Wompi");

            if (string.IsNullOrWhiteSpace(wompi.EventsSecret) ||
                !WompiSignature.VerifyEventSignature(eventDto, wompi.EventsSecret))
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "La firma del evento Wompi no es válida."
                };
            }

            WompiTransaction? transaction = eventDto.Data?.Transaction;

            if (transaction == null || string.IsNullOrWhiteSpace(transaction.Reference))
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = true,
                    Result = true
                };
            }

            CorporationSubscription? subscription = await _context.CorporationSubscriptions
                .Include(x => x.Corporation)
                .FirstOrDefaultAsync(x =>
                    x.Gateway == "Wompi" &&
                    x.ExternalReference == transaction.Reference);

            if (subscription == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = true,
                    Result = true
                };
            }

            if (subscription.Status == CorporationSubscriptionStatus.Active)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = true,
                    Result = true
                };
            }

            subscription.WompiTransactionId = transaction.Id;
            string status = transaction.Status?.Trim().ToUpperInvariant() ?? string.Empty;

            if (status != "APPROVED")
            {
                subscription.Status = status == "DECLINED" || status == "VOIDED"
                    ? CorporationSubscriptionStatus.Cancelled
                    : CorporationSubscriptionStatus.PendingPayment;

                await _context.SaveChangesAsync();

                return new ActionResponse<bool>
                {
                    WasSuccess = true,
                    Result = true
                };
            }

            long expectedAmount = (long)Math.Round(
                subscription.Amount * 100m,
                MidpointRounding.AwayFromZero);

            if (transaction.AmountInCents != expectedAmount)
            {
                subscription.Status = CorporationSubscriptionStatus.Cancelled;
                await _context.SaveChangesAsync();

                return new ActionResponse<bool>
                {
                    WasSuccess = true,
                    Result = false
                };
            }

            DateTime nowUtc = DateTime.UtcNow;
            DateTime baseDate = subscription.Corporation!.DateEnd > nowUtc
                ? subscription.Corporation.DateEnd
                : nowUtc;
            DateTime periodEnd = subscription.Cycle == SubscriptionCycle.Annual
                ? baseDate.AddYears(1)
                : baseDate.AddMonths(1);

            subscription.Status = CorporationSubscriptionStatus.Active;
            subscription.CurrentPeriodStartsUtc = baseDate;
            subscription.CurrentPeriodEndsUtc = periodEnd;
            subscription.Corporation.Active = true;
            subscription.Corporation.SoftPlanId = subscription.SoftPlanId;
            subscription.Corporation.DateEnd = periodEnd;

            await _context.SaveChangesAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception exception)
        {
            return await _httpErrorHandler.HandleErrorAsync<bool>(exception);
        }
    }

    private async Task<ActionResponse<SubscriptionCheckoutDTO>> CreateWompiCheckoutAsync(
        int corporationId,
        string username,
        SubscriptionCheckoutRequestDTO request)
    {
        WompiSettings wompi = _secretStore.Bind<WompiSettings>("Wompi");

        if (string.IsNullOrWhiteSpace(wompi.PublicKey) ||
            string.IsNullOrWhiteSpace(wompi.IntegritySecret) ||
            string.IsNullOrWhiteSpace(wompi.CheckoutUrl) ||
            string.IsNullOrWhiteSpace(wompi.RedirectUrl))
        {
            return FailureCheckout("Wompi no está configurado en SaaS.");
        }

        SoftPlan? plan = await _context.SoftPlans
            .FirstOrDefaultAsync(x => x.SoftPlanId == request.SoftPlanId && x.Active);

        Corporation? corporation = await _context.Corporations
            .FirstOrDefaultAsync(x => x.CorporationId == corporationId);

        if (plan == null || corporation == null)
        {
            return FailureCheckout("El plan o la corporación no fueron encontrados.");
        }

        decimal amount = request.Cycle == SubscriptionCycle.Annual
            ? plan.AnnualPrice ?? (plan.Price * 10)
            : plan.Price;
        string reference = $"spix-wompi-{corporationId}-{Guid.NewGuid():N}";
        long amountInCents = (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
        string currency = "COP";
        string integrity = WompiSignature.BuildIntegritySignature(
            reference,
            amountInCents,
            currency,
            wompi.IntegritySecret);
        string checkoutUrl = BuildWompiCheckoutUrl(
            wompi,
            reference,
            amountInCents,
            currency,
            integrity);

        var subscription = new CorporationSubscription
        {
            CorporationId = corporationId,
            SoftPlanId = plan.SoftPlanId,
            Cycle = request.Cycle,
            Status = CorporationSubscriptionStatus.PendingPayment,
            DateCreatedUtc = DateTime.UtcNow,
            ExternalReference = reference,
            Gateway = "Wompi",
            Amount = amount,
            Currency = currency,
            CheckoutUrl = checkoutUrl,
            UserModifiedByName = username
        };

        _context.CorporationSubscriptions.Add(subscription);
        corporation.SoftPlanId = plan.SoftPlanId;
        await _context.SaveChangesAsync();

        return new ActionResponse<SubscriptionCheckoutDTO>
        {
            WasSuccess = true,
            Result = new SubscriptionCheckoutDTO
            {
                CheckoutUrl = checkoutUrl,
                CorporationSubscriptionId = subscription.CorporationSubscriptionId
            }
        };
    }

    private static string BuildWompiCheckoutUrl(
        WompiSettings wompi,
        string reference,
        long amountInCents,
        string currency,
        string integrity)
    {
        string baseUrl = wompi.CheckoutUrl!.TrimEnd('?', '&');
        string separator = baseUrl.Contains('?') ? "&" : "?";

        return baseUrl
            + separator
            + $"public-key={Uri.EscapeDataString(wompi.PublicKey!)}"
            + $"&currency={Uri.EscapeDataString(currency)}"
            + $"&amount-in-cents={amountInCents}"
            + $"&reference={Uri.EscapeDataString(reference)}"
            + $"&signature:integrity={integrity}"
            + $"&redirect-url={Uri.EscapeDataString(wompi.RedirectUrl!)}";
    }

    private async Task<Response> SendActivationEmailAsync(User user, string frontUrl)
    {
        string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
        string tokenLink = frontUrl.CombineFrontendUrl($"api/accounts/ConfirmEmail?userid={user.Id}&token={token}");
        string subject = _localizer["AccountActivation_Subject"];
        string body = ImplementEmails.LocalizedEmailTemplateFactory.BuildAccountActivation(
            _localizer, user.FirstName, user.LastName, user.Pass, tokenLink);

        return await _emailHelper.ConfirmarCuenta(user.Email!, $"{user.FirstName} {user.LastName}", subject, body);
    }

    private static string? TryGetString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out JsonElement property)
            ? property.GetString()
            : null;
    }

    private static ActionResponse<SubscriptionAccessDTO> FailureAccess(string message)
    {
        return new ActionResponse<SubscriptionAccessDTO>
        {
            WasSuccess = false,
            Message = message
        };
    }

    private static ActionResponse<SubscriptionCheckoutDTO> FailureCheckout(string message)
    {
        return new ActionResponse<SubscriptionCheckoutDTO>
        {
            WasSuccess = false,
            Message = message
        };
    }

    private static MercadoPagoPlatformSettingDTO ToMercadoPagoSettingDto(MercadoPagoPlatformSetting setting)
    {
        return new MercadoPagoPlatformSettingDTO
        {
            Name = setting.Name,
            WebhookUrl = setting.WebhookUrl,
            Active = setting.Active,
            HasPublicKey = !string.IsNullOrWhiteSpace(setting.PublicKeyEncrypted),
            HasAccessToken = !string.IsNullOrWhiteSpace(setting.AccessTokenEncrypted),
            HasWebhookSecret = !string.IsNullOrWhiteSpace(setting.WebhookSecretEncrypted)
        };
    }

    private static ActionResponse<MercadoPagoPlatformSettingDTO> FailureSetting(string message)
    {
        return new ActionResponse<MercadoPagoPlatformSettingDTO>
        {
            WasSuccess = false,
            Message = message
        };
    }
}
