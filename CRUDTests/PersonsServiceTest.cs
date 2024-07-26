﻿using CRUDTests.Helpers;
using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using Xunit.Abstractions;

namespace CRUDTests;

public class PersonsServiceTest
{
    private readonly IPersonService _personService;
    private readonly ICountriesService _countriesService;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly PersonTestHelper _personTestHelper;
    private readonly ApplicationDbContext _dbContext;

    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
        List<Country> countriesInitialData = new() { };
        List<Person> personsInitialData = new() { };

        DbContextMock<ApplicationDbContext> dbContextMock =
            new(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

        ApplicationDbContext dbContext = dbContextMock.Object;
        dbContextMock.CreateDbSetMock(temp => temp.Countries,
            countriesInitialData);
        dbContextMock.CreateDbSetMock(temp => temp.Persons,
            personsInitialData);

        _countriesService = new CountriesService(dbContext);

        _personService = new PersonsService(dbContext, _countriesService);

        _testOutputHelper = testOutputHelper;

        _personTestHelper = new PersonTestHelper(_personService,
            _countriesService, testOutputHelper);
    }

    #region AddPerson

    // When we supply null value as PersonAddRequest,
    // it should throw ArgumentNullException
    [Fact]
    public async Task AddPerson_NullPerson()
    {
        // Arrange
        PersonAddRequest? personAddRequest = null;

        // Act
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _personService.AddPerson(personAddRequest);
        });
    }

    // When we supply null value as PersonName,
    // it should throw ArgumentException
    [Fact]
    public async Task AddPerson_PersonNameIsNull()
    {
        // Arrange
        PersonAddRequest? personAddRequest = new()
        {
            PersonName = null
        };

        // Act
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _personService.AddPerson(personAddRequest);
        });
    }

    // When we supply proper person details,
    // it should insert the person into the person list,
    // and it should return an object of PersonResponse, 
    // which includes with the newly generated PersonID
    [Fact]
    public async Task AddPerson_ProperPersonDetails()
    {
        // Arrange
        CountryResponse countryResponse = await
            _countriesService.AddCountry(new CountryAddRequest
                { CountryName = "TestCountry" });
        PersonAddRequest personAddRequest =
            _personTestHelper.CreatePersonAddRequest(
                "Nicola Kaleta",
                "nicola.kaleta@test.com",
                "sample address",
                countryResponse.CountryId,
                DateTime.Parse("2006-08-16"),
                GenderOptions.Female,
                true
            );


        // Act
        PersonResponse personResponseFromAdd =
            await _personTestHelper
                .AddPersonAndReturnResponse(personAddRequest);
        List<PersonResponse> personList = await _personService.GetAllPersons();


        // Assert
        Assert.True(personResponseFromAdd.ID != Guid.Empty);
        Assert.Contains(personResponseFromAdd, personList);
    }

    #endregion

    #region GetPersonById

    // When we supply null value as PersonID,
    // it should return null as PersonResponse
    [Fact]
    public async Task GetPersonById_NullPersonId()
    {
        // Arrange
        Guid? personId = null;

        // Act
        PersonResponse? personResponseFromGet = await
            _personService.GetPersonById(personId);

        // Assert
        Assert.Null(personResponseFromGet);
    }

    // If we supply a valid PersonID,
    // it should return the VALID person details as 
    // PersonResponse object
    [Fact]
    public async Task GetPersonById_WithPersonID()
    {
        // Arrange
        PersonResponse personResponseFromAdd = await
            _personTestHelper.CreateAndAddPerson(
                "Nicola Kaleta",
                "email@sample.com",
                "address",
                "Poland",
                DateTime.Parse("2000-08-16"),
                GenderOptions.Female,
                false
            );

        // Act
        PersonResponse? personResponseFromGet = await
            _personService.GetPersonById(personResponseFromAdd.ID);

        // Assert
        Assert.Equal(personResponseFromAdd, personResponseFromGet);
    }

    #endregion

    #region GetAllPersons

    // The GetAllPersons method should return an
    // empty list by default 
    [Fact]
    public async Task GetAllPersons_EmptyList()
    {
        //Act
        List<PersonResponse> personFromGet = await
            _personService.GetAllPersons();

        //Assert
        Assert.Empty(personFromGet);
    }

    // First, we will add few persons; and then we call
    // GetAllPersons method, it should return the same
    // persons that were added
    [Fact]
    public async Task GetAllPersons_AddFewPersons()
    {
        // Arrange
        List<PersonAddRequest> personRequests =
            await _personTestHelper.CreatePersonRequests();
        List<PersonResponse> personResponseListFromAdd = await
            _personTestHelper.AddPersonsAndReturnResponses(personRequests);

        // Act 
        List<PersonResponse> personsListFromGet = await
            _personService.GetAllPersons();

        // Log expected responses
        _personTestHelper.LogPersonResponses("Expected: ",
            personResponseListFromAdd);

        // Log actual responses
        _personTestHelper.LogPersonResponses("Actual: ", personsListFromGet);

        // Assert
        // Ensure that each person added is present in the list returned by GetAllPersons
        foreach (PersonResponse personResponseFromAdd in
                 personResponseListFromAdd)
            Assert.Contains(personResponseFromAdd, personsListFromGet);
    }

    #endregion

    #region GetFilteredPersons

    // If the search text is empty and search by
    // is "PersonName", it should return all persons
    [Fact]
    public async Task GetFilteredPersons_EmptySearchText()
    {
        // Arrange
        List<PersonAddRequest>
            personRequests =
                await _personTestHelper.CreatePersonRequests();
        List<PersonResponse> personResponseListFromAdd = await
            _personTestHelper.AddPersonsAndReturnResponses(personRequests);

        // Act 
        List<PersonResponse> personsListFromSearch = await
            _personService.GetFilteredPersons(nameof(Person.PersonName), "");

        // Log actual responses
        _personTestHelper.LogPersonResponses("Actual: ", personsListFromSearch);

        // Assert
        _personTestHelper.AssertPersonResponsesInList(personResponseListFromAdd,
            personsListFromSearch);
    }


    // First we will add few persons; and then 
    // we will search based on person name with some
    // search string. It should return the matching persons
    [Fact]
    public async Task GetFilteredPersons_SearchByPersonName()
    {
        // Arrange
        List<PersonAddRequest> personRequests = await
            _personTestHelper.CreatePersonRequests();
        List<PersonResponse> personResponseListFromAdd =
            await _personTestHelper
                .AddPersonsAndReturnResponses(personRequests);

        // Act 
        List<PersonResponse> personsListFromSearch = await
            _personService.GetFilteredPersons(nameof(Person.PersonName), "ma");

        // Log expected responses
        _personTestHelper.LogPersonResponses("Expected: ",
            personResponseListFromAdd.Where(p =>
                    p.PersonName.Contains("ma",
                        StringComparison.OrdinalIgnoreCase))
                .ToList());

        // Log actual responses
        _personTestHelper.LogPersonResponses("Actual: ", personsListFromSearch);

        // Assert
        // Ensure that each person in the filtered list matches the
        // search criteria and is present in the initial list
        foreach (PersonResponse personResponseFromAdd in
                 personResponseListFromAdd)
            if (personResponseFromAdd.PersonName != null &&
                personResponseFromAdd.PersonName.Contains("ma",
                    StringComparison.OrdinalIgnoreCase))
                Assert.Contains(personResponseFromAdd, personsListFromSearch);
    }

    #endregion

    #region GetUpdatedPerson

    // When we sort based on PersonName in DESC,
    // it should return the persons in descending order
    [Fact]
    public async Task GetSortedPersons()
    {
        // Arrange
        List<PersonAddRequest> personRequests =
            await _personTestHelper.CreatePersonRequests();
        List<PersonResponse> personResponseListFromAdd = await
            _personTestHelper.AddPersonsAndReturnResponses(personRequests);

        List<PersonResponse> allPersons = await _personService.GetAllPersons();
        // Act 
        List<PersonResponse> personsListFromSort = await
            _personService.GetSortedPersons(allPersons,
                nameof(Person.PersonName),
                SortOderOptions.DESC);

        // Log expected responses
        _personTestHelper.LogPersonResponses("Expected: ",
            personResponseListFromAdd.Where(p =>
                    p.PersonName.Contains("ma",
                        StringComparison.OrdinalIgnoreCase))
                .ToList());

        // Log actual responses
        _personTestHelper.LogPersonResponses("Actual: ", personsListFromSort);

        personResponseListFromAdd =
            personResponseListFromAdd.OrderByDescending(temp => temp.PersonName)
                .ToList();

        // Assert
        for (int i = 0; i < personResponseListFromAdd.Count; i++)
            Assert.Equal(personResponseListFromAdd[i], personsListFromSort[i]);
    }

    #endregion

    #region UpdatePerson

    // When we supply null value as PersonUpdateRequest,
    // it should throw ArgumentNullException

    [Fact]
    public async Task UpdatePerson_NullPerson()
    {
        // Arrange
        PersonUpdateRequest? personUpdateRequest = null;

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            // Act
            await _personService.UpdatePerson(personUpdateRequest);
        });
    }

    // When we supply invalid person ID,
    // // it should throw ArgumentException

    [Fact]
    public async Task UpdatePerson_InvalidPersonId()
    {
        // Arrange
        PersonUpdateRequest? personUpdateRequest = new()
        {
            Id = Guid.NewGuid()
        };

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            // Act
            await _personService.UpdatePerson(personUpdateRequest);
        });
    }

    // When PersonName is null
    // // it should throw ArgumentException

    [Fact]
    public async Task UpdatePerson_PersonNameIsNull()
    {
        // Arrange
        CountryAddRequest countryAddRequest = new()
        {
            CountryName = "UK"
        };

        CountryResponse countryResponseFromAdd = await
            _countriesService.AddCountry(countryAddRequest);

        PersonAddRequest personAddRequest = new()
        {
            PersonName = "John",
            CountryId = countryResponseFromAdd.CountryId,
            Address = "Abc road",
            Gender = GenderOptions.Male,
            Email = "john@example.com"
        };

        PersonResponse personResponseFromAdd = await
            _personService.AddPerson(personAddRequest);

        PersonUpdateRequest personUpdateRequest =
            personResponseFromAdd.ToPersonUpdateRequest();

        personUpdateRequest.PersonName = null;

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            // Act
            await _personService.UpdatePerson(personUpdateRequest);
        });
    }

    // First, add a new person; then try to update the 
    // personName and Email

    [Fact]
    public async Task UpdatePerson_PersonFullDetailsUpdate()
    {
        // Arrange
        CountryAddRequest countryAddRequest = new()
        {
            CountryName = "UK"
        };

        CountryResponse countryResponseFromAdd = await
            _countriesService.AddCountry(countryAddRequest);

        PersonAddRequest personAddRequest = new()
        {
            PersonName = "John",
            CountryId = countryResponseFromAdd.CountryId,
            Address = "Abc road",
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Email = "abc@example.com",
            Gender = GenderOptions.Male,
            ReceiveNewsLetters = true
        };

        PersonResponse personResponseFromAdd = await
            _personService.AddPerson(personAddRequest);

        PersonUpdateRequest personUpdateRequest =
            personResponseFromAdd.ToPersonUpdateRequest();

        personUpdateRequest.PersonName = "William";
        personUpdateRequest.Email = "william@example.com";


        // Act
        PersonResponse personResponseFromUpdate = await
            _personService.UpdatePerson(personUpdateRequest);

        PersonResponse personResponseFromGet = await
            _personService.GetPersonById(personResponseFromAdd.ID);

        // Assert
        Assert.Equal(personResponseFromGet, personResponseFromUpdate);
    }

    #endregion

    #region DeletePerson

    // If you supply an valid PersonId, it should return true
    [Fact]
    public async Task DeletePerson_ValidPersonId()
    {
        // Arrange
        CountryAddRequest countryAddRequest = new()
        {
            CountryName = "USA"
        };

        CountryResponse countryResponseFromAdd = await
            _countriesService.AddCountry(countryAddRequest);

        PersonAddRequest personAddRequest = new()
        {
            PersonName = "Jones",
            CountryId = countryResponseFromAdd.CountryId,
            Address = "address",
            DateOfBirth = DateTime.Parse("2010-01-01"),
            Email = "abc@example.pl",
            ReceiveNewsLetters = true,
            Gender = GenderOptions.Male
        };

        PersonResponse personResponseFromAdd = await
            _personService.AddPerson(personAddRequest);

        // Act
        bool isDeleted =
            await _personService.DeletePerson(personResponseFromAdd.ID);

        // Assert
        Assert.True(isDeleted);
    }

    // If you supply an invalid PersonId, it should return false
    [Fact]
    public async Task DeletePerson_InvalidPersonId()
    {
        // Act
        bool isDeleted = await _personService.DeletePerson(Guid.NewGuid());

        // Assert
        Assert.False(isDeleted);
    }

    #endregion
}
