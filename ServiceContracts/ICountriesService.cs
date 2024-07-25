using ServiceContracts.DTO;
using Microsoft.AspNetCore.Http;

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
    Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);

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

    /// <summary>
    /// Uploads countries from an excel file into the database
    /// </summary>
    /// <param name="formFile">Excel file with list of countries</param>
    /// <returns>Returns number of countries added</returns>
    Task<int> UploadCountriesFromExcelFile(IFormFile formFile);
}
