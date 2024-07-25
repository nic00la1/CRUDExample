﻿using ServiceContracts.DTO;
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
    Task<PersonResponse> AddPerson(PersonAddRequest personAddRequest);

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
    /// Returns sorted list of persons
    /// </summary>
    /// <param name="allPersons">Represents lift of persons to sort</param>
    /// <param name="sortBy">Name of the property (key), based on
    /// which the persons should be sorted</param>
    /// <param name="sortOrder">ASC or DESC</param>
    /// <returns>Returns sorted persons as PersonResponse list</returns>
    Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons,
                                                string sortBy,
                                                SortOderOptions sortOrder
    );

    /// <summary>
    /// Updates the specified person details based on given personId
    /// </summary>
    /// <param name="personUpdateRequest">Person details to update,
    /// including person id</param>
    /// <returns>Returns the person response object after updating</returns>
    Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);

    /// <summary>
    /// Deletes a person based on the given personId
    /// </summary>
    /// <param name="PersonId">PersonId to delete</param>
    /// <returns>Returns true, if the deletion is successful;
    /// otherwise false</returns>
    Task<bool> DeletePerson(Guid? PersonId);

    /// <summary>
    /// Return persons as CSV file
    /// </summary>
    /// <returns>Return the memory stream with CSV</returns>
    Task<MemoryStream> GetPersonCSV();
}
