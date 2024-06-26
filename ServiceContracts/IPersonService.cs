using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts;

/// <summary>
/// Represents business logic for manipulating Person entity
/// </summary>
public interface IPersonService
{
    /// <summary>
    /// Adds a new person into the list of persons 
    /// </summary>
    /// <param name="personAddRequest">Person to add</param>
    /// <returns>Returns the same person details, along with
    /// newly generated PersonID</returns>
    PersonResponse AddPerson(PersonAddRequest personAddRequest);

    /// <summary>
    /// Returns all person
    /// </summary>
    /// <returns>Returns a list of objects of PersonResponse type</returns>
    List<PersonResponse> GetAllPersons();

    /// <summary>
    /// Returns the person object based on the given personId
    /// </summary>
    /// <param name="personId">Person id to search</param>
    /// <returns>Returns matching person object</returns>
    PersonResponse? GetPersonById(Guid? personId);

    /// <summary>
    /// Returns all person objects that matches
    /// with the given search field and search string
    /// </summary>
    /// <param name="searchBy">Search field to search</param>
    /// <param name="searchString">Search string to search</param>
    /// <returns>Returns all matching persons based on the given
    ///  search field and search string</returns>
    List<PersonResponse> GetFilteredPersons(string searchBy,
                                            string? searchString
    );

    /// <summary>
    /// Returns sorted list of persons
    /// </summary>
    /// <param name="allPersons">Represents lift of persons to sort</param>
    /// <param name="sortBy">Name of the property (key), based on
    /// which the persons should be sorted</param>
    /// <param name="sortOder">ASC or DESC</param>
    /// <returns>Returns sorted persons as PersonResponse list</returns>
    List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons,
                                          string sortBy,
                                          SortOderOptions sortOder
    );
}
