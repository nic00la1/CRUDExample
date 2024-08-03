using ServiceContracts.DTO;
using Microsoft.AspNetCore.Http;

namespace ServiceContracts;

/// <summary>
/// Represents business logic for manipulating
/// Country entities
/// </summary>
public interface ICountriesExcelService
{
    /// <summary>
    /// Uploads countries from an excel file into the database
    /// </summary>
    /// <param name="formFile">Excel file with list of countries</param>
    /// <returns>Returns number of countries added</returns>
    Task<int> UploadCountriesFromExcelFile(IFormFile formFile);
}
