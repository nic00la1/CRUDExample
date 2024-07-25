using System.ComponentModel.DataAnnotations;
using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO;

/// <summary>
/// Represents a request to update a person
/// </summary>
public class PersonUpdateRequest
{
    [Required(ErrorMessage = "Person ID can't be blank")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Person PersonName can't be blank")]
    public string? PersonName { get; set; }

    [Required(ErrorMessage = "Email can't be blank")]
    [EmailAddress(ErrorMessage = "Email value should be a valid email")]
    public string? Email { get; set; }

    [DataType(DataType.Date)] public DateTime? DateOfBirth { get; set; }
    public GenderOptions? Gender { get; set; }
    public Guid? CountryID { get; set; }
    public string? Address { get; set; }
    public bool ReceiveNewsLetters { get; set; }

    /// <summary>
    /// Converts the current object of PersonAddRequest
    /// into a new object of Person type
    /// </summary>
    /// <returns>Returns Person object</returns>
    public Person ToPerson()
    {
        return new Person()
        {
            Id = Id,
            PersonName = PersonName,
            Email = Email,
            DateOfBirth = DateOfBirth,
            Gender = Gender.ToString(),
            Address = Address,
            CountryId = CountryID,
            ReceiveNewsLetters = ReceiveNewsLetters
        };
    }
}
