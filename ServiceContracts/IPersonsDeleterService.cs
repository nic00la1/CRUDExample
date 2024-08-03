using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts;

/// <summary>
/// Represents business logic for manipulating Person entity
/// </summary>
public interface IPersonsDeleterService
{
    /// <summary>
    /// Deletes a person based on the given personId
    /// </summary>
    /// <param name="PersonId">PersonId to delete</param>
    /// <returns>Returns true, if the deletion is successful;
    /// otherwise false</returns>
    Task<bool> DeletePerson(Guid? PersonId);
}
