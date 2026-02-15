using Spix.DomainLogic.ModelUtility;

namespace Spix.AppBack.LoadCountries;

public interface IApiService
{
    Task<Response> GetListAsync<T>(string servicePrefix, string controller);
}