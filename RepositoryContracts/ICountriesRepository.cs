using Entities;

namespace RepositoryContracts;

/// <summary>
/// Represents data access logic for managing Persons entities.
/// </summary>
public interface ICountriesRepository
{
    /// <summary>
    /// Adds a new country object to the data store.
    /// </summary>
    /// <param name="country">Country object to add</param>
    /// <returns>Returns the country object after adding it to the data store</returns>
    Task<Country> AddCountry(Country country);

    /// <summary>
    /// Returns a list of all countries in the data store.
    /// </summary>
    /// <returns>All countries from the table</returns>
    Task<List<Country>> GetAllCountries();

    /// <summary>
    /// Returns a country object based on the given country id;
    /// otherwise, it returns null.
    /// </summary>
    /// <param name="countryId">CountryId to search</param>
    /// <returns>Matching a country or null</returns>
    Task<Country?> GetCountryByCountryId(Guid countryId);

    /// <summary>
    /// Returns a country object based on the given country name;
    /// </summary>
    /// <param name="countryName">Country name to search</param>
    /// <returns>Matching country or null</returns>
    Task<Country?> GetCountryByCountryName(string countryName);
}
