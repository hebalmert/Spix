using Newtonsoft.Json;

namespace Spix.AppBack.LoadCountries;

public class CountryResponse
{
    [JsonProperty("id")]
    public long CountryId { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("iso2")]
    public string? Iso2 { get; set; }
}