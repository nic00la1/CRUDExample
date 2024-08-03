using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts;

/// <summary>
/// Represents business logic for manipulating Person entity
/// </summary>
public interface IPersonsUpdaterService
{
    /// <summary>
    /// Updates the specified person details based on given personId
    /// </summary>
    /// <param name="personUpdateRequest">Person details to update,
    /// including person id</param>
    /// <returns>Returns the person response object after updating</returns>
    Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);
}
