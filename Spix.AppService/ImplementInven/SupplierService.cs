using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfacesInven;
using Spix.Domain.EntitiesInven;
using Spix.DomainLogic.EntitiesInvenDTO;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;
using Spix.xLanguage.Resources;

namespace Spix.Services.ImplementInven;

//Los proveedores de la corporacion. Toda consulta va filtrada por la corporacion del usuario,
//y un proveedor con compras no se borra: se inactiva.
public class SupplierService : ISupplierService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IFileStorage _fileStorage;
    private readonly IUserHelper _userHelper;
    private readonly ImgSetting _imgOption;
    private readonly IStringLocalizer _localizer;

    public SupplierService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager,
        IFileStorage fileStorage,
        HttpErrorHandler httpErrorHandler,
        IUserHelper userHelper,
        IOptions<ImgSetting> imgOption,
        IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _fileStorage = fileStorage;
        _httpErrorHandler = httpErrorHandler;
        _userHelper = userHelper;
        _imgOption = imgOption.Value;
        _localizer = localizer;
    }

    //Proveedores activos, con el neutro traducido en la posicion 0
    public async Task<ActionResponse<IEnumerable<Supplier>>> ComboAsync(string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<Supplier>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var list = await _context.Suppliers
                .AsNoTracking()
                .Where(x => x.Active && x.CorporationId == corporationId)
                .OrderBy(x => x.Name)
                .ToListAsync();

            list.Insert(0, new Supplier
            {
                SupplierId = Guid.Empty,
                Name = _localizer["Supplier_Select"]
            });

            return Success<IEnumerable<Supplier>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<Supplier>>(ex);
        }
    }

    //El listado: cada proveedor con cuantas compras tiene y la ultima
    public async Task<ActionResponse<IEnumerable<SupplierListItemDto>>> GetAsync(PaginationDTO pagination, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<IEnumerable<SupplierListItemDto>>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var queryable = _context.Suppliers
                .AsNoTracking()
                .Where(x => x.CorporationId == corporationId);

            //Se busca por nombre, documento o correo
            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                var filter = pagination.Filter.Trim();
                queryable = queryable.Where(x =>
                    EF.Functions.Like(x.Name, $"%{filter}%") ||
                    EF.Functions.Like(x.Document, $"%{filter}%") ||
                    EF.Functions.Like(x.Email, $"%{filter}%"));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

            var list = await queryable
                .OrderBy(x => x.Name)
                .Paginate(pagination)
                .Select(x => new SupplierListItemDto
                {
                    SupplierId = x.SupplierId,
                    Name = x.Name,
                    Document = x.Document,
                    PhoneNumber = x.PhoneNumber,
                    Email = x.Email,
                    Active = x.Active,
                    Photo = x.Photo,
                    Purchases = x.Purchases!.Count(),
                    LastPurchase = x.Purchases!.Max(p => (DateTime?)p.PurchaseDate)
                })
                .ToListAsync();

            //La foto vive en un contenedor privado: se entrega con un enlace temporal
            foreach (var item in list.Where(x => !string.IsNullOrWhiteSpace(x.Photo)))
            {
                item.ImageFullPath = await _fileStorage.GetBlobSasUrlAsync(item.Photo!, _imgOption.ImgSuppliers, TimeSpan.FromMinutes(3));
            }

            return Success<IEnumerable<SupplierListItemDto>>(list);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<SupplierListItemDto>>(ex);
        }
    }

    public async Task<ActionResponse<Supplier>> GetAsync(Guid id, string username)
    {
        try
        {
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return Fail<Supplier>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var modelo = await _context.Suppliers
                .AsNoTracking()
                .Include(x => x.State)
                .ThenInclude(x => x!.Cities)
                .Include(x => x.DocumentType)
                .FirstOrDefaultAsync(x => x.SupplierId == id && x.CorporationId == corporationId);
            if (modelo == null) return Fail<Supplier>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            modelo.ImageFullPath = string.IsNullOrWhiteSpace(modelo.Photo)
                ? _imgOption.ImgNoImage
                : await _fileStorage.GetBlobSasUrlAsync(modelo.Photo, _imgOption.ImgSuppliers, TimeSpan.FromMinutes(2));

            return Success(modelo);
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<Supplier>(ex);
        }
    }

    public async Task<ActionResponse<Supplier>> AddAsync(Supplier modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<Supplier>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var error = await ValidateUniqueAsync(modelo, corporationId.Value, null);
            if (error != null) return await FailRollbackAsync<Supplier>(error);

            //Lo que decide el servidor
            var nuevo = new Supplier { CorporationId = corporationId.Value };
            CopyFields(modelo, nuevo);

            if (!string.IsNullOrEmpty(modelo.ImgBase64))
            {
                var imagen = Convert.FromBase64String(modelo.ImgBase64);
                nuevo.Photo = await _fileStorage.SaveImageAsync(imagen, Guid.NewGuid() + ".jpg", _imgOption.ImgSuppliers);
            }

            //Persistencia
            _context.Suppliers.Add(nuevo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(nuevo);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Supplier>(ex);
        }
    }

    public async Task<ActionResponse<Supplier>> UpdateAsync(Supplier modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<Supplier>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.Suppliers.FirstOrDefaultAsync(x =>
                x.SupplierId == modelo.SupplierId &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<Supplier>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var error = await ValidateUniqueAsync(modelo, corporationId.Value, current.SupplierId);
            if (error != null) return await FailRollbackAsync<Supplier>(error);

            //Mapeo campo por campo
            CopyFields(modelo, current);

            //Foto nueva: reusa el nombre del archivo si ya tenia uno
            if (!string.IsNullOrEmpty(modelo.ImgBase64))
            {
                var imagen = Convert.FromBase64String(modelo.ImgBase64);
                current.Photo = await _fileStorage.SaveImageAsync(imagen, current.Photo ?? Guid.NewGuid() + ".jpg", _imgOption.ImgSuppliers);
            }

            //Persistencia
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return Success(current);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<Supplier>(ex);
        }
    }

    //Solo se borra un proveedor sin compras; la foto se quita despues de confirmar el borrado
    public async Task<ActionResponse<bool>> DeleteAsync(Guid id, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            //Validacion
            var corporationId = await GetCorporationIdAsync(username);
            if (corporationId == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_AuthIdFail)]);

            var current = await _context.Suppliers.FirstOrDefaultAsync(x =>
                x.SupplierId == id &&
                x.CorporationId == corporationId);
            if (current == null) return await FailRollbackAsync<bool>(_localizer[nameof(Resource.Generic_IdNotFound)]);

            var inUse = await _context.Purchases.AnyAsync(x => x.SupplierId == id);
            if (inUse) return await FailRollbackAsync<bool>(_localizer["Supplier_InUse", current.Name]);

            //Persistencia
            var photo = current.Photo;
            _context.Suppliers.Remove(current);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            //Si la foto no se pudo quitar, el registro ya quedo borrado: no se revierte por eso
            if (!string.IsNullOrWhiteSpace(photo))
            {
                await _fileStorage.RemoveFileAsync(_imgOption.ImgSuppliers!, photo);
            }

            return Success(true);
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
        }
    }

    //Los datos que el usuario puede cambiar
    private static void CopyFields(Supplier from, Supplier to)
    {
        to.Name = from.Name.Trim();
        to.DocumentTypeId = from.DocumentTypeId;
        to.Document = from.Document.Trim();
        to.PhoneNumber = from.PhoneNumber;
        to.Address = from.Address;
        to.Email = from.Email.Trim();
        to.ContactName = from.ContactName;
        to.StateId = from.StateId;
        to.CityId = from.CityId;
        to.Active = from.Active;
    }

    //Nombre y documento no se repiten dentro de la corporacion
    private async Task<string?> ValidateUniqueAsync(Supplier modelo, int corporationId, Guid? supplierId)
    {
        var name = modelo.Name.Trim();
        var nameRepeated = await _context.Suppliers.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.Name == name &&
            x.SupplierId != supplierId);
        if (nameRepeated) return _localizer["Supplier_NameRepeated", name];

        var document = modelo.Document.Trim();
        var documentRepeated = await _context.Suppliers.AnyAsync(x =>
            x.CorporationId == corporationId &&
            x.DocumentTypeId == modelo.DocumentTypeId &&
            x.Document == document &&
            x.SupplierId != supplierId);
        if (documentRepeated) return _localizer["Supplier_DocumentRepeated", document];

        return null;
    }

    private async Task<int?> GetCorporationIdAsync(string username)
    {
        var user = await _userHelper.GetUserByUserNameAsync(username);
        return user?.CorporationId;
    }

    private async Task<ActionResponse<T>> FailRollbackAsync<T>(string message)
    {
        await _transactionManager.RollbackTransactionAsync();
        return Fail<T>(message);
    }

    private static ActionResponse<T> Success<T>(T result) => new() { WasSuccess = true, Result = result };

    private static ActionResponse<T> Fail<T>(string message) => new() { WasSuccess = false, Message = message };
}
