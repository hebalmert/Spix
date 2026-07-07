using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
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

            var queryable = _context.ContractDocumentTemplates
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

            var fileName = $"{Guid.NewGuid()}.pdf";
            var containerName = GetContainerName(model.DocumentType);
            model.FileName = await _fileStorage.SaveFileAsync(fileBytes, fileName, containerName, model.OriginalFileName ?? fileName, "application/pdf");
            model.DateCreated = DateTime.UtcNow.Date;
            model.Active = true;
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

            current.Name = model.Name;
            current.DocumentType = model.DocumentType;
            current.PageCount = model.PageCount;
            current.Active = model.Active;

            if (!string.IsNullOrWhiteSpace(model.FileBase64))
            {
                var oldContainerName = GetContainerName(current.DocumentType);
                await _fileStorage.RemoveFileAsync(oldContainerName, current.FileName);

                var fileBytes = ReadBase64File(model.FileBase64);
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

            var template = await GetTemplateWithFieldsAsync(templateId, user.CorporationId!.Value);
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

            var hasSignedDocuments = await _context.ContractSignedDocuments.AnyAsync(x => x.ContractDocumentTemplateId == id);
            if (hasSignedDocuments)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<bool> { WasSuccess = false, Message = "No se puede eliminar una plantilla que ya tiene documentos generados." };
            }

            _context.ContractDocumentTemplates.Remove(model);
            await _fileStorage.RemoveFileAsync(GetContainerName(model.DocumentType), model.FileName);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

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
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return AuthFail<ContractSignedDocument>();
            }

            var contract = await GetContractAsync(contractClientId, user.CorporationId!.Value);
            var template = await GetTemplateWithFieldsAsync(templateId, user.CorporationId!.Value);

            if (contract == null || template == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return NotFound<ContractSignedDocument>();
            }

            var existing = await _context.ContractSignedDocuments
                .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId && x.ContractDocumentTemplateId == templateId);

            if (existing?.Signed == true)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractSignedDocument> { WasSuccess = false, Message = "El documento ya esta firmado." };
            }

            var templateBytes = await GetFileBytesAsync(template.FileName, GetContainerName(template.DocumentType));
            var values = BuildValues(contract);
            var fields = BuildPdfFields(template.ContractDocumentTemplateFields);
            var pdfBytes = _pdfSignatureService.FillPdf(templateBytes, fields, values);
            var fileName = $"{Guid.NewGuid()}.pdf";
            var containerName = GetContainerName(template.DocumentType);
            var savedFileName = await _fileStorage.SaveFileAsync(pdfBytes, fileName, containerName, fileName, "application/pdf");

            if (existing == null)
            {
                existing = new ContractSignedDocument
                {
                    ContractClientId = contractClientId,
                    ContractDocumentTemplateId = templateId,
                    DocumentType = template.DocumentType,
                    DateCreated = DateTime.UtcNow.Date,
                    CorporationId = user.CorporationId!.Value,
                    UsuarioOwner = $"{user.FirstName} {user.LastName}",
                    UserId = Guid.Parse(user.Id)
                };
                _context.ContractSignedDocuments.Add(existing);
            }
            else if (!string.IsNullOrWhiteSpace(existing.FileName))
            {
                await _fileStorage.RemoveFileAsync(containerName, existing.FileName);
            }

            existing.FileName = savedFileName;
            existing.Signed = false;

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            await SetSignedDocumentUrlAsync(existing);
            return new ActionResponse<ContractSignedDocument> { WasSuccess = true, Result = existing };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
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

            var current = await _context.ContractSignedDocuments
                .Include(x => x.ContractDocumentTemplate)
                .ThenInclude(x => x!.ContractDocumentTemplateFields)
                .FirstOrDefaultAsync(x => x.ContractSignedDocumentId == model.ContractSignedDocumentId &&
                                          x.CorporationId == user.CorporationId!.Value);

            if (current == null || current.ContractDocumentTemplate == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return NotFound<ContractSignedDocument>();
            }

            if (current.Signed)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractSignedDocument> { WasSuccess = false, Message = "El documento ya esta firmado." };
            }

            if (string.IsNullOrWhiteSpace(model.SignatureBase64))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractSignedDocument> { WasSuccess = false, Message = "Debe capturar la firma." };
            }

            var signatureField = BuildPdfFields(current.ContractDocumentTemplate.ContractDocumentTemplateFields)
                .FirstOrDefault(x => string.Equals(x.FieldName, nameof(ContractDocumentFieldType.Signature), StringComparison.OrdinalIgnoreCase));

            if (signatureField == null)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractSignedDocument> { WasSuccess = false, Message = "La plantilla no tiene coordenadas para la firma." };
            }

            var containerName = GetContainerName(current.DocumentType);
            var currentBytes = await GetFileBytesAsync(current.FileName!, containerName);
            var signedBytes = _pdfSignatureService.AddSignature(currentBytes, signatureField, model.SignatureBase64);
            var fileName = $"{Guid.NewGuid()}.pdf";
            var savedFileName = await _fileStorage.SaveFileAsync(signedBytes, fileName, containerName, fileName, "application/pdf");

            await _fileStorage.RemoveFileAsync(containerName, current.FileName!);

            current.FileName = savedFileName;
            current.Signed = true;
            current.DateSigned = DateTime.UtcNow.Date;
            current.UsuarioOwnerSigned = $"{user.FirstName} {user.LastName}";
            current.UserIdSigned = Guid.Parse(user.Id);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            await SetSignedDocumentUrlAsync(current);
            return new ActionResponse<ContractSignedDocument> { WasSuccess = true, Result = current };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractSignedDocument>(ex);
        }
    }

    private async Task<ContractClient?> GetContractAsync(Guid contractClientId, int corporationId) =>
        await _context.ContractClients
            .Include(x => x.Client)
            .ThenInclude(x => x!.DocumentType)
            .FirstOrDefaultAsync(x => x.ContractClientId == contractClientId && x.CorporationId == corporationId);

    private async Task<ContractDocumentTemplate?> GetTemplateWithFieldsAsync(Guid templateId, int corporationId) =>
        await _context.ContractDocumentTemplates
            .Include(x => x.ContractDocumentTemplateFields)
            .FirstOrDefaultAsync(x => x.ContractDocumentTemplateId == templateId && x.CorporationId == corporationId && x.Active);

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
        var document = client == null ? string.Empty : $"{client.DocumentType?.DocumentName} {client.Document}";

        return new Dictionary<string, string?>
        {
            [nameof(ContractDocumentFieldType.FullName)] = fullName,
            [nameof(ContractDocumentFieldType.Document)] = document,
            [nameof(ContractDocumentFieldType.Phone)] = contract.PhoneNumber,
            [nameof(ContractDocumentFieldType.Date)] = DateTime.UtcNow.ToString("MM/dd/yyyy")
        };
    }

    private static Dictionary<string, string?> BuildTestValues() =>
        new()
        {
            [nameof(ContractDocumentFieldType.FullName)] = "Cliente Prueba Spix",
            [nameof(ContractDocumentFieldType.Document)] = "ID 123456789",
            [nameof(ContractDocumentFieldType.Phone)] = "305-555-0100",
            [nameof(ContractDocumentFieldType.Date)] = DateTime.UtcNow.ToString("MM/dd/yyyy"),
            ["SignatureTest"] = "Firma Test"
        };

    private static IEnumerable<PdfSignatureField> BuildTestPdfFields(IEnumerable<PdfSignatureField> fields)
    {
        foreach (var field in fields)
        {
            yield return field;

            if (string.Equals(field.FieldName, nameof(ContractDocumentFieldType.Signature), StringComparison.OrdinalIgnoreCase))
            {
                yield return new PdfSignatureField
                {
                    FieldName = "SignatureTest",
                    PageNumber = field.PageNumber,
                    PositionX = field.PositionX,
                    PositionY = field.PositionY,
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

    private static ActionResponse<T> NotFound<T>() =>
        new() { WasSuccess = false, Message = "Recurso no encontrado." };
}
