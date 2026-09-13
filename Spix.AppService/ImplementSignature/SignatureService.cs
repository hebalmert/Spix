using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.ImplementContratos;
using Spix.AppService.InterfacesSignature;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;
using Spix.xFiles.SignatureHelper;

namespace Spix.AppService.ImplementSignature;

public class SignatureService : ISignatureService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserHelper _userHelper;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IFileStorage _fileStorage;
    private readonly IPdfSignatureService _pdfSignatureService;
    private readonly ImgSetting _imgOption;

    public SignatureService(DataContext context, IHttpContextAccessor httpContextAccessor,
        IUserHelper userHelper, ITransactionManager transactionManager, HttpErrorHandler httpErrorHandler,
        IFileStorage fileStorage, IPdfSignatureService pdfSignatureService, IOptions<ImgSetting> imgOption)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _userHelper = userHelper;
        _transactionManager = transactionManager;
        _httpErrorHandler = httpErrorHandler;
        _fileStorage = fileStorage;
        _pdfSignatureService = pdfSignatureService;
        _imgOption = imgOption.Value;
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
            current.DateSigned = DateTime.UtcNow.Date;
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
