﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.Validations;
using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.Domain.Resources;
using Spix.DomainLogic.Pagination;
using Spix.DomainLogic.SpixResponse;
using Spix.Services.InterfacesEntitiesData;

namespace Spix.Services.ImplementEntitiesData;

public class ChainTypesService : IChainTypesService
{
    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITransactionManager _transactionManager;
    private readonly HttpErrorHandler _httpErrorHandler;
    private readonly IStringLocalizer _localizer;

    public ChainTypesService(DataContext context, IHttpContextAccessor httpContextAccessor,
        ITransactionManager transactionManager, HttpErrorHandler httpErrorHandler, IStringLocalizer localizer)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _transactionManager = transactionManager;
        _httpErrorHandler = httpErrorHandler;
        _localizer = localizer;
    }

    public async Task<ActionResponse<IEnumerable<IntItemModel>>> ComboAsync()
    {
        try
        {
            List<IntItemModel> ListModel = await _context.ChainTypes.Where(x => x.Active)
                .Select(x => new IntItemModel { Name = x.ChainName, Value = x.ChainTypeId }).ToListAsync();
            // Insertar el elemento neutro al inicio
            var defaultItem = new IntItemModel
            {
                Value = 0,
                Name = _localizer[nameof(Resource.Select_B_FrecuencyType)]
            };
            ListModel.Insert(0, defaultItem);

            return new ActionResponse<IEnumerable<IntItemModel>>
            {
                WasSuccess = true,
                Result = ListModel
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<IEnumerable<ChainType>>> GetAsync(PaginationDTO pagination)
    {
        try
        {
            var queryable = _context.ChainTypes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.ChainName!.ToLower().Contains(pagination.Filter.ToLower()));
            }

            await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);
            var modelo = await queryable.OrderBy(x => x.ChainName).Paginate(pagination).ToListAsync();

            return new ActionResponse<IEnumerable<ChainType>>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ChainType>>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<ChainType>> GetAsync(int id)
    {
        try
        {
            var modelo = await _context.ChainTypes.FindAsync(id);
            if (modelo == null)
            {
                return new ActionResponse<ChainType>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_RegisterNotFound)]
                };
            }

            return new ActionResponse<ChainType>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ChainType>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<ChainType>> UpdateAsync(ChainType modelo)
    {
        if (modelo == null || modelo.ChainTypeId <= 0)
        {
            return new ActionResponse<ChainType>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }
        await _transactionManager.BeginTransactionAsync();

        try
        {
            _context.ChainTypes.Update(modelo);

            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ChainType>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ChainType>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<ChainType>> AddAsync(ChainType modelo)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<ChainType>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }
        await _transactionManager.BeginTransactionAsync();
        try
        {
            _context.ChainTypes.Add(modelo);
            await _transactionManager.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ChainType>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ChainType>(ex); // ✅ Manejo de errores automático
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(int id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var DataRemove = await _context.ChainTypes.FindAsync(id);
            if (DataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.ChainTypes.Remove(DataRemove);

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
            return await _httpErrorHandler.HandleErrorAsync<bool>(ex); // ✅ Manejo de errores automático
        }
    }
}