using ServiceContracts.DTO;

namespace ServiceContracts;

/// <summary>
/// Represents business logic for manipulating
/// Country entities
/// </summary>
public interface ICountriesService
{
    /// <summary>
    /// Adds a new country object to the list of countries
    /// </summary>
    /// <param name="countryAddRequest">Country object to add</param>
    /// <returns>Returns the country object after adding it
    /// (including newly generated country id)</returns>
    CountryResponse AddCountry(CountryAddRequest? countryAddRequest);
}
