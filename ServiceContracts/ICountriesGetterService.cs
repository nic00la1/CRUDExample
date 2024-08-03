using ServiceContracts.DTO;
using Microsoft.AspNetCore.Http;

namespace ServiceContracts;

/// <summary>
/// Represents business logic for manipulating
/// Country entities
/// </summary>
public interface ICountriesGetterService
{
    /// <summary>
    /// Retrieves all countries from the list of countries
    /// </summary>
    /// <returns> All countries from the list as List of Country Response</returns>
    Task<List<CountryResponse>> GetAllCountries();

    /// <summary>
    /// Returns a country object based on the given country id
    /// </summary>
    /// <param name="countryId">CountryId (guid) to search</param>
    /// <returns>Matching country as CountryResponse object</returns>
    Task<CountryResponse?> GetCountryByCountryId(Guid? countryId);
}
