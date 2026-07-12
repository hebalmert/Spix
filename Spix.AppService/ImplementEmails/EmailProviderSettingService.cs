using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.SecretProtection;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfacesEmails;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesEmails;
using Spix.DomainLogic.EntitiesEmailDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;
using Spix.xNotification.Interfaces;

namespace Spix.AppService.ImplementEmails;

public class EmailProviderSettingService : IEmailProviderSettingService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;
    private readonly ISecretProtector _secretProtector;
    private readonly IEmailDeliveryService _emailDeliveryService;

    public EmailProviderSettingService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, HttpErrorHandler httpErrorHandler,
        IStringLocalizer localizer, ISecretProtector secretProtector, IEmailDeliveryService emailDeliveryService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
        _secretProtector = secretProtector;
        _emailDeliveryService = emailDeliveryService;
    }

    public async Task<ActionResponse<IEnumerable<EmailProviderSetting>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<IEnumerable<EmailProviderSetting>>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var queryable = _context.EmailProviderSettings
                .Where(x => x.CorporationId == user.CorporationId)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                string filter = pagination.Filter.ToLower();
                queryable = queryable.Where(x => x.Name.ToLower().Contains(filter) ||
                                                 x.FromEmail.ToLower().Contains(filter));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var modelo = await queryable
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.ProviderType)
                .ThenBy(x => x.Name)
                .Paginate(pagination)
                .ToListAsync();

            foreach (var item in modelo)
            {
                ClearSecrets(item);
            }

            return new ActionResponse<IEnumerable<EmailProviderSetting>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<EmailProviderSetting>>(ex);
        }
    }

    public async Task<ActionResponse<EmailProviderSetting>> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<EmailProviderSetting>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var modelo = await _context.EmailProviderSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmailProviderSettingId == id);
            if (modelo == null)
            {
                return new ActionResponse<EmailProviderSetting>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            modelo.SendGridApiKey = _secretProtector.Unprotect(modelo.SendGridApiKeyEncrypted);
            modelo.SmtpPassword = _secretProtector.Unprotect(modelo.SmtpPasswordEncrypted);
            ClearEncryptedSecrets(modelo);

            return new ActionResponse<EmailProviderSetting>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<EmailProviderSetting>(ex);
        }
    }

    public async Task<ActionResponse<EmailProviderSetting>> AddAsync(EmailProviderSetting modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<EmailProviderSetting>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<EmailProviderSetting>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            modelo.DateCreated = DateTime.UtcNow;
            modelo.UsuarioOwner = $"{user.FirstName} {user.LastName}";
            modelo.UserId = Guid.TryParse(user.Id, out var userId) ? userId : null;
            NormalizeProvider(modelo);

            if (modelo.IsDefault)
            {
                await ClearDefaultAsync(modelo.CorporationId);
            }

            _context.EmailProviderSettings.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            ClearSecrets(modelo);

            return new ActionResponse<EmailProviderSetting>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<EmailProviderSetting>(ex);
        }
    }

    public async Task<ActionResponse<EmailProviderSetting>> UpdateAsync(EmailProviderSetting modelo, string username)
    {
        if (modelo == null || modelo.EmailProviderSettingId == Guid.Empty)
        {
            return new ActionResponse<EmailProviderSetting>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<EmailProviderSetting>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var current = await _context.EmailProviderSettings
                .FirstOrDefaultAsync(x => x.EmailProviderSettingId == modelo.EmailProviderSettingId &&
                                          x.CorporationId == user.CorporationId);

            if (current == null)
            {
                return new ActionResponse<EmailProviderSetting>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            current.Name = modelo.Name;
            current.ProviderType = modelo.ProviderType;
            current.FromEmail = modelo.FromEmail;
            current.FromName = modelo.FromName;
            current.SmtpHost = modelo.SmtpHost;
            current.SmtpPort = modelo.SmtpPort;
            current.SmtpUseSsl = modelo.SmtpUseSsl;
            current.SmtpUser = modelo.SmtpUser;
            current.Active = modelo.Active;
            current.IsDefault = modelo.IsDefault;

            if (!string.IsNullOrWhiteSpace(modelo.SendGridApiKey))
            {
                current.SendGridApiKeyEncrypted = _secretProtector.Protect(modelo.SendGridApiKey);
            }

            if (!string.IsNullOrWhiteSpace(modelo.SmtpPassword))
            {
                current.SmtpPasswordEncrypted = _secretProtector.Protect(modelo.SmtpPassword);
            }

            NormalizeProvider(current);

            if (current.IsDefault)
            {
                await ClearDefaultAsync(current.CorporationId, current.EmailProviderSettingId);
            }

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            ClearSecrets(current);

            return new ActionResponse<EmailProviderSetting>
            {
                WasSuccess = true,
                Result = current
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<EmailProviderSetting>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var dataRemove = await _context.EmailProviderSettings
                .FirstOrDefaultAsync(x => x.EmailProviderSettingId == id && x.CorporationId == user.CorporationId);

            if (dataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.EmailProviderSettings.Remove(dataRemove);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<bool>
            {
                WasSuccess = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    public async Task<ActionResponse<bool>> TestAsync(Guid id, string username)
    {
        try
        {
            User user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var provider = await _context.EmailProviderSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.EmailProviderSettingId == id &&
                                          x.CorporationId == user.CorporationId);

            if (provider == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            if (!provider.Active)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "El proveedor de correo no esta activo."
                };
            }

            string to = user.Email!;
            string nameTo = $"{user.FirstName} {user.LastName}";
            string subject = _localizer["EmailProviderTest_Subject"];
            string providerName = System.Net.WebUtility.HtmlEncode(provider.Name);
            string providerType = System.Net.WebUtility.HtmlEncode(provider.ProviderType.ToString());
            string body = $"""
                <!doctype html>
                <html lang="en">
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1">
                    <title>{subject}</title>
                </head>
                <body style="margin:0;padding:0;background-color:#f3f6fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background-color:#f3f6fb;">
                        <tr><td align="center" style="padding:40px 16px;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:600px;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.10);">
                                <tr><td style="padding:30px 36px;background:linear-gradient(135deg,#6d3fd5 0%,#1478ff 100%);color:#ffffff;">
                                    <div style="font-size:13px;letter-spacing:1.4px;text-transform:uppercase;opacity:.85;">EMAIL SERVICE</div>
                                    <h1 style="margin:8px 0 0;font-size:26px;line-height:1.3;font-weight:700;">{_localizer["EmailProviderTest_Title"]}</h1>
                                </td></tr>
                                <tr><td style="padding:34px 36px 18px;">
                                    <div style="text-align:center;margin-bottom:26px;">
                                        <div style="display:inline-block;width:58px;height:58px;line-height:58px;border-radius:50%;background:#e8f8ef;color:#16834a;font-size:30px;font-weight:bold;">&#10003;</div>
                                        <h2 style="margin:16px 0 8px;font-size:21px;color:#162033;">{_localizer["EmailProviderTest_Result"]}</h2>
                                        <p style="margin:0;color:#64748b;font-size:15px;line-height:1.7;">{_localizer["EmailProviderTest_SuccessMessage"]}</p>
                                    </div>
                                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background:#f8fafc;border:1px solid #e5eaf1;border-radius:12px;">
                                        <tr><td style="padding:18px 20px;color:#64748b;font-size:13px;text-transform:uppercase;letter-spacing:.6px;">{_localizer["EmailProviderTest_Provider"]}</td></tr>
                                        <tr><td style="padding:0 20px 20px;font-size:17px;font-weight:700;color:#1e293b;">{providerName}<span style="display:inline-block;margin-left:8px;padding:4px 9px;border-radius:20px;background:#e8efff;color:#315fc5;font-size:12px;font-weight:600;">{providerType}</span></td></tr>
                                    </table>
                                </td></tr>
                                <tr><td style="padding:22px 36px 30px;text-align:center;color:#94a3b8;font-size:12px;line-height:1.6;border-top:1px solid #eef2f7;">{_localizer["EmailProviderTest_Footer"]}</td></tr>
                            </table>
                        </td></tr>
                    </table>
                </body>
                </html>
                """;

            var model = new EmailDeliveryDTO
            {
                ProviderType = provider.ProviderType,
                SendGridApiKey = _secretProtector.Unprotect(provider.SendGridApiKeyEncrypted),
                SmtpHost = provider.SmtpHost,
                SmtpPort = provider.SmtpPort ?? 0,
                SmtpUseSsl = provider.SmtpUseSsl,
                SmtpUser = provider.SmtpUser,
                SmtpPassword = _secretProtector.Unprotect(provider.SmtpPasswordEncrypted),
                FromEmail = provider.FromEmail,
                FromName = provider.FromName,
                To = to,
                NameTo = nameTo,
                Subject = subject,
                Body = body
            };

            Response response = await _emailDeliveryService.SendAsync(model);

            if (!response.IsSuccess)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = response.Message ?? "No se pudo enviar el correo de prueba."
                };
            }

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

    private void NormalizeProvider(EmailProviderSetting modelo)
    {
        if (modelo.ProviderType == EmailProviderType.SendGrid)
        {
            modelo.SendGridApiKeyEncrypted = string.IsNullOrWhiteSpace(modelo.SendGridApiKey)
                ? modelo.SendGridApiKeyEncrypted
                : _secretProtector.Protect(modelo.SendGridApiKey);
            modelo.SmtpPasswordEncrypted = null;
            modelo.SmtpHost = null;
            modelo.SmtpPort = null;
            modelo.SmtpUseSsl = false;
            modelo.SmtpUser = null;
        }
        else
        {
            modelo.SmtpPasswordEncrypted = string.IsNullOrWhiteSpace(modelo.SmtpPassword)
                ? modelo.SmtpPasswordEncrypted
                : _secretProtector.Protect(modelo.SmtpPassword);
            modelo.SendGridApiKeyEncrypted = null;
        }

        modelo.SendGridApiKey = null;
        modelo.SmtpPassword = null;
    }

    private async Task ClearDefaultAsync(int corporationId, Guid? exceptId = null)
    {
        var defaults = await _context.EmailProviderSettings
            .Where(x => x.CorporationId == corporationId &&
                        x.IsDefault &&
                        (!exceptId.HasValue || x.EmailProviderSettingId != exceptId.Value))
            .ToListAsync();

        foreach (var item in defaults)
        {
            item.IsDefault = false;
        }
    }

    private static void ClearSecrets(EmailProviderSetting modelo)
    {
        modelo.SendGridApiKey = null;
        modelo.SmtpPassword = null;
        ClearEncryptedSecrets(modelo);
    }

    private static void ClearEncryptedSecrets(EmailProviderSetting modelo)
    {
        modelo.SendGridApiKeyEncrypted = null;
        modelo.SmtpPasswordEncrypted = null;
    }
}
