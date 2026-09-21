using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.SecretProtection;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.ImplementContratos;
using Spix.AppService.ImplementEmails;
using Spix.AppService.InterfacesSignature;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesEmails;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EntitiesEmailDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;
using Spix.xFiles.SignatureHelper;
using Spix.xNotification.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Spix.AppService.ImplementSignature;

public class SignatureService : ISignatureService
{
    private const char SlashChar = '/';

    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IFileStorage _fileStorage;
    private readonly IPdfSignatureService _pdfSignatureService;
    private readonly ImgSetting _imgOption;
    private readonly IEmailDeliveryService _emailDeliveryService;
    private readonly ISecretProtector _secretProtector;
    private readonly IStringLocalizer _localizer;

    public SignatureService(DataContext context, IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper, ITransactionManager transactionManager, HttpErrorHandler httpErrorHandler,
        IFileStorage fileStorage, IPdfSignatureService pdfSignatureService, IOptions<ImgSetting> imgOption,
        IEmailDeliveryService emailDeliveryService, ISecretProtector secretProtector, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _transactionManager = transactionManager;
        _httpErrorHandler = httpErrorHandler;
        _fileStorage = fileStorage;
        _pdfSignatureService = pdfSignatureService;
        _imgOption = imgOption.Value;
        _emailDeliveryService = emailDeliveryService;
        _secretProtector = secretProtector;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<ContractDocumentTemplate>>> GetTemplatesAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ContractDocumentTemplate>>();

            var queryable = _context.ContractDocumentTemplates.AsNoTracking()
                .Include(x => x.ContractDocumentTemplateFields)
                .Where(x => x.CorporationId == user.CorporationId!.Value)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x => EF.Functions.Like(x.Name, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var list = await queryable
                .OrderBy(x => x.DocumentType)
                .ThenBy(x => x.Name)
                .Paginate(pagination)
                .ToListAsync();

            foreach (var item in list)
                await SetTemplateUrlAsync(item);

            return new ActionResponse<IEnumerable<ContractDocumentTemplate>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractDocumentTemplate>>(ex);
        }
    }

    public async Task<ActionResponse<ContractDocumentTemplate>> GetTemplateAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ContractDocumentTemplate>();

            var model = await _context.ContractDocumentTemplates
                .Include(x => x.ContractDocumentTemplateFields)
                .FirstOrDefaultAsync(x => x.ContractDocumentTemplateId == id && x.CorporationId == user.CorporationId!.Value);

            if (model == null)
                return NotFound<ContractDocumentTemplate>();

