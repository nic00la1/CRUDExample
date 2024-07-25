using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO;

/// <summary>
/// Represents DTO class that is used as return
/// type of most methods of the PersonService
/// </summary>
public class PersonResponse
{
    public Guid ID { get; set; }
    public string? PersonName { get; set; }
    public string? Email { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public Guid? CountryId { get; set; }
    public string? Country { get; set; }
    public string? Address { get; set; }
    public bool ReceiveNewsLetters { get; set; }
    public double? Age { get; set; }

    /// <summary>
    /// Compares the current object data with the parameter object
    /// </summary>
    /// <param name="obj">The PersonResponse Object to compare</param>
    /// <returns>True or False, indicating whether all person details
    /// are matched with the specified parameter object</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;

        if (obj.GetType() != typeof(PersonResponse)) return false;

        PersonResponse person = (PersonResponse)obj;

        return ID == person.ID && PersonName == person.PersonName && Email ==
            person.Email && DateOfBirth == person.DateOfBirth &&
            Gender == person.Gender && CountryId == person.CountryId &&
            Address == person.Address &&
            ReceiveNewsLetters == person.ReceiveNewsLetters;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return
            $"Person Id: {ID}, Person PersonName: {PersonName}, Email: {Email}," +
            $"Date of Birth: {DateOfBirth?.ToString("dd MMM yyyy")}," +
            $"Gender: {Gender}, Country Id: {CountryId}, Address: {Address}," +
            $"Receive News Letters: {ReceiveNewsLetters}";
    }

    public PersonUpdateRequest ToPersonUpdateRequest()
    {
        return new PersonUpdateRequest()
        {
            Id = ID,
            PersonName = PersonName,
            Email = Email,
            DateOfBirth = DateOfBirth,
            Gender =
                (GenderOptions)Enum.Parse(typeof(GenderOptions), Gender, true),
            CountryID = CountryId,
            Address = Address,
            ReceiveNewsLetters = ReceiveNewsLetters
        };
    }
}

public static class PersonExtensions
{
    /// <summary>
    /// An extension method to convert an object of Person class into
    /// PersonResponse class
    /// </summary>
    /// <param name="person">The Person object to convert</param>
    /// <returns>Returns the converted PersonResponse object</returns>
    public static PersonResponse ToPersonResponse(this Person person)
    {
        return new PersonResponse()
        {
            ID = person.Id,
            PersonName = person.PersonName,
            Email = person.Email,
            DateOfBirth = person.DateOfBirth,
            Gender = person.Gender,
            Age = person.DateOfBirth != null
                ? Math.Round((DateTime.Now - person.DateOfBirth.Value)
                    .TotalDays / 365.25)
                : null,
            Address = person.Address,
            ReceiveNewsLetters = person.ReceiveNewsLetters,
            CountryId =
                person.CountryId, // Assuming Person has a CountryId property
            Country = person.Country?.CountryName
        };
    }
}
