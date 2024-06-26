using System.ComponentModel.DataAnnotations;
using Entities;
using Microsoft.VisualBasic;
using ServiceContracts;
using ServiceContracts.DTO;
using Services.Helpers;

namespace Services;

public class PersonsService : IPersonService
{
    private readonly List<Person> _persons;
    private readonly CountriesService _countriesService;

    public PersonsService()
    {
        _persons = new List<Person>();
        _countriesService = new CountriesService();
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
        _persons.Add(person);

        // Convert Person object into PersonResponse type
        return ConvertPersonToPersonResponse(person);
    }

    public List<PersonResponse> GetAllPersons()
    {
        return _persons.Select(p => p.ToPersonResponse()).ToList();
    }

    public PersonResponse? GetPersonById(Guid? personId)
    {
        if (personId == null) return null;

        Person? person = _persons.FirstOrDefault(p => p.Id == personId);

        if (person == null) return null;

        return person.ToPersonResponse();
    }

    public List<PersonResponse> GetFilteredPersons(
        string searchBy,
        string? searchString
    )
    {
        List<PersonResponse> allPersons = GetAllPersons();
        List<PersonResponse> matchingPersons = allPersons;

        if (string.IsNullOrEmpty(searchBy) ||
            string.IsNullOrEmpty(searchString)) return matchingPersons;

        switch (searchBy)
        {
            case nameof(Person.Name):
                matchingPersons =
                    allPersons.Where(temp =>
                        string.IsNullOrEmpty(temp.Name)
                            ? temp.Name.Contains(searchString,
                                StringComparison.OrdinalIgnoreCase)
                            : true).ToList();
                break;

            case nameof(Person.Email):
                matchingPersons =
                    allPersons.Where(temp =>
                        string.IsNullOrEmpty(temp.Email)
                            ? temp.Email.Contains(searchString,
                                StringComparison.OrdinalIgnoreCase)
                            : true).ToList();
                break;

            case nameof(Person.DateOfBirth):
                matchingPersons =
                    allPersons.Where(temp =>
                        temp.DateOfBirth != null
                            ? temp.DateOfBirth.Value.ToString("dd MMMM yyyy")
                                .Contains(searchString,
                                    StringComparison.OrdinalIgnoreCase)
                            : true).ToList();
                break;

            case nameof(Person.Gender):
                matchingPersons =
                    allPersons.Where(temp =>
                        !string.IsNullOrEmpty(temp.Gender)
                            ? temp.Gender.Contains(searchString,
                                StringComparison.OrdinalIgnoreCase)
                            : true).ToList();
                break;

            case nameof(Person.CountryID):
                matchingPersons =
                    allPersons.Where(temp =>
                        temp.CountryId != null
                            ? temp.CountryId.ToString().Contains(searchString,
                                StringComparison.OrdinalIgnoreCase)
                            : true).ToList();
                break;

            case nameof(Person.Address):
                matchingPersons = allPersons.Where(temp =>
                    !string.IsNullOrEmpty(temp.Address)
                        ? temp.Address.Contains(searchString,
                            StringComparison.OrdinalIgnoreCase)
                        : true).ToList();
                break;

            default:
                matchingPersons = allPersons;
                break;
        }

        return matchingPersons;
    }
}
