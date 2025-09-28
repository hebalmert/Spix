using Spix.DomainLogic.SpixResponse;

namespace Spix.AppBack.LoadCountries;

public interface IApiService
{
    Task<Response> GetListAsync<T>(string servicePrefix, string controller);
}