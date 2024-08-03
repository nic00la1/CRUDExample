using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts;

/// <summary>
/// Represents business logic for manipulating Person entity
/// </summary>
public interface IPersonsGetterService
{
    /// <summary>
    /// Returns all person
    /// </summary>
    /// <returns>Returns a list of objects of PersonResponse type</returns>
    Task<List<PersonResponse>> GetAllPersons();

    /// <summary>
    /// Returns the person object based on the given personId
    /// </summary>
    /// <param name="personId">Person id to search</param>
    /// <returns>Returns matching person object</returns>
    Task<PersonResponse?> GetPersonById(Guid? personId);

    /// <summary>
    /// Returns all person objects that matches
    /// with the given search field and search string
    /// </summary>
    /// <param name="searchBy">Search field to search</param>
    /// <param name="searchString">Search string to search</param>
    /// <returns>Returns all matching persons based on the given
    ///  search field and search string</returns>
    Task<List<PersonResponse>> GetFilteredPersons(string searchBy,
                                                  string? searchString
    );

    /// <summary>
    /// Return persons as CSV file
    /// </summary>
    /// <returns>Return the memory stream with CSV data of persons</returns>
    Task<MemoryStream> GetPersonCSV();

    /// <summary>
    /// Return persons as Excel file
    /// </summary>
    /// <returns>Returns the memory stream with Excel data of persons</returns>
    Task<MemoryStream> GetPersonExcel();
}
