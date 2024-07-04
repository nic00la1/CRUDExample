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

    public PersonsService(bool initialize = true)
    {
        _persons = new List<Person>();
        _countriesService = new CountriesService();
        _personsServiceHelper = new PersonsServiceHelper();

        if (initialize)
            // {C737E32D-A924-447D-BB13-50331D35579A}
            // {B8BC36E0-48FF-49EB-BC7E-BF9F28BB7C70}
            // {D3D3D3D3-D3D3-D3D3-D3D3-D3D3D3D3D3D3}
            // {E5E5E5E5-E5E5-E5E5-E5E5-E5E5E5E5E5E5}
            // {F5F5F5F5-F5F5-F5F5-F5F5-F5F5F5F5F5F5}
            _persons.AddRange(new List<Person>()
            {
                new()
                {
                    Id = Guid.Parse("C737E32D-A924-447D-BB13-50331D35579A"),
                    Name = "Joletta",
                    Email = "jduchart0@indiegogo.com",
                    DateOfBirth = new DateTime(1994, 3, 15),
                    Gender = "Female",
                    CountryID =
                        Guid.Parse("F8B7F2A4-D571-44D9-A9C5-71F8165CB22C"),
                    Address = "27 Brentwood Junction",
                    ReceiveNewsLetters = false
                },
                new()
                {
                    Id = Guid.Parse("B8BC36E0-48FF-49EB-BC7E-BF9F28BB7C70"),
                    Name = "Dex",
                    Email = "dashton1@fotki.com",
                    DateOfBirth = new DateTime(2005, 12, 2),
                    Gender = "Male",
                    CountryID =
                        Guid.Parse("C35C19BD-01DD-4F3D-912C-F4C962646F7E"),
                    Address = "6 Artisan Hill",
                    ReceiveNewsLetters = false
                },
                new()
                {
                    Id = Guid.Parse("D3D3D3D3-D3D3-D3D3-D3D3-D3D3D3D3D3D3"),
                    Name = "Kylila",
                    Email = "kcosans2@geocities.com",
                    DateOfBirth = new DateTime(2005, 3, 6),
                    Gender = "Female",
                    CountryID =
                        Guid.Parse("6340C2AD-A46F-4953-A318-A76CCEB19B1B"),
                    Address = "6044 Buhler Park",
                    ReceiveNewsLetters = false
                },
                new()
                {
                    Id = Guid.Parse("E5E5E5E5-E5E5-E5E5-E5E5-E5E5E5E5E5E5"),
                    Name = "Jocelin",
                    Email = "jspohr3@paypal.com",
                    DateOfBirth = new DateTime(1993, 8, 30),
                    Gender = "Female",
                    CountryID =
                        Guid.Parse("6CA1838D-346B-447A-A8C3-901A1B9147C6"),
                    Address = "2475 Roth Street",
                    ReceiveNewsLetters = true
                },
                new()
                {
                    Id = Guid.Parse("F5F5F5F5-F5F5-F5F5-F5F5-F5F5F5F5F5F5"),
                    Name = "Andria",
                    Email = "acisar4@amazon.de",
                    DateOfBirth = new DateTime(1990, 12, 3),
                    Gender = "Female",
                    CountryID = Guid.Parse("<CountryID>"),
                    Address = "8 Lerdahl Way",
                    ReceiveNewsLetters = false
                },
                new()
                {
                    Name = "Noella",
                    Email = "nfretwell5@guardian.co.uk",
                    DateOfBirth = new DateTime(1998, 1, 8),
                    Gender = "Female",
                    Address = "88 Waywood Park",
                    ReceiveNewsLetters = true
                },

                new()
                {
                    Name = "Jody",
                    Email = "jbulleyn6@cnbc.com",
                    DateOfBirth = new DateTime(2000, 1, 7),
                    Gender = "Female",
                    Address = "91167 Commercial Park",
                    ReceiveNewsLetters = true
                },

                new()
                {
                    Name = "Berte",
                    Email = "bcarletti7@ehow.com",
                    DateOfBirth = new DateTime(1999, 8, 25),
                    Gender = "Female",
                    Address = "34682 Dahle Parkway",
                    ReceiveNewsLetters = false
                },

                new()
                {
                    Name = "Palmer",
                    Email = "paish8@noaa.gov",
                    DateOfBirth = new DateTime(1996, 8, 13),
                    Gender = "Male",
                    Address = "5 Oriole Terrace",
                    ReceiveNewsLetters = true
                },

                new()
                {
                    Name = "Cari",
                    Email = "cbrotherhed9@baidu.com",
                    DateOfBirth = new DateTime(2001, 12, 9),
                    Gender = "Female",
                    Address = "16 Miller Court",
                    ReceiveNewsLetters = true
                }
            });
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
        if (personUpdateRequest == null)
            throw new ArgumentNullException(nameof(Person));

        // Validation
        ValidationHelper.ModelValidation(personUpdateRequest);

        // Get matching person object to update
        Person? matchingPerson = _persons.FirstOrDefault(p =>
            p.Id == personUpdateRequest.Id);

        if (matchingPerson == null)
            throw new ArgumentException("Given person id doesn't exist!");

        // Update all details
        matchingPerson.Name = personUpdateRequest.Name;
        matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
        matchingPerson.Email = personUpdateRequest.Email;
        matchingPerson.CountryID = personUpdateRequest.CountryID;
        matchingPerson.Gender = personUpdateRequest.Gender.ToString();
        matchingPerson.Address = personUpdateRequest.Address;
        matchingPerson.ReceiveNewsLetters =
            personUpdateRequest.ReceiveNewsLetters;


        return matchingPerson.ToPersonResponse();
    }

    public bool DeletePerson(Guid? PersonId)
    {
        if (PersonId == null) throw new ArgumentNullException(nameof(PersonId));

        Person? person = _persons.FirstOrDefault(temp => temp.Id == PersonId);

        if (person == null) return false;

        _persons.RemoveAll(temp => temp.Id == PersonId);

        return true;
    }
}
