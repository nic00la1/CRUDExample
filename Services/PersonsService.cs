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
    private readonly List<Person> _persons;
    private readonly CountriesService _countriesService;
    private readonly PersonsServiceHelper _personsServiceHelper;

    public PersonsService()
    {
        _persons = new List<Person>();
        _countriesService = new CountriesService();
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
        throw new NotImplementedException();
    }
}