            await SetTemplateUrlAsync(model);
            return new ActionResponse<ContractDocumentTemplate> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractDocumentTemplate>(ex);
        }
    }

    public async Task<ActionResponse<ContractDocumentTemplate>> AddTemplateAsync(ContractDocumentTemplate model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ContractDocumentTemplate>();
            }

            var fileBytes = ReadBase64File(model.FileBase64);
            if (fileBytes.Length == 0)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractDocumentTemplate> { WasSuccess = false, Message = "Debe subir un archivo PDF." };
            }

            //Las paginas se leen del PDF (ya no se escriben a mano)
            var pageCount = _pdfSignatureService.GetPageCount(fileBytes);
            if (pageCount == 0)
            {
                await _transactionManager.RollbackTransactionAsync();
                return InvalidPdf<ContractDocumentTemplate>();
            }

            model.PageCount = pageCount;

            //Solo una plantilla activa por tipo (Contrato / Consent) en la corporacion; se valida antes de subir el PDF
            if (model.Active && await OtherActiveTemplateExistsAsync(user.CorporationId!.Value, model.DocumentType, Guid.Empty))
            {
                await _transactionManager.RollbackTransactionAsync();
                return ActiveTemplateExists<ContractDocumentTemplate>();
            }

            var fileName = $"{Guid.NewGuid()}.pdf";
            var containerName = GetContainerName(model.DocumentType);
            model.FileName = await _fileStorage.SaveFileAsync(fileBytes, fileName, containerName, model.OriginalFileName ?? fileName, "application/pdf");
            model.DateCreated = DateTime.UtcNow.Date;
            model.CorporationId = user.CorporationId!.Value;
            model.UsuarioOwner = $"{user.FirstName} {user.LastName}";
            model.UserId = Guid.Parse(user.Id);

            _context.ContractDocumentTemplates.Add(model);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractDocumentTemplate> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractDocumentTemplate>(ex);
        }
    }

    public async Task<ActionResponse<ContractDocumentTemplate>> UpdateTemplateAsync(ContractDocumentTemplate model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ContractDocumentTemplate>();
            }

            var current = await _context.ContractDocumentTemplates
                .FirstOrDefaultAsync(x => x.ContractDocumentTemplateId == model.ContractDocumentTemplateId && x.CorporationId == user.CorporationId!.Value);

            if (current == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return NotFound<ContractDocumentTemplate>();
            }

            //Solo una plantilla activa por tipo (Contrato / Consent) en la corporacion
            if (model.Active && await OtherActiveTemplateExistsAsync(current.CorporationId, model.DocumentType, current.ContractDocumentTemplateId))
            {
                await _transactionManager.RollbackTransactionAsync();
                return ActiveTemplateExists<ContractDocumentTemplate>();
            }

            //Si viene PDF nuevo se valida ANTES de tocar nada
            var fileBytes = ReadBase64File(model.FileBase64);
            var pageCount = fileBytes.Length == 0 ? 0 : _pdfSignatureService.GetPageCount(fileBytes);
            if (fileBytes.Length > 0 && pageCount == 0)
            {
                await _transactionManager.RollbackTransactionAsync();
                return InvalidPdf<ContractDocumentTemplate>();
            }

            current.Name = model.Name;
            current.DocumentType = model.DocumentType;
            current.Active = model.Active;

            if (fileBytes.Length > 0)
            {
                var oldContainerName = GetContainerName(current.DocumentType);
                await _fileStorage.RemoveFileAsync(oldContainerName, current.FileName);

                //Paginas del PDF nuevo; las coordenadas de paginas que ya no existen se eliminan
                current.PageCount = pageCount;
                var outOfRangeFields = await _context.ContractDocumentTemplateFields
                    .Where(x => x.ContractDocumentTemplateId == current.ContractDocumentTemplateId && x.PageNumber > pageCount)
                    .ToListAsync();
                _context.ContractDocumentTemplateFields.RemoveRange(outOfRangeFields);

                var fileName = $"{Guid.NewGuid()}.pdf";
                var containerName = GetContainerName(model.DocumentType);
                current.FileName = await _fileStorage.SaveFileAsync(fileBytes, fileName, containerName, model.OriginalFileName ?? fileName, "application/pdf");
                current.OriginalFileName = model.OriginalFileName;
            }

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractDocumentTemplate> { WasSuccess = true, Result = current };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractDocumentTemplate>(ex);
        }
    }

    public async Task<ActionResponse<ContractDocumentTestDTO>> TestTemplateAsync(Guid templateId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ContractDocumentTestDTO>();

            //El test NO exige plantilla activa: asi se prueban las coordenadas del PDF nuevo antes de activarlo
            var template = await _context.ContractDocumentTemplates
                .AsNoTracking()
                .Include(x => x.ContractDocumentTemplateFields)
                .FirstOrDefaultAsync(x => x.ContractDocumentTemplateId == templateId && x.CorporationId == user.CorporationId!.Value);

            if (template == null || string.IsNullOrWhiteSpace(template.FileName))
                return NotFound<ContractDocumentTestDTO>();

            var templateBytes = await GetFileBytesAsync(template.FileName, GetContainerName(template.DocumentType));
            var fields = BuildPdfFields(template.ContractDocumentTemplateFields).ToList();
            var pdfBytes = _pdfSignatureService.FillPdf(templateBytes, BuildTestPdfFields(fields), BuildTestValues());

            var fileName = $"test-{Guid.NewGuid()}.pdf";
            var containerName = GetContainerName(template.DocumentType);
            var savedFileName = await _fileStorage.SaveFileAsync(pdfBytes, fileName, containerName, fileName, "application/pdf");
            var pdfUrl = await _fileStorage.GetBlobSasUrlAsync(savedFileName, containerName, TimeSpan.FromMinutes(30));

            return new ActionResponse<ContractDocumentTestDTO>
            {
                WasSuccess = true,
                Result = new ContractDocumentTestDTO { FileFullPath = pdfUrl }
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractDocumentTestDTO>(ex);
        }
    }

    public async Task<ActionResponse<ContractDocumentPdfDTO>> GetTemplatePdfAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ContractDocumentPdfDTO>();

            var template = await _context.ContractDocumentTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractDocumentTemplateId == id && x.CorporationId == user.CorporationId!.Value);

            if (template == null || string.IsNullOrWhiteSpace(template.FileName))
                return NotFound<ContractDocumentPdfDTO>();

            //PDF original para el editor visual; se entrega en base64 (sin CORS de Azure y sin guardar nada)
            var templateBytes = await GetFileBytesAsync(template.FileName, GetContainerName(template.DocumentType));

            return new ActionResponse<ContractDocumentPdfDTO>
            {
                WasSuccess = true,
                Result = new ContractDocumentPdfDTO
                {
                    FileBase64 = Convert.ToBase64String(templateBytes),
                    PageCount = _pdfSignatureService.GetPageCount(templateBytes)
                }
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractDocumentPdfDTO>(ex);
        }
    }

    public async Task<ActionResponse<ContractDocumentPdfDTO>> PreviewTemplateAsync(Guid id, List<ContractDocumentTemplateField> fields, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ContractDocumentPdfDTO>();

            var template = await _context.ContractDocumentTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractDocumentTemplateId == id && x.CorporationId == user.CorporationId!.Value);

            if (template == null || string.IsNullOrWhiteSpace(template.FileName))
                return NotFound<ContractDocumentPdfDTO>();

            //Vista previa con los campos que estan en pantalla (aun SIN guardar), llena con datos de prueba
            var templateBytes = await GetFileBytesAsync(template.FileName, GetContainerName(template.DocumentType));
            var pdfFields = BuildPdfFields(fields).ToList();
            var pdfBytes = _pdfSignatureService.FillPdf(templateBytes, BuildTestPdfFields(pdfFields), BuildTestValues());

            return new ActionResponse<ContractDocumentPdfDTO>
            {
                WasSuccess = true,
                Result = new ContractDocumentPdfDTO
                {
                    FileBase64 = Convert.ToBase64String(pdfBytes),
                    PageCount = _pdfSignatureService.GetPageCount(pdfBytes)
                }
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractDocumentPdfDTO>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<ContractDocumentTemplateField>>> SaveTemplateFieldsAsync(Guid id, List<ContractDocumentTemplateField> fields, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<IEnumerable<ContractDocumentTemplateField>>();
            }

            var template = await _context.ContractDocumentTemplates
                .FirstOrDefaultAsync(x => x.ContractDocumentTemplateId == id && x.CorporationId == user.CorporationId!.Value);

            if (template == null || string.IsNullOrWhiteSpace(template.FileName))
            {
                await _transactionManager.RollbackTransactionAsync();
                return NotFound<IEnumerable<ContractDocumentTemplateField>>();
            }

            //Paginas reales del PDF (corrige plantillas viejas donde se escribieron a mano)
            var templateBytes = await GetFileBytesAsync(template.FileName, GetContainerName(template.DocumentType));
            var pageCount = _pdfSignatureService.GetPageCount(templateBytes);
            if (pageCount == 0)
            {
                await _transactionManager.RollbackTransactionAsync();
                return InvalidPdf<IEnumerable<ContractDocumentTemplateField>>();
            }

            //Validacion de cada campo
            foreach (var field in fields)
            {
                var validType = Enum.IsDefined(typeof(ContractDocumentFieldType), field.FieldType);
                var validPage = field.PageNumber >= 1 && field.PageNumber <= pageCount;
                var validPosition = field.PositionX >= 0 && field.PositionY >= 0;

                if (!validType || !validPage || !validPosition)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<IEnumerable<ContractDocumentTemplateField>> { WasSuccess = false, Message = "Hay un campo fuera del PDF. Revise la pagina y la posicion." };
                }
            }

            //Se reemplaza la configuracion completa en una sola transaccion (los campos solo dependen de la plantilla)
            var currentFields = await _context.ContractDocumentTemplateFields
                .Where(x => x.ContractDocumentTemplateId == id)
                .ToListAsync();
            _context.ContractDocumentTemplateFields.RemoveRange(currentFields);

            var newFields = fields.Select(x => new ContractDocumentTemplateField
            {
                ContractDocumentTemplateFieldId = Guid.NewGuid(),
                ContractDocumentTemplateId = id,
                FieldType = x.FieldType,
                PageNumber = x.PageNumber,
                PositionX = Math.Round(x.PositionX, 2),
                PositionY = Math.Round(x.PositionY, 2),
                Width = x.Width.HasValue ? Math.Round(x.Width.Value, 2) : null,
                Height = x.Height.HasValue ? Math.Round(x.Height.Value, 2) : null,
                FontSize = x.FontSize is < 1 or > 80 ? 12 : x.FontSize
            }).ToList();
            _context.ContractDocumentTemplateFields.AddRange(newFields);

            template.PageCount = pageCount;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            //Se corta la navegacion a la plantilla (EF la enlaza al estar rastreada) para no inflar el JSON
            foreach (var field in newFields)
                field.ContractDocumentTemplate = null;

            return new ActionResponse<IEnumerable<ContractDocumentTemplateField>> { WasSuccess = true, Result = newFields };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractDocumentTemplateField>>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteTemplateAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<bool>();
            }

            var model = await _context.ContractDocumentTemplates
                .FirstOrDefaultAsync(x => x.ContractDocumentTemplateId == id && x.CorporationId == user.CorporationId!.Value);

            if (model == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return NotFound<bool>();
            }

            //Lo firmado NUNCA se toca: si algun cliente firmo con esta plantilla, solo se puede desactivar
            var hasSignedDocuments = await _context.ContractSignedDocuments.AnyAsync(x => x.ContractDocumentTemplateId == id && x.Signed);
            if (hasSignedDocuments)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "No se puede eliminar una plantilla que ya tiene documentos firmados. Desactivela para dejar de usarla." };
            }

            //Documentos generados SIN firmar y coordenadas: dependen solo de esta plantilla y se van con ella
            //(el pendiente se vuelve a generar con la plantilla activa cuando se abre la firma del cliente)
            var pendingDocuments = await _context.ContractSignedDocuments
                .Where(x => x.ContractDocumentTemplateId == id)
                .ToListAsync();

            var fields = await _context.ContractDocumentTemplateFields
                .Where(x => x.ContractDocumentTemplateId == id)
                .ToListAsync();

            _context.ContractSignedDocuments.RemoveRange(pendingDocuments);
            _context.ContractDocumentTemplateFields.RemoveRange(fields);
            _context.ContractDocumentTemplates.Remove(model);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            //Los PDF se borran despues del commit: si la base falla, ningun registro queda sin su archivo
            var containerName = GetContainerName(model.DocumentType);
            foreach (var document in pendingDocuments.Where(x => !string.IsNullOrWhiteSpace(x.FileName)))
                await _fileStorage.RemoveFileAsync(GetContainerName(document.DocumentType), document.FileName!);

            if (!string.IsNullOrWhiteSpace(model.FileName))
                await _fileStorage.RemoveFileAsync(containerName, model.FileName);

            return new ActionResponse<bool> { WasSuccess = true, Result = true };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    public async Task<ActionResponse<ContractDocumentTemplateField>> AddTemplateFieldAsync(ContractDocumentTemplateField model, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ContractDocumentTemplateField>();

            var template = await _context.ContractDocumentTemplates
                .FirstOrDefaultAsync(x => x.ContractDocumentTemplateId == model.ContractDocumentTemplateId && x.CorporationId == user.CorporationId!.Value);

            if (template == null)
                return NotFound<ContractDocumentTemplateField>();

            if (model.PageNumber < 1 || model.PageNumber > template.PageCount)
                return new ActionResponse<ContractDocumentTemplateField> { WasSuccess = false, Message = "La pagina esta fuera del rango del PDF." };

            if (model.FontSize <= 0)
                model.FontSize = 12;

            _context.ContractDocumentTemplateFields.Add(model);
            await _context.SaveChangesAsync();

            return new ActionResponse<ContractDocumentTemplateField> { WasSuccess = true, Result = model };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractDocumentTemplateField>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteTemplateFieldAsync(Guid id, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<bool>();

            var model = await _context.ContractDocumentTemplateFields
                .Include(x => x.ContractDocumentTemplate)
                .FirstOrDefaultAsync(x => x.ContractDocumentTemplateFieldId == id &&
                                          x.ContractDocumentTemplate!.CorporationId == user.CorporationId!.Value);

            if (model == null)
                return NotFound<bool>();

            _context.ContractDocumentTemplateFields.Remove(model);
            await _context.SaveChangesAsync();

            return new ActionResponse<bool> { WasSuccess = true, Result = true };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    //Envia al cliente la invitacion para firmar. El enlace solo lleva al portal: para firmar
    //hay que entrar con la cuenta y validar el codigo (ver docs/Firma-Electronica-Part11.md).
    public async Task<ActionResponse<bool>> SendSignatureRequestAsync(Guid contractClientId, string urlFront, ClaimsDTOs context)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(context.UserName);
            if (user == null)
                return AuthFail<bool>();

            var corporationId = user.CorporationId!.Value;

            var contract = await _context.ContractClients
                .AsNoTracking()
                .Include(x => x.Client)
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId && x.CorporationId == corporationId);

            if (contract?.Client == null)
                return NotFound<bool>();

            if (string.IsNullOrWhiteSpace(contract.Client.Email))
                return new ActionResponse<bool> { WasSuccess = false, Message = "El cliente no tiene correo registrado." };

            //Documentos que le faltan por firmar
            var signedTypes = await _context.ContractSignedDocuments
                .AsNoTracking()
                .Where(x => x.ContractClientId == contractClientId && x.Signed)
                .Select(x => x.DocumentType)
                .Distinct()
                .ToListAsync();

            var pending = new List<string>();
            if (!signedTypes.Contains(ContractDocumentType.ConsentData))
                pending.Add("Consentimiento de datos");

            if (!signedTypes.Contains(ContractDocumentType.Contract))
                pending.Add("Contrato de servicio");

            if (pending.Count == 0)
                return new ActionResponse<bool> { WasSuccess = false, Message = "El cliente ya firmo todos sus documentos." };

            //Sin plantilla activa no hay nada que firmar
            var activeTypes = await _context.ContractDocumentTemplates
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId && x.Active)
                .Select(x => x.DocumentType)
                .Distinct()
                .ToListAsync();

            if (!signedTypes.Contains(ContractDocumentType.ConsentData) && !activeTypes.Contains(ContractDocumentType.ConsentData))
                return new ActionResponse<bool> { WasSuccess = false, Message = "No existe una plantilla activa de Consent Datos." };

            if (!signedTypes.Contains(ContractDocumentType.Contract) && !activeTypes.Contains(ContractDocumentType.Contract))
                return new ActionResponse<bool> { WasSuccess = false, Message = "No existe una plantilla activa de Contrato." };

            var provider = await _context.EmailProviderSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.CorporationId == corporationId && x.Active && x.IsDefault);

            if (provider == null)
                return new ActionResponse<bool> { WasSuccess = false, Message = "La corporacion no tiene proveedor de correo activo por defecto." };

            var link = $"{(urlFront ?? string.Empty).TrimEnd(SlashChar)}/client-dashboard";

            var body = LocalizedEmailTemplateFactory.BuildSignatureRequest(
                _localizer,
                contract.Client.FirstName,
                contract.Client.LastName,
                contract.ControlContrato.ToString(),
                string.Join(" y ", pending),
                link);

            var email = new EmailDeliveryDTO
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
                To = contract.Client.Email,
                NameTo = $"{contract.Client.FirstName} {contract.Client.LastName}",
                Subject = _localizer["SignatureRequest_Subject"],
                Body = body
            };

            var response = await _emailDeliveryService.SendAsync(email);
            if (!response.IsSuccess)
                return new ActionResponse<bool> { WasSuccess = false, Message = response.Message };

            //Se marca la solicitud: si ya estaba marcada solo se refresca la fecha, no se crea nada nuevo
            var tracked = await _context.ContractClients.FirstAsync(x => x.ContractClientId == contractClientId);
            tracked.SignatureRequestedAt = DateTime.UtcNow;

            //Bitacora: queda constancia de quien envio la solicitud y desde donde
            foreach (var documentType in PendingTypes(signedTypes))
                await AddEventAsync(contractClientId, documentType, SignatureEventType.RequestSent, MaskEmail(contract.Client.Email), context, corporationId);

            await _context.SaveChangesAsync();

            return new ActionResponse<bool> { WasSuccess = true, Result = true };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    public async Task<ActionResponse<IEnumerable<ContractSignedDocument>>> GetContractDocumentsAsync(Guid contractClientId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<IEnumerable<ContractSignedDocument>>();

            var list = await _context.ContractSignedDocuments
                .Include(x => x.ContractDocumentTemplate)
                .Where(x => x.ContractClientId == contractClientId && x.CorporationId == user.CorporationId!.Value)
                .OrderBy(x => x.DocumentType)
                .ToListAsync();

            foreach (var item in list)
                await SetSignedDocumentUrlAsync(item);

            return new ActionResponse<IEnumerable<ContractSignedDocument>> { WasSuccess = true, Result = list };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractSignedDocument>>(ex);
        }
    }

    public async Task<ActionResponse<ContractSignedDocument>> GenerateContractDocumentAsync(Guid contractClientId, Guid templateId, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ContractSignedDocument>();

            var contract = await GetContractAsync(contractClientId, user.CorporationId!.Value);
            var template = await GetTemplateWithFieldsAsync(templateId, user.CorporationId!.Value);

            if (contract == null || template == null)
                return NotFound<ContractSignedDocument>();

            var alreadySigned = await _context.ContractSignedDocuments
                .AnyAsync(x => x.ContractClientId == contractClientId && x.ContractDocumentTemplateId == templateId && x.Signed);

            if (alreadySigned)
                return new ActionResponse<ContractSignedDocument> { WasSuccess = false, Message = "El documento ya esta firmado." };

            //VISTA PREVIA: PDF lleno con los datos del cliente para revisarlo antes de firmar.
            //NO crea registro en la base: el documento del cliente solo se guarda cuando firma.
            //El archivo tiene nombre fijo por cliente+plantilla, asi cada apertura lo sobrescribe y no se acumulan.
            var pdfBytes = await BuildFilledPdfAsync(contract, template);
            var containerName = GetContainerName(template.DocumentType);
            var previewFileName = GetPreviewFileName(contractClientId, templateId);
            await _fileStorage.SaveFileAsync(pdfBytes, previewFileName, containerName, previewFileName, "application/pdf");

            var preview = new ContractSignedDocument
            {
                ContractClientId = contractClientId,
                ContractDocumentTemplateId = templateId,
                DocumentType = template.DocumentType,
                CorporationId = user.CorporationId!.Value,
                Signed = false,
                FileFullPath = await _fileStorage.GetBlobSasUrlAsync(previewFileName, containerName, TimeSpan.FromMinutes(30))
            };

            return new ActionResponse<ContractSignedDocument> { WasSuccess = true, Result = preview };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractSignedDocument>(ex);
        }
    }

    public async Task<ActionResponse<ContractSignedDocument>> GenerateContractDocumentByTypeAsync(Guid contractClientId, ContractDocumentType documentType, string username)
    {
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
                return AuthFail<ContractSignedDocument>();

            //Si el cliente ya firmo este tipo de documento (con cualquier plantilla, activa o no), se muestra
            //el firmado: cambiar la plantilla activa NO obliga a los clientes que ya firmaron a firmar de nuevo
            var signed = await _context.ContractSignedDocuments
                .AsNoTracking()
                .Where(x => x.ContractClientId == contractClientId &&
                            x.CorporationId == user.CorporationId!.Value &&
                            x.DocumentType == documentType &&
                            x.Signed)
                .OrderByDescending(x => x.DateSigned)
                .FirstOrDefaultAsync();

            if (signed != null)
            {
                await SetSignedDocumentUrlAsync(signed);
                return new ActionResponse<ContractSignedDocument> { WasSuccess = true, Result = signed };
            }

            var template = await _context.ContractDocumentTemplates
                .Where(x => x.CorporationId == user.CorporationId!.Value &&
                            x.DocumentType == documentType &&
                            x.Active)
                .OrderBy(x => x.Name)
                .FirstOrDefaultAsync();

            if (template == null)
                return new ActionResponse<ContractSignedDocument> { WasSuccess = false, Message = "No existe una plantilla activa para este documento." };

            return await GenerateContractDocumentAsync(contractClientId, template.ContractDocumentTemplateId, username);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractSignedDocument>(ex);
        }
    }

    public async Task<ActionResponse<ContractSignedDocument>> SignContractDocumentAsync(ContractSignedDocument model, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ContractSignedDocument>();
            }

            if (string.IsNullOrWhiteSpace(model.SignatureBase64))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractSignedDocument> { WasSuccess = false, Message = "Debe capturar la firma." };
            }

            //Se firma sobre la plantilla ACTIVA que el cliente reviso en la vista previa
            var corporationId = user.CorporationId!.Value;
            var contract = await GetContractAsync(model.ContractClientId, corporationId);
            var template = await GetTemplateWithFieldsAsync(model.ContractDocumentTemplateId, corporationId);

            if (contract == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return NotFound<ContractSignedDocument>();
            }

            if (template == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractSignedDocument> { WasSuccess = false, Message = "La plantilla ya no esta activa. Cierre y vuelva a abrir el documento." };
            }

            //Un solo documento firmado por tipo: si ya firmo (con cualquier plantilla) no firma de nuevo
            var alreadySigned = await _context.ContractSignedDocuments
                .AnyAsync(x => x.ContractClientId == model.ContractClientId &&
                               x.CorporationId == corporationId &&
                               x.DocumentType == template.DocumentType &&
                               x.Signed);

            if (alreadySigned)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractSignedDocument> { WasSuccess = false, Message = "El documento ya esta firmado." };
            }

            var signatureField = BuildPdfFields(template.ContractDocumentTemplateFields)
                .FirstOrDefault(x => string.Equals(x.FieldName, nameof(ContractDocumentFieldType.Signature), StringComparison.OrdinalIgnoreCase));

            if (signatureField == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractSignedDocument> { WasSuccess = false, Message = "La plantilla no tiene coordenadas para la firma." };
            }

            //Se llena la plantilla con los mismos datos de la vista previa y se estampa la firma
            var filledBytes = await BuildFilledPdfAsync(contract, template);
            var signedBytes = _pdfSignatureService.AddSignature(filledBytes, signatureField, model.SignatureBase64);
            var containerName = GetContainerName(template.DocumentType);
            var fileName = $"{Guid.NewGuid()}.pdf";
            var savedFileName = await _fileStorage.SaveFileAsync(signedBytes, fileName, containerName, fileName, "application/pdf");

            //Registro pendiente de la version anterior (antes se creaba al abrir): se reutiliza para no chocar con el indice unico
            var current = await _context.ContractSignedDocuments
                .FirstOrDefaultAsync(x => x.ContractClientId == model.ContractClientId && x.ContractDocumentTemplateId == template.ContractDocumentTemplateId);

            var oldPendingFileName = current?.FileName;

            if (current == null)
            {
                current = new ContractSignedDocument
                {
                    ContractClientId = model.ContractClientId,
                    ContractDocumentTemplateId = template.ContractDocumentTemplateId,
                    DocumentType = template.DocumentType,
                    DateCreated = DateTime.UtcNow.Date,
                    CorporationId = corporationId,
                    UsuarioOwner = $"{user.FirstName} {user.LastName}",
                    UserId = Guid.Parse(user.Id)
                };
                _context.ContractSignedDocuments.Add(current);
            }

            current.FileName = savedFileName;
            current.Signed = true;
            current.DateSigned = DateTime.UtcNow;
            current.UsuarioOwnerSigned = $"{user.FirstName} {user.LastName}";
            current.UserIdSigned = Guid.Parse(user.Id);

            await _transactionManager.SaveChangesAsync();

            //Si con esta firma ya tiene todo (fotos + Consentimiento + Contrato), pasa solo de Draft a Pending Approval
            await ContractRequirementRules.PromoteWhenCompleteAsync(_context, model.ContractClientId);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            //Despues del commit se limpian la vista previa y el PDF pendiente viejo (si existia)
            await _fileStorage.RemoveFileAsync(containerName, GetPreviewFileName(model.ContractClientId, template.ContractDocumentTemplateId));
            if (!string.IsNullOrWhiteSpace(oldPendingFileName))
                await _fileStorage.RemoveFileAsync(containerName, oldPendingFileName);

            await SetSignedDocumentUrlAsync(current);
            return new ActionResponse<ContractSignedDocument> { WasSuccess = true, Result = current };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractSignedDocument>(ex);
        }
    }

    //===================== Portal del cliente: firma con codigo (ver docs/Firma-Electronica-Part11.md) =====================

    private const int CodeMinutes = 15;
    private const int CodeMaxAttempts = 5;

    //Vigencia del enlace firmado del blob: alcanza para abrir el PDF y nada mas.
    //El enlace se pide en el momento de abrir el documento, no al listar.
    private const int LinkMinutes = 3;

    //Documentos del cliente que entro a su cuenta: los firmados y los que le faltan con su vista previa
    public async Task<ActionResponse<IEnumerable<MySignatureDocumentDTO>>> GetMyDocumentsAsync(string username)
    {
        try
        {
            var client = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserName == username);

            if (client == null)
                return new ActionResponse<IEnumerable<MySignatureDocumentDTO>> { WasSuccess = true, Result = new List<MySignatureDocumentDTO>() };

            //Solo los contratos cuya solicitud de firma ya se envio, o que ya tienen algo firmado.
            //Asi no le aparecen al cliente contratos que todavia se estan armando en la oficina.
            var contracts = await _context.ContractClients
                .AsNoTracking()
                .Include(x => x.Client)
                .ThenInclude(x => x!.DocumentType)
                .Include(x => x.Zone)
                .Include(x => x.ContractPlans!)
                .ThenInclude(x => x.Plan)
                .Where(x => x.ClientId == client.ClientId &&
                            (x.SignatureRequestedAt != null ||
                             _context.ContractSignedDocuments.Any(d => d.ContractClientId == x.ContractClientId && d.Signed)))
                .ToListAsync();

            var result = new List<MySignatureDocumentDTO>();

            foreach (var contract in contracts)
            {
                foreach (var documentType in new[] { ContractDocumentType.ConsentData, ContractDocumentType.Contract })
                {
                    var signed = await _context.ContractSignedDocuments
                        .AsNoTracking()
                        .Where(x => x.ContractClientId == contract.ContractClientId &&
                                    x.DocumentType == documentType &&
                                    x.Signed)
                        .OrderByDescending(x => x.DateSigned)
                        .FirstOrDefaultAsync();

                    var item = new MySignatureDocumentDTO
                    {
                        ContractClientId = contract.ContractClientId,
                        ContractNumber = contract.ControlContrato,
                        ContractDate = contract.DateCreado,
                        ContractAddress = contract.Address,
                        ZoneName = contract.Zone?.ZoneName,
                        PlanName = contract.ContractPlans?.FirstOrDefault()?.Plan?.PlanName,
                        DocumentType = documentType,
                        Signed = signed != null,
                        DateSigned = signed?.DateSigned,
                        VerificationCode = signed?.VerificationCode
                    };

                    //El listado NO trae enlaces: el PDF se pide aparte, al abrirlo, con un enlace
                    //de vigencia corta. Asi no quedan enlaces vivos en la pantalla ni en el historial.
                    if (signed != null && !string.IsNullOrWhiteSpace(signed.FileName))
                    {
                        result.Add(item);
                        continue;
                    }

                    //Pendiente: solo se muestra si la corporacion tiene plantilla activa
                    var hasTemplate = await _context.ContractDocumentTemplates
                        .AnyAsync(x => x.CorporationId == contract.CorporationId && x.DocumentType == documentType && x.Active);

                    if (hasTemplate)
                        result.Add(item);
                }
            }

            return new ActionResponse<IEnumerable<MySignatureDocumentDTO>> { WasSuccess = true, Result = result };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<MySignatureDocumentDTO>>(ex);
        }
    }

    //Envia al correo del cliente el codigo de un solo uso
    public async Task<ActionResponse<SignatureCodeDTO>> RequestSignatureCodeAsync(Guid contractClientId, ContractDocumentType documentType, ClaimsDTOs context)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var contract = await GetOwnedContractAsync(contractClientId, context.UserName);
            if (contract?.Client == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return NotFound<SignatureCodeDTO>();
            }

            if (await AlreadySignedAsync(contractClientId, documentType))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<SignatureCodeDTO> { WasSuccess = false, Message = "El documento ya esta firmado." };
            }

            if (string.IsNullOrWhiteSpace(contract.Client.Email))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<SignatureCodeDTO> { WasSuccess = false, Message = "El cliente no tiene correo registrado." };
            }

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var now = DateTime.UtcNow;

            var record = new ContractSignatureCode
            {
                ContractSignatureCodeId = Guid.NewGuid(),
                ContractClientId = contractClientId,
                DocumentType = documentType,
                Email = contract.Client.Email,
                CodeHash = HashCode(code, contractClientId, documentType),
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(CodeMinutes),
                Attempts = 0,
                RequestIp = context.SourceIp,
                RequestUserAgent = Trim(context.UserAgent, 512),
                CorporationId = contract.CorporationId,
                UsuarioOwner = context.UserName,
                UserId = Guid.TryParse(context.Id, out var userId) ? userId : null
            };

            _context.ContractSignatureCodes.Add(record);
            await AddEventAsync(contractClientId, documentType, SignatureEventType.CodeSent, MaskEmail(contract.Client.Email), context, contract.CorporationId);
            await _transactionManager.SaveChangesAsync();

            var sent = await SendCodeEmailAsync(contract, code);
            if (!sent.IsSuccess)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<SignatureCodeDTO> { WasSuccess = false, Message = sent.Message };
            }

            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<SignatureCodeDTO>
            {
                WasSuccess = true,
                Result = new SignatureCodeDTO { MaskedEmail = MaskEmail(contract.Client.Email), ExpiresAt = record.ExpiresAt }
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<SignatureCodeDTO>(ex);
        }
    }

    //Firma del cliente: valida el codigo, estampa la firma y guarda toda la evidencia
    public async Task<ActionResponse<bool>> SignMyDocumentAsync(SignDocumentRequestDTO model, string urlFront, ClaimsDTOs context)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            if (!model.TermsAccepted)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "Debe confirmar que leyo el documento y esta conforme." };
            }

            if (string.IsNullOrWhiteSpace(model.SignatureBase64))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "Debe capturar la firma." };
            }

            var contract = await GetOwnedContractAsync(model.ContractClientId, context.UserName);
            if (contract?.Client == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return NotFound<bool>();
            }

            //Idempotencia: si ya esta firmado no se vuelve a firmar
            if (await AlreadySignedAsync(model.ContractClientId, model.DocumentType))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "El documento ya esta firmado." };
            }

            //Codigo vigente mas reciente que no se haya usado
            var record = await _context.ContractSignatureCodes
                .Where(x => x.ContractClientId == model.ContractClientId &&
                            x.DocumentType == model.DocumentType &&
                            x.UsedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            if (record == null || record.ExpiresAt < DateTime.UtcNow)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "El codigo vencio. Solicite uno nuevo." };
            }

            if (record.Attempts >= CodeMaxAttempts)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "Supero los intentos permitidos. Solicite un codigo nuevo." };
            }

            //Codigo errado: se cuenta el intento y se guarda
            if (record.CodeHash != HashCode(model.Code?.Trim() ?? string.Empty, model.ContractClientId, model.DocumentType))
            {
                record.Attempts++;
                await AddEventAsync(model.ContractClientId, model.DocumentType, SignatureEventType.CodeFailed, $"Intento {record.Attempts}", context, contract.CorporationId);
                await _transactionManager.SaveChangesAsync();
                await _transactionManager.CommitTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "El codigo no es correcto." };
            }

            var template = await GetActiveTemplateAsync(contract.CorporationId, model.DocumentType);
            if (template == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "No existe una plantilla activa para este documento." };
            }

            var signatureField = BuildPdfFields(template.ContractDocumentTemplateFields)
                .FirstOrDefault(x => string.Equals(x.FieldName, nameof(ContractDocumentFieldType.Signature), StringComparison.OrdinalIgnoreCase));

            if (signatureField == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "La plantilla no tiene coordenadas para la firma." };
            }

            var signedAt = DateTime.UtcNow;

            //Se llena la plantilla con los mismos datos que vio, se estampa la firma y el sello de evidencia
            var filledBytes = await BuildFilledPdfAsync(contract, template);
            var signedBytes = _pdfSignatureService.AddSignature(filledBytes, signatureField, model.SignatureBase64);

            var footer = $"Firmado electronicamente el {signedAt:yyyy-MM-dd HH:mm} UTC | Metodo: Portal verificado | Correo: {MaskEmail(record.Email)} | IP: {context.SourceIp}";
            signedBytes = _pdfSignatureService.AddEvidenceFooter(signedBytes, footer);

            //La huella se calcula sobre el documento firmado, ANTES de anexar el certificado
            //(que es justo donde se imprime): asi lo que dice el papel es lo que guarda la base.
            var documentHash = Sha256(signedBytes);

            var corporationName = await _context.Corporations
                .AsNoTracking()
                .Where(x => x.CorporationId == contract.CorporationId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            var verificationCode = await NewVerificationCodeAsync();
            var verificationUrl = $"{(urlFront ?? string.Empty).TrimEnd(SlashChar)}/verificar/{verificationCode}";

            var certificate = BuildCertificate(contract, template, record, corporationName, context, signedAt,
                documentHash, verificationCode, verificationUrl);

            signedBytes = _pdfSignatureService.AddCertificatePage(signedBytes, certificate);

            //El archivo queda cerrado: se lee e imprime, no se edita
            signedBytes = _pdfSignatureService.Protect(signedBytes);

            //Huella del archivo final: es la que puede comprobar cualquiera que descargue el PDF
            var fileHash = Sha256(signedBytes);

            var containerName = GetContainerName(model.DocumentType);
            var fileName = $"{Guid.NewGuid()}.pdf";
            var savedFileName = await _fileStorage.SaveFileAsync(signedBytes, fileName, containerName, fileName, "application/pdf");

            //Se reutiliza el registro pendiente si existe, para no chocar con el indice unico
            var current = await _context.ContractSignedDocuments
                .FirstOrDefaultAsync(x => x.ContractClientId == model.ContractClientId &&
                                          x.ContractDocumentTemplateId == template.ContractDocumentTemplateId);

            var oldFileName = current?.FileName;
            var signerName = $"{contract.Client.FirstName} {contract.Client.LastName}".Trim();

            if (current == null)
            {
                current = new ContractSignedDocument
                {
                    ContractClientId = model.ContractClientId,
                    ContractDocumentTemplateId = template.ContractDocumentTemplateId,
                    DocumentType = model.DocumentType,
                    DateCreated = signedAt.Date,
                    CorporationId = contract.CorporationId,
                    UsuarioOwner = signerName,
                    UserId = Guid.TryParse(context.Id, out var ownerId) ? ownerId : null
                };
                _context.ContractSignedDocuments.Add(current);
            }

            current.FileName = savedFileName;
            current.Signed = true;
            current.DateSigned = signedAt;
            current.UsuarioOwnerSigned = signerName;
            current.UserIdSigned = Guid.TryParse(context.Id, out var signerId) ? signerId : null;

            //Evidencia de la firma electronica
            current.SignatureMethod = SignatureMethod.PortalVerified;
            current.SignerEmail = record.Email;
            current.CodeSentAt = record.CreatedAt;
            current.CodeValidatedAt = signedAt;
            current.SignerIp = context.SourceIp;
            current.SignerUserAgent = Trim(context.UserAgent, 512);
            current.TermsAcceptedAt = signedAt;
            current.DocumentHash = documentHash;
            current.FileHash = fileHash;
            current.VerificationCode = verificationCode;
            current.ConsentVersion = ElectronicSignatureConsent.Version;
            current.ConsentHash = Sha256(Encoding.UTF8.GetBytes(ElectronicSignatureConsent.Text));

            //El codigo queda quemado: un solo uso
            record.UsedAt = signedAt;

            await AddEventAsync(model.ContractClientId, model.DocumentType, SignatureEventType.CodeValidated, MaskEmail(record.Email), context, contract.CorporationId);
            await AddEventAsync(model.ContractClientId, model.DocumentType, SignatureEventType.Signed, verificationCode, context, contract.CorporationId);

            await _transactionManager.SaveChangesAsync();

            //Si con esta firma ya tiene todo, el contrato pasa solo de Draft a Pending Approval
            await ContractRequirementRules.PromoteWhenCompleteAsync(_context, model.ContractClientId);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            //Despues del commit se limpian la vista previa y el PDF pendiente viejo
            await _fileStorage.RemoveFileAsync(containerName, GetPreviewFileName(model.ContractClientId, template.ContractDocumentTemplateId));
            if (!string.IsNullOrWhiteSpace(oldFileName))
                await _fileStorage.RemoveFileAsync(containerName, oldFileName);

            return new ActionResponse<bool> { WasSuccess = true, Result = true };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    //Hoja de certificado que se anexa al final: todo el rastro de la firma en el propio documento
    private static PdfCertificateData BuildCertificate(ContractClient contract, ContractDocumentTemplate template,
        ContractSignatureCode record, string? corporationName, ClaimsDTOs context, DateTime signedAt, string documentHash,
        string verificationCode, string verificationUrl)
    {
        var client = contract.Client!;
        var documento = $"{client.DocumentType?.DocumentName} {FormatDocumentNumber(client.Document)}".Trim();
        var tipoDocumento = template.DocumentType == ContractDocumentType.Contract ? "Contrato de servicio" : "Consentimiento de datos";

        return new PdfCertificateData
        {
            Title = "Certificado de firma electronica",
            Subtitle = $"{corporationName} - Documento {tipoDocumento} del contrato #{contract.ControlContrato}",
            VerificationCode = verificationCode,
            VerificationUrl = verificationUrl,
            Sections = new List<PdfCertificateSection>
            {
                new()
                {
                    Title = "Documento",
                    Items = new List<PdfCertificateItem>
                    {
                        new("Plantilla firmada", template.Name),
                        new("Tipo de documento", tipoDocumento),
                        new("Contrato", $"#{contract.ControlContrato}"),
                        new("Direccion del servicio", contract.Address)
                    }
                },
                new()
                {
                    Title = "Firmante",
                    Items = new List<PdfCertificateItem>
                    {
                        new("Nombre", $"{client.FirstName} {client.LastName}".Trim()),
                        new("Documento de identidad", documento),
                        new("Usuario de la plataforma", context.UserName),
                        new("Correo registrado", record.Email)
                    }
                },
                new()
                {
                    Title = "Verificacion de identidad",
                    Items = new List<PdfCertificateItem>
                    {
                        new("Metodo", "Portal verificado: clave de la cuenta + codigo de un solo uso al correo"),
                        new("Codigo enviado el", $"{record.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC"),
                        new("Codigo validado el", $"{signedAt:yyyy-MM-dd HH:mm:ss} UTC"),
                        new("Intentos fallidos", record.Attempts.ToString()),
                        new("Acepto haber leido y estar conforme", $"{signedAt:yyyy-MM-dd HH:mm:ss} UTC"),
                        new("Direccion IP de origen", context.SourceIp),
                        new("Dispositivo y navegador", UserAgentReader.Describe(context.UserAgent)),
                        new("Identificacion completa del navegador", context.UserAgent)
                    }
                },
                new()
                {
                    Title = "Integridad del documento",
                    Items = new List<PdfCertificateItem>
                    {
                        new("Algoritmo", "SHA-256"),
                        new("Huella del documento firmado", documentHash),
                        new("Aviso de firma electronica aceptado", $"Version {ElectronicSignatureConsent.Version}")
                    }
                }
            },
            Note = "Esta hoja forma parte inseparable del documento y certifica su firma electronica. La huella SHA-256 " +
                   "corresponde al documento firmado antes de anexar este certificado: cualquier cambio posterior en el " +
                   "contenido produce una huella distinta. El codigo de verificacion es de un solo uso, vence a los 15 " +
                   "minutos y nunca se guarda en texto plano. La direccion IP y el navegador los registra el servidor, " +
                   "no el equipo del firmante."
        };
    }

    //Enlace del PDF, pedido en el momento de abrirlo. Dura pocos minutos: sirve para abrir el
    //documento y leerlo, no para compartirlo. De paso queda en la bitacora que lo abrio.
    public async Task<ActionResponse<SignatureLinkDTO>> GetMyDocumentLinkAsync(Guid contractClientId, ContractDocumentType documentType, ClaimsDTOs context)
    {
        try
        {
            var contract = await GetOwnedContractAsync(contractClientId, context.UserName);
            if (contract == null)
                return NotFound<SignatureLinkDTO>();

            var containerName = GetContainerName(documentType);

            //Ya firmado: se entrega el documento definitivo
            var signed = await _context.ContractSignedDocuments
                .AsNoTracking()
                .Where(x => x.ContractClientId == contractClientId && x.DocumentType == documentType && x.Signed)
                .OrderByDescending(x => x.DateSigned)
                .FirstOrDefaultAsync();

            if (signed != null && !string.IsNullOrWhiteSpace(signed.FileName))
            {
                var signedUrl = await _fileStorage.GetBlobSasUrlAsync(signed.FileName, containerName, TimeSpan.FromMinutes(LinkMinutes));
                return new ActionResponse<SignatureLinkDTO> { WasSuccess = true, Result = new SignatureLinkDTO { Url = signedUrl ?? string.Empty } };
            }

            //Pendiente: se arma la vista previa con la plantilla activa (no se guarda nada en la base)
            var template = await GetActiveTemplateAsync(contract.CorporationId, documentType);
            if (template == null)
                return new ActionResponse<SignatureLinkDTO> { WasSuccess = false, Message = "No existe una plantilla activa para este documento." };

            var previewUrl = await BuildPreviewUrlAsync(contract, template);

            await AddEventAsync(contractClientId, documentType, SignatureEventType.DocumentViewed, null, context, contract.CorporationId);
            await _context.SaveChangesAsync();

            return new ActionResponse<SignatureLinkDTO> { WasSuccess = true, Result = new SignatureLinkDTO { Url = previewUrl ?? string.Empty } };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<SignatureLinkDTO>(ex);
        }
    }

    //Verificacion publica: cualquiera con el identificador impreso puede comprobar la firma.
    //No pide sesion y no entrega datos personales completos.
    public async Task<ActionResponse<SignatureVerificationDTO>> VerifySignatureAsync(string verificationCode, ClaimsDTOs context)
    {
        try
        {
            var code = (verificationCode ?? string.Empty).Trim().ToUpperInvariant();

            var document = await _context.ContractSignedDocuments
                .AsNoTracking()
                .Include(x => x.ContractClient)
                .ThenInclude(x => x!.Client)
                .Include(x => x.Corporation)
                .FirstOrDefaultAsync(x => x.VerificationCode == code && x.Signed);

            if (document?.ContractClient == null)
                return new ActionResponse<SignatureVerificationDTO> { WasSuccess = true, Result = new SignatureVerificationDTO { Found = false, VerificationCode = code } };

            //La IP y el navegador son datos personales del firmante: los ve el propio firmante
            //o alguien de la corporacion que emitio el documento, nadie mas.
            var user = await _userHelper.GetUserByUserNameAsync(context.UserName);
            var esDeLaCorporacion = user?.CorporationId == document.CorporationId;
            var esElFirmante = document.ContractClient.Client?.UserName == context.UserName;
            var puedeVerOrigen = esDeLaCorporacion || esElFirmante;

            var events = await _context.ContractSignatureEvents
                .AsNoTracking()
                .Where(x => x.ContractClientId == document.ContractClientId && x.DocumentType == document.DocumentType)
                .OrderBy(x => x.CreatedAt)
                //La IP y el navegador del firmante son datos personales: quedan en la bitacora
                //interna, no en la consulta publica
                .Select(x => new SignatureEventDTO
                {
                    EventType = x.EventType,
                    CreatedAt = x.CreatedAt,
                    Detail = x.Detail,
                    SourceIp = puedeVerOrigen ? x.SourceIp : null,
                    UserAgent = puedeVerOrigen ? x.UserAgent : null
                })
                .ToListAsync();

            var client = document.ContractClient.Client;

            var result = new SignatureVerificationDTO
            {
                Found = true,
                VerificationCode = code,
                CorporationName = document.Corporation?.Name,
                DocumentName = document.DocumentType == ContractDocumentType.Contract ? "Contrato de servicio" : "Consentimiento de datos",
                ContractNumber = document.ContractClient.ControlContrato,
                SignerName = MaskName($"{client?.FirstName} {client?.LastName}".Trim()),
                SignerEmail = MaskEmail(document.SignerEmail ?? string.Empty),
                SignedAt = document.DateSigned,
                Method = document.SignatureMethod == SignatureMethod.PortalVerified
                    ? "Portal verificado: clave de la cuenta + codigo de un solo uso al correo"
                    : "Firma presencial verificada",
                DocumentHash = document.DocumentHash,
                FileHash = document.FileHash,
                ConsentVersion = document.ConsentVersion,
                SignerIp = puedeVerOrigen ? document.SignerIp : null,
                SignerUserAgent = puedeVerOrigen ? document.SignerUserAgent : null,
                Events = events
            };

            return new ActionResponse<SignatureVerificationDTO> { WasSuccess = true, Result = result };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<SignatureVerificationDTO>(ex);
        }
    }

    //----- Bitacora e identificadores -----

    //Solo agrega el renglon: quien llama decide cuando guardar, dentro de su transaccion
    //Un paso de la firma queda en dos sitios: su bitacora propia, que es la evidencia
    //que sostiene el certificado, y la bitacora del contrato, para verlo junto a lo demas.
    private async Task AddEventAsync(Guid contractClientId, ContractDocumentType documentType, SignatureEventType eventType,
        string? detail, ClaimsDTOs context, int corporationId)
    {
        var detalle = string.IsNullOrWhiteSpace(detail) ? documentType.ToString() : $"{documentType} - {detail}";

        await ContractAuditLog.AddAsync(_context, contractClientId, ToContractEvent(eventType), detalle,
            context.UserName,
            Guid.TryParse(context.Id, out var auditUserId) ? auditUserId : null,
            sourceIp: context.SourceIp,
            userAgent: context.UserAgent,
            corporationId: corporationId);

        AddEvent(contractClientId, documentType, eventType, detail, context, corporationId);
    }

    private static ContractEventType ToContractEvent(SignatureEventType eventType) => eventType switch
    {
        SignatureEventType.RequestSent => ContractEventType.SignatureRequested,
        SignatureEventType.DocumentViewed => ContractEventType.DocumentViewed,
        SignatureEventType.CodeSent => ContractEventType.CodeSent,
        SignatureEventType.CodeFailed => ContractEventType.CodeFailed,
        SignatureEventType.CodeValidated => ContractEventType.CodeValidated,
        _ => ContractEventType.Signed
    };


    private void AddEvent(Guid contractClientId, ContractDocumentType documentType, SignatureEventType eventType,
        string? detail, ClaimsDTOs context, int corporationId)
    {
        _context.ContractSignatureEvents.Add(new ContractSignatureEvent
        {
            ContractSignatureEventId = Guid.NewGuid(),
            ContractClientId = contractClientId,
            DocumentType = documentType,
            EventType = eventType,
            CreatedAt = DateTime.UtcNow,
            Detail = detail,
            SourceIp = context.SourceIp,
            UserAgent = Trim(context.UserAgent, 512),
            CorporationId = corporationId,
            UsuarioOwner = context.UserName,
            UserId = Guid.TryParse(context.Id, out var userId) ? userId : null
        });
    }

    private static IEnumerable<ContractDocumentType> PendingTypes(List<ContractDocumentType> signedTypes)
    {
        if (!signedTypes.Contains(ContractDocumentType.ConsentData))
            yield return ContractDocumentType.ConsentData;

        if (!signedTypes.Contains(ContractDocumentType.Contract))
            yield return ContractDocumentType.Contract;
    }

    //Identificador publico: 12 caracteres en grupos, sin letras que se confundan (I, O, 0, 1)
    private async Task<string> NewVerificationCodeAsync()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var letters = new char[12];
            for (var i = 0; i < letters.Length; i++)
                letters[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];

            var code = $"{new string(letters, 0, 4)}-{new string(letters, 4, 4)}-{new string(letters, 8, 4)}";

            if (!await _context.ContractSignedDocuments.AnyAsync(x => x.VerificationCode == code))
                return code;
        }

        //Practicamente imposible: 32^12 combinaciones
        throw new InvalidOperationException("No se pudo generar el identificador de firma.");
    }

    //Nombre enmascarado para la pagina publica: "Rolando Rojo" -> "Ro***** Ro**"
    private static string MaskName(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return string.Join(' ', parts.Select(part =>
            part.Length <= 2 ? part : $"{part[..2]}{new string('*', part.Length - 2)}"));
    }

    //----- Apoyos del portal del cliente -----

    //El cliente solo puede tocar SUS contratos: se busca por el usuario logueado, no por corporacion
    private async Task<ContractClient?> GetOwnedContractAsync(Guid contractClientId, string username)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == username);

        if (client == null)
            return null;

        return await _context.ContractClients
            .AsNoTracking()
            .Include(x => x.Client)
            .ThenInclude(x => x!.DocumentType)
            .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId && x.ClientId == client.ClientId);
    }

    private async Task<bool> AlreadySignedAsync(Guid contractClientId, ContractDocumentType documentType) =>
        await _context.ContractSignedDocuments
            .AnyAsync(x => x.ContractClientId == contractClientId && x.DocumentType == documentType && x.Signed);

    private async Task<ContractDocumentTemplate?> GetActiveTemplateAsync(int corporationId, ContractDocumentType documentType) =>
        await _context.ContractDocumentTemplates
            .AsNoTracking()
            .Include(x => x.ContractDocumentTemplateFields)
            .FirstOrDefaultAsync(x => x.CorporationId == corporationId && x.DocumentType == documentType && x.Active);

    private async Task<string?> BuildPreviewUrlAsync(ContractClient contract, ContractDocumentTemplate template)
    {
        var pdfBytes = await BuildFilledPdfAsync(contract, template);
        var containerName = GetContainerName(template.DocumentType);
        var previewFileName = GetPreviewFileName(contract.ContractClientId, template.ContractDocumentTemplateId);

        await _fileStorage.SaveFileAsync(pdfBytes, previewFileName, containerName, previewFileName, "application/pdf");
        return await _fileStorage.GetBlobSasUrlAsync(previewFileName, containerName, TimeSpan.FromMinutes(LinkMinutes));
    }

    private async Task<Response> SendCodeEmailAsync(ContractClient contract, string code)
    {
        var provider = await _context.EmailProviderSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CorporationId == contract.CorporationId && x.Active && x.IsDefault);

        if (provider == null)
            return new Response { IsSuccess = false, Message = "La corporacion no tiene proveedor de correo activo por defecto." };

        var body = LocalizedEmailTemplateFactory.BuildSignatureCode(
            _localizer,
            contract.Client!.FirstName,
            contract.Client.LastName,
            code,
            CodeMinutes);

        var email = new EmailDeliveryDTO
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
            To = contract.Client.Email,
            NameTo = $"{contract.Client.FirstName} {contract.Client.LastName}",
            Subject = _localizer["SignatureCode_Subject"],
            Body = body
        };

        return await _emailDeliveryService.SendAsync(email);
    }

    //El codigo NUNCA se guarda en texto plano, solo su hash
    private static string HashCode(string code, Guid contractClientId, ContractDocumentType documentType)
    {
        var raw = $"{code}|{contractClientId}|{(int)documentType}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }

    //Huella del PDF firmado: demuestra que no se cambio despues
    private static string Sha256(byte[] content) => Convert.ToHexString(SHA256.HashData(content));

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1)
            return email;

        var visible = Math.Min(2, at);
        return $"{email[..visible]}{new string('*', Math.Max(1, at - visible))}{email[at..]}";
    }

    private static string? Trim(string? value, int maxLength) =>
        string.IsNullOrEmpty(value) || value.Length <= maxLength ? value : value[..maxLength];

    //Solo lectura (AsNoTracking): se usan para llenar el PDF y asi no se enganchan al documento que se devuelve
    private async Task<ContractClient?> GetContractAsync(Guid contractClientId, int corporationId) =>
        await _context.ContractClients
            .AsNoTracking()
            .Include(x => x.Client)
            .ThenInclude(x => x!.DocumentType)
            .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId && x.CorporationId == corporationId);

    private async Task<ContractDocumentTemplate?> GetTemplateWithFieldsAsync(Guid templateId, int corporationId) =>
        await _context.ContractDocumentTemplates
            .AsNoTracking()
            .Include(x => x.ContractDocumentTemplateFields)
            .FirstOrDefaultAsync(x => x.ContractDocumentTemplateId == templateId && x.CorporationId == corporationId && x.Active);

    private async Task<byte[]> BuildFilledPdfAsync(ContractClient contract, ContractDocumentTemplate template)
    {
        var templateBytes = await GetFileBytesAsync(template.FileName!, GetContainerName(template.DocumentType));
        var fields = BuildPdfFields(template.ContractDocumentTemplateFields);
        return _pdfSignatureService.FillPdf(templateBytes, fields, BuildValues(contract));
    }

    private static string GetPreviewFileName(Guid contractClientId, Guid templateId) =>
        $"preview-{contractClientId}-{templateId}.pdf";

    private async Task<bool> OtherActiveTemplateExistsAsync(int corporationId, ContractDocumentType documentType, Guid excludeTemplateId) =>
        await _context.ContractDocumentTemplates
            .AnyAsync(x => x.CorporationId == corporationId &&
                           x.DocumentType == documentType &&
                           x.Active &&
                           x.ContractDocumentTemplateId != excludeTemplateId);

    private async Task<byte[]> GetFileBytesAsync(string fileName, string containerName)
    {
        var file = await _fileStorage.GetFileBase64Async(fileName, containerName);
        if (string.IsNullOrWhiteSpace(file?.Base64))
            throw new InvalidOperationException("No se encontro el archivo PDF.");

        return ReadBase64File(file.Base64);
    }

    private async Task SetTemplateUrlAsync(ContractDocumentTemplate model)
    {
        model.FileFullPath = await _fileStorage.GetBlobSasUrlAsync(model.FileName, GetContainerName(model.DocumentType), TimeSpan.FromMinutes(30));
    }

    private async Task SetSignedDocumentUrlAsync(ContractSignedDocument model)
    {
        if (!string.IsNullOrWhiteSpace(model.FileName))
            model.FileFullPath = await _fileStorage.GetBlobSasUrlAsync(model.FileName, GetContainerName(model.DocumentType), TimeSpan.FromMinutes(30));
    }

    private string GetContainerName(ContractDocumentType documentType) =>
        documentType == ContractDocumentType.Contract ? _imgOption.ContractContract : _imgOption.ContractConsent;

    private static Dictionary<string, string?> BuildValues(ContractClient contract)
    {
        var client = contract.Client;
        var fullName = client == null ? string.Empty : $"{client.FirstName} {client.LastName}";

        //Documento: abreviatura del tipo + numero con puntos de miles (CC 1.220.478.524)
        var document = client == null ? string.Empty : $"{client.DocumentType?.DocumentName} {FormatDocumentNumber(client.Document)}".Trim();

        //Nombre imprenta: primer nombre + primer apellido
        var printName = client == null ? string.Empty : $"{FirstWord(client.FirstName)} {FirstWord(client.LastName)}".Trim();

        //Direccion del contrato (donde se presta el servicio); si no tiene, la del cliente
        var address = string.IsNullOrWhiteSpace(contract.Address) ? client?.Address : contract.Address;

        return new Dictionary<string, string?>
        {
            [nameof(ContractDocumentFieldType.FullName)] = fullName,
            [nameof(ContractDocumentFieldType.Document)] = document,
            [nameof(ContractDocumentFieldType.Phone)] = contract.PhoneNumber,
            [nameof(ContractDocumentFieldType.Date)] = DateTime.UtcNow.ToString("MM/dd/yyyy"),
            [nameof(ContractDocumentFieldType.Address)] = address,
            [nameof(ContractDocumentFieldType.Email)] = client?.Email,
            [nameof(ContractDocumentFieldType.PrintName)] = printName
        };
    }

    //Solo si el documento es todo numeros se agrupan los miles; si trae letras o puntos se deja igual
    private static string FormatDocumentNumber(string? document)
    {
        if (string.IsNullOrWhiteSpace(document))
            return string.Empty;

        var clean = document.Trim();
        if (!clean.All(char.IsDigit))
            return clean;

        var groups = new List<string>();
        for (var end = clean.Length; end > 0; end -= 3)
            groups.Insert(0, clean[Math.Max(0, end - 3)..end]);

        return string.Join(".", groups);
    }

    private static string FirstWord(string? value) =>
        value?.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

    private static Dictionary<string, string?> BuildTestValues() =>
        new()
        {
            [nameof(ContractDocumentFieldType.FullName)] = "Cliente Prueba Spix",
            [nameof(ContractDocumentFieldType.Document)] = "CC 1.220.478.524",
            [nameof(ContractDocumentFieldType.Phone)] = "305-555-0100",
            [nameof(ContractDocumentFieldType.Date)] = DateTime.UtcNow.ToString("MM/dd/yyyy"),
            [nameof(ContractDocumentFieldType.Address)] = "Calle 10 # 20-30 Barrio Centro",
            [nameof(ContractDocumentFieldType.Email)] = "cliente@correo.com",
            [nameof(ContractDocumentFieldType.PrintName)] = "Cliente Prueba",
            ["SignatureTest"] = "Firma Test"
        };

    private static IEnumerable<PdfSignatureField> BuildTestPdfFields(IEnumerable<PdfSignatureField> fields)
    {
        foreach (var field in fields)
        {
            yield return field;

            if (string.Equals(field.FieldName, nameof(ContractDocumentFieldType.Signature), StringComparison.OrdinalIgnoreCase))
            {
                //El texto de prueba va a la mitad del recuadro de la firma (el texto se escribe sobre su linea base)
                yield return new PdfSignatureField
                {
                    FieldName = "SignatureTest",
                    PageNumber = field.PageNumber,
                    PositionX = field.PositionX,
                    PositionY = field.PositionY + (field.Height ?? 60) / 2,
                    Width = field.Width,
                    Height = field.Height,
                    FontSize = field.FontSize
                };
            }
        }
    }

    private static IEnumerable<PdfSignatureField> BuildPdfFields(IEnumerable<ContractDocumentTemplateField>? fields) =>
        fields?.Select(x => new PdfSignatureField
        {
            FieldName = x.FieldType.ToString(),
            PageNumber = x.PageNumber,
            PositionX = (double)x.PositionX,
            PositionY = (double)x.PositionY,
            Width = x.Width.HasValue ? (double)x.Width.Value : null,
            Height = x.Height.HasValue ? (double)x.Height.Value : null,
            FontSize = x.FontSize <= 0 ? 12 : x.FontSize
        }) ?? Enumerable.Empty<PdfSignatureField>();

    private static byte[] ReadBase64File(string? fileBase64)
    {
        if (string.IsNullOrWhiteSpace(fileBase64))
            return Array.Empty<byte>();

        var cleanBase64 = fileBase64.Contains(',')
            ? fileBase64[(fileBase64.IndexOf(',') + 1)..]
            : fileBase64;

        return Convert.FromBase64String(cleanBase64);
    }

    private static ActionResponse<T> AuthFail<T>() =>
        new() { WasSuccess = false, Message = "No fue posible validar el usuario." };

    private static ActionResponse<T> InvalidPdf<T>() =>
        new() { WasSuccess = false, Message = "El archivo debe ser un PDF valido." };

    private static ActionResponse<T> ActiveTemplateExists<T>() =>
        new() { WasSuccess = false, Message = "Ya existe una plantilla activa de este tipo. Desactive la actual antes de activar otra." };

    private static ActionResponse<T> NotFound<T>() =>
        new() { WasSuccess = false, Message = "Recurso no encontrado." };
}
