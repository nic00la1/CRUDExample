using System.ComponentModel.DataAnnotations;
using Entities;
using Microsoft.VisualBasic;
using ServiceContracts;
using ServiceContracts.DTO;
using Services.Helpers;
using System.Reflection;
using ServiceContracts.Enums;

namespace Services;

public class PersonsService : IPersonService
{
    private readonly PersonsDbContext _db;
    private readonly ICountriesService _countriesService;
    private readonly PersonsServiceHelper _personsServiceHelper;

    public PersonsService(PersonsDbContext personsDbContext,
                          ICountriesService countriesService
    )
    {
        _db = personsDbContext;
        _countriesService = countriesService;
        _personsServiceHelper = new PersonsServiceHelper();
    }

    private PersonResponse ConvertPersonToPersonResponse(Person person)
    {
        PersonResponse personResponse = person.ToPersonResponse();

        personResponse.Country =
            _countriesService.GetCountryByCountryId(person.CountryID)
                ?.CountryName;

        return personResponse;
    }


    public PersonResponse AddPerson(PersonAddRequest? personAddRequest)
    {
        // Check if PersonAddRequest is not null
        if (personAddRequest == null)
            throw new ArgumentNullException(nameof(personAddRequest));

        // Model Validation
        ValidationHelper.ModelValidation(personAddRequest);

        // Convert personAddRequest into Person type
        Person person = personAddRequest.ToPerson();

        // Generate a new PersonID
        person.Id = Guid.NewGuid();

        // Add Person object to persons list
        _db.Persons.Add(person);
        _db.SaveChanges();

        // Convert Person object into PersonResponse type
        return ConvertPersonToPersonResponse(person);
    }

    public List<PersonResponse> GetAllPersons()
    {
        // SELECT * FROM Persons
/*        return _db.Persons.ToList()
            .Select(p => ConvertPersonToPersonResponse(p))
            .ToList();
*/

        return _db.sp_GetAllPersons()
            .Select(temp => ConvertPersonToPersonResponse(temp)).ToList();
    }

    public PersonResponse? GetPersonById(Guid? personId)
    {
        if (personId == null) return null;

        Person? person = _db.Persons.FirstOrDefault(p => p.Id == personId);

        if (person == null) return null;

        return ConvertPersonToPersonResponse(person);
    }

    public List<PersonResponse> GetFilteredPersons(
        string searchBy,
        string? searchString
    )
    {
        List<PersonResponse> allPersons = GetAllPersons();
        if (string.IsNullOrEmpty(searchBy) ||
            string.IsNullOrEmpty(searchString)) return allPersons;

        List<PersonResponse> filteredPersons = allPersons.Where(person =>
        {
            // Use reflection to get the property by name
            PropertyInfo? propertyInfo =
                typeof(PersonResponse).GetProperty(searchBy,
                    BindingFlags.IgnoreCase | BindingFlags.Public |
                    BindingFlags.Instance);
            if (propertyInfo != null)
            {
                // Get the value of the property
                object? value = propertyInfo.GetValue(person);
                if (value != null)
                {
                    // Special handling for DateOfBirth as it's a DateTime? and needs to be converted to string
                    if (propertyInfo.PropertyType == typeof(DateTime?))
                    {
                        DateTime? date = (DateTime?)value;
                        return date?.ToString("dd MMMM yyyy")
                            .Contains(searchString,
                                StringComparison.OrdinalIgnoreCase) ?? false;
                    }

                    // Convert the value to string and perform the comparison
                    return value.ToString()?.Contains(searchString,
                        StringComparison.OrdinalIgnoreCase) ?? false;
                }
            }

            return false;
        }).ToList();

        return filteredPersons;
    }

    public List<PersonResponse> GetSortedPersons(
        List<PersonResponse> allPersons,
        string sortBy,
        SortOderOptions sortOrder
    )
    {
        if (string.IsNullOrEmpty(sortBy)) return allPersons;

        // Use the SortByProperty method from PersonsServiceHelper
        return _personsServiceHelper.SortByProperty(allPersons, sortBy,
            sortOrder);
    }

    public PersonResponse UpdatePerson(PersonUpdateRequest? personUpdateRequest)
    {
        if (personUpdateRequest == null)
            throw new ArgumentNullException(nameof(Person));

        // Validation
        ValidationHelper.ModelValidation(personUpdateRequest);

        // Get matching person object to update
        Person? matchingPerson = _db.Persons.FirstOrDefault(p =>
            p.Id == personUpdateRequest.Id);

        if (matchingPerson == null)
            throw new ArgumentException("Given person id doesn't exist!");

        // Update all details
        matchingPerson.PersonName = personUpdateRequest.PersonName;
        matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
        matchingPerson.Email = personUpdateRequest.Email;
        matchingPerson.CountryID = personUpdateRequest.CountryID;
        matchingPerson.Gender = personUpdateRequest.Gender.ToString();
        matchingPerson.Address = personUpdateRequest.Address;
        matchingPerson.ReceiveNewsLetters =
            personUpdateRequest.ReceiveNewsLetters;

        _db.SaveChanges(); // UPDATE
        return ConvertPersonToPersonResponse(matchingPerson);
    }

    public bool DeletePerson(Guid? PersonId)
    {
        if (PersonId == null) throw new ArgumentNullException(nameof(PersonId));

        Person? person =
            _db.Persons.FirstOrDefault(temp => temp.Id == PersonId);

        if (person == null) return false;

        _db.Persons.Remove(_db.Persons.First(temp => temp.Id == PersonId));
        _db.SaveChanges(); // DELETE

        return true;
    }
}
