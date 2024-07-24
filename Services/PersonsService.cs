﻿using System.ComponentModel.DataAnnotations;
using Entities;
using Microsoft.VisualBasic;
using ServiceContracts;
using ServiceContracts.DTO;
using Services.Helpers;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
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

    public async Task<PersonResponse> AddPerson(
        PersonAddRequest? personAddRequest
    )
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
        await _db.SaveChangesAsync();

        //         _db.sp_InsertPerson(person);

        // Convert Person object into PersonResponse type
        return person.ToPersonResponse();
    }

    public async Task<List<PersonResponse>> GetAllPersons()
    {
        // SELECT * FROM Persons
        List<Person> persons =
            await _db.Persons.Include("Country").ToListAsync();

        return persons
            .Select(p => p.ToPersonResponse()).ToList();


/*        return _db.sp_GetAllPersons()
            .Select(temp => ConvertPersonToPersonResponse(temp)).ToList();
*/
    }

    public async Task<PersonResponse?> GetPersonById(Guid? personId)
    {
        if (personId == null) return null;

        Person? person = await _db.Persons.Include("Country")
            .FirstOrDefaultAsync(p => p.Id == personId);

        if (person == null) return null;

        return person.ToPersonResponse();
    }

    public async Task<List<PersonResponse>> GetFilteredPersons(
        string searchBy,
        string? searchString
    )
    {
        List<PersonResponse> allPersons = await GetAllPersons();
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

    public async Task<List<PersonResponse>> GetSortedPersons(
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

    public async Task<PersonResponse> UpdatePerson(
        PersonUpdateRequest? personUpdateRequest
    )
    {
        if (personUpdateRequest == null)
            throw new ArgumentNullException(nameof(personUpdateRequest));

        // Validation
        ValidationHelper.ModelValidation(personUpdateRequest);

        // Get matching person object to update
        Person? matchingPerson = await _db.Persons.FirstOrDefaultAsync(p =>
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

        // Call the stored procedure to update the person
        _db.UpdatePersonAsync(matchingPerson).GetAwaiter().GetResult();

        return matchingPerson.ToPersonResponse();
    }

    public async Task<bool> DeletePerson(Guid? personId)
    {
        return sp_DeletePersonAsync(personId).GetAwaiter().GetResult();
    }

    public async Task<bool> sp_DeletePersonAsync(Guid? personId)
    {
        if (personId == null) throw new ArgumentNullException(nameof(personId));

        int result = await _db.sp_DeletePersonAsync(personId.Value);

        return result > 0;
    }
}
