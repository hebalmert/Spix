using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.SettingModels;
using Spix.xFiles.FileHelper;

namespace Spix.AppService.ImplementContratos;

public class ContractIDPicService : IContractIDPicService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IMapperService _mapperService;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly ImgSetting _imgOption;
    private readonly IFileStorage _fileStorage;

    public ContractIDPicService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, IUserHelper userHelper, IMapperService mapperService,
        HttpErrorHandler httpErrorHandler, IOptions<ImgSetting> ImgOption, IFileStorage fileStorage)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _mapperService = mapperService;
        _httpErrorHandler = httpErrorHandler;
        _imgOption = ImgOption.Value;
        _fileStorage = fileStorage;
    }

    public async Task<ActionResponse<ContractIDPic>> GetAsync(Guid id)
    {
        try
        {
            var modelo = await _context.ContractIDPics
                .Include(x => x.ContractClient)
                .FirstOrDefaultAsync(x => x.ContractIDPicId == id);

            if (modelo == null)
            {
                return new ActionResponse<ContractIDPic>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }
            if (!string.IsNullOrWhiteSpace(modelo.PhotoIDFront))
            {
                var FileResult = await _fileStorage.GetBlobSasUrlAsync(modelo.PhotoIDFront, _imgOption.ImgContractIDPic, TimeSpan.FromMinutes(2));
                modelo.ImageFrontFullPath = FileResult;
            }
            else
            {
                modelo.ImageFrontFullPath = _imgOption.ImgNoImage;
            }

            if (!string.IsNullOrWhiteSpace(modelo.PhotoIDBack))
            {
                var FileResult = await _fileStorage.GetBlobSasUrlAsync(modelo.PhotoIDBack, _imgOption.ImgContractIDPic, TimeSpan.FromMinutes(2));
                modelo.ImageBackFullPath = FileResult;
            }
            else
            {
                modelo.ImageBackFullPath = _imgOption.ImgNoImage;
            }

            return new ActionResponse<ContractIDPic>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractIDPic>(ex);
        }
    }

    public async Task<ActionResponse<ContractIDPic>> UpdateAsync(ContractIDPic modelo)
    {
        await _transactionManager.BeginTransactionAsync();

        try
        {
            //Implementando el Mapeo de Modelos con Mapster
            ContractIDPic NuevoModelo = _mapperService.Map<ContractIDPic, ContractIDPic>(modelo);
            if (!string.IsNullOrEmpty(modelo.ImgFrontBase64))
            {
                string guid = modelo.PhotoIDFront == null ? Guid.NewGuid().ToString() + ".jpg" : modelo.PhotoIDFront;
                var imageId = Convert.FromBase64String(modelo.ImgFrontBase64);
                NuevoModelo.PhotoIDFront = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgContractIDPic);
            }

            if (!string.IsNullOrEmpty(modelo.ImgBackBase64))
            {
                string guid = modelo.PhotoIDBack == null ? Guid.NewGuid().ToString() + ".jpg" : modelo.PhotoIDBack;
                var imageId = Convert.FromBase64String(modelo.ImgBackBase64);
                NuevoModelo.PhotoIDBack = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgContractIDPic);
            }

            _context.ContractIDPics.Update(NuevoModelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractIDPic>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractIDPic>(ex);
        }
    }

    public async Task<ActionResponse<ContractIDPic>> AddAsync(ContractIDPic modelo, string username)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<ContractIDPic>
                {
                    WasSuccess = false,
                    Message = "Problemas de Validacion de Usuario"
                };
            }

            modelo.CorporationId = Convert.ToInt32(user.CorporationId);
            modelo.DateCreado = DateTime.Now;
            //control de Auditoria
            modelo.UsuarioOwner = $"{user.FirstName!} {user.LastName!}";
            modelo.UserId = Guid.Parse(user.Id);

            if (!string.IsNullOrEmpty(modelo.ImgFrontBase64))
            {
                string guid = Guid.NewGuid().ToString() + ".jpg";
                var imageId = Convert.FromBase64String(modelo.ImgFrontBase64);
                modelo.PhotoIDFront = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgContractIDPic);
            }

            if (!string.IsNullOrEmpty(modelo.ImgBackBase64))
            {
                string guid = Guid.NewGuid().ToString() + ".jpg";
                var imageId = Convert.FromBase64String(modelo.ImgBackBase64);
                modelo.PhotoIDBack = await _fileStorage.SaveImageAsync(imageId, guid, _imgOption.ImgContractIDPic);
            }

            _context.ContractIDPics.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractIDPic>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractIDPic>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.ContractIDPics.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Problemas para Enconstrar el Registro Indicado"
                };
            }
            _context.ContractIDPics.Remove(DataRemove);

            if (DataRemove.PhotoIDFront is not null)
            {
                var response = _fileStorage.DeleteImage(_imgOption.ImgContractIDPic!, DataRemove.PhotoIDFront);
                if (!response)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "Se Elimino el Registro pero Sin la Imagen"
                    };
                }
            }

            if (DataRemove.PhotoIDBack is not null)
            {
                var response = _fileStorage.DeleteImage(_imgOption.ImgContractIDPic!, DataRemove.PhotoIDBack);
                if (!response)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "Se Elimino el Registro pero Sin la Imagen"
                    };
                }
            }

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
}
