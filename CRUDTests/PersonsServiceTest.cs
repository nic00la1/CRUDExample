﻿using Entities;
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

    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
        _personService = new PersonsService();
        _countriesService = new CountriesService();
        _testOutputHelper = testOutputHelper;
    }

    private PersonResponse AddPersonAndReturnResponse(PersonAddRequest request)
    {
        return _personService.AddPerson(request);
    }

    private PersonAddRequest CreatePersonAddRequest(
        string name,
        string email,
        string address,
        Guid countryId,
        DateTime dateOfBirth,
        GenderOptions gender,
        bool receiveNewsLetters
    )
    {
        return new PersonAddRequest
        {
            Name = name,
            Email = email,
            Address = address,
            CountryID = countryId,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            ReceiveNewsLetters = receiveNewsLetters
        };
    }

    private List<PersonAddRequest> CreatePersonRequests()
    {
        CountryResponse countryResponse1 =
            _countriesService.AddCountry(new CountryAddRequest
                { CountryName = "Poland" });
        CountryResponse countryResponse2 =
            _countriesService.AddCountry(new CountryAddRequest
                { CountryName = "Germany" });

        List<PersonAddRequest> personRequests = new()
        {
            CreatePersonAddRequest("Mary", "mary@example.com", "address",
                countryResponse1.CountryId, DateTime.Parse("1999-04-20"),
                GenderOptions.Female, true),
            CreatePersonAddRequest("Rahman", "rahman@gmail.com", "address",
                countryResponse2.CountryId, DateTime.Parse("1998-04-20"),
                GenderOptions.Male, false),
            CreatePersonAddRequest("Smith", "smith@gmail.com", "address",
                countryResponse2.CountryId, DateTime.Parse("2007-02-3"),
                GenderOptions.Male, true)
        };

        return personRequests;
    }

    private List<PersonResponse> AddPersonsAndReturnResponses(
        List<PersonAddRequest> personRequests
    )
    {
        List<PersonResponse> personResponses = personRequests
            .Select(request => _personService.AddPerson(request)).ToList();
        LogPersonResponses("Expected: ", personResponses);
        return personResponses;
    }

    private void LogPersonResponses(string message,
                                    List<PersonResponse> personResponses
    )
    {
        _testOutputHelper.WriteLine(message);
        foreach (PersonResponse personResponse in personResponses)
            _testOutputHelper.WriteLine(personResponse.ToString());
    }

    private void AssertPersonResponsesInList(
        List<PersonResponse> expectedResponses,
        List<PersonResponse> actualResponses
    )
    {
        foreach (PersonResponse expectedResponse in expectedResponses)
            Assert.Contains(expectedResponse, actualResponses);
    }

    private PersonResponse CreateAndAddPerson(string name,
                                              string email,
                                              string address,
                                              string countryName,
                                              DateTime dateOfBirth,
                                              GenderOptions gender,
                                              bool receiveNewsLetters
    )
    {
        CountryResponse countryResponse =
            _countriesService.AddCountry(new CountryAddRequest
                { CountryName = countryName });
        PersonAddRequest personRequest = CreatePersonAddRequest(name, email,
            address, countryResponse.CountryId, dateOfBirth, gender,
            receiveNewsLetters);
        return _personService.AddPerson(personRequest);
    }

    #region AddPerson

    // When we supply null value as PersonAddRequest,
    // it should throw ArgumentNullException
    [Fact]
    public void AddPerson_NullPerson()
    {
        // Arrange
        PersonAddRequest? personAddRequest = null;

        // Act
        Assert.Throws<ArgumentNullException>(() =>
        {
            _personService.AddPerson(personAddRequest);
        });
    }

    // When we supply null value as PersonName,
    // it should throw ArgumentException
    [Fact]
    public void AddPerson_PersonNameIsNull()
    {
        // Arrange
        PersonAddRequest? personAddRequest = new()
        {
            Name = null
        };

        // Act
        Assert.Throws<ArgumentException>(() =>
        {
            _personService.AddPerson(personAddRequest);
        });
    }

    // When we supply proper person details,
    // it should insert the person into the person list,
    // and it should return an object of PersonResponse, 
    // which includes with the newly generated PersonID
    [Fact]
    public void AddPerson_ProperPersonDetails()
    {
        // Arrange
        CountryResponse countryResponse =
            _countriesService.AddCountry(new CountryAddRequest
                { CountryName = "TestCountry" });
        PersonAddRequest personAddRequest = CreatePersonAddRequest(
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
            AddPersonAndReturnResponse(personAddRequest);
        List<PersonResponse> personList = _personService.GetAllPersons();


        // Assert
        Assert.True(personResponseFromAdd.ID != Guid.Empty);
        Assert.Contains(personResponseFromAdd, personList);
    }

    #endregion

    #region GetPersonById

    // When we supply null value as PersonID,
    // it should return null as PersonResponse
    [Fact]
    public void GetPersonById_NullPersonId()
    {
        // Arrange
        Guid? personId = null;

        // Act
        PersonResponse? personResponseFromGet =
            _personService.GetPersonById(personId);

        // Assert
        Assert.Null(personResponseFromGet);
    }

    // If we supply a valid PersonID,
    // it should return the VALID person details as 
    // PersonResponse object
    [Fact]
    public void GetPersonById_WithPersonID()
    {
        // Arrange
        PersonResponse personResponseFromAdd = CreateAndAddPerson(
            "Nicola Kaleta",
            "email@sample.com",
            "address",
            "Poland",
            DateTime.Parse("2000-08-16"),
            GenderOptions.Female,
            false
        );

        // Act
        PersonResponse? personResponseFromGet =
            _personService.GetPersonById(personResponseFromAdd.ID);

        // Assert
        Assert.Equal(personResponseFromAdd, personResponseFromGet);
    }

    #endregion

    #region GetAllPersons

    // The GetAllPersons method should return an
    // empty list by default 
    [Fact]
    public void GetAllPersons_EmptyList()
    {
        //Act
        List<PersonResponse> personFromGet =
            _personService.GetAllPersons();

        //Assert
        Assert.Empty(personFromGet);
    }

    // First, we will add few persons; and then we call
    // GetAllPersons method, it should return the same
    // persons that were added
    [Fact]
    public void GetAllPersons_AddFewPersons()
    {
        // Arrange
        List<PersonAddRequest> personRequests = CreatePersonRequests();
        List<PersonResponse> personResponseListFromAdd =
            AddPersonsAndReturnResponses(personRequests);

        // Act 
        List<PersonResponse> personsListFromGet =
            _personService.GetAllPersons();

        // Log expected responses
        LogPersonResponses("Expected: ", personResponseListFromAdd);

        // Log actual responses
        LogPersonResponses("Actual: ", personsListFromGet);

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
    public void GetFilteredPersons_EmptySearchText()
    {
        // Arrange
        List<PersonAddRequest>
            personRequests =
                CreatePersonRequests();
        List<PersonResponse> personResponseListFromAdd =
            AddPersonsAndReturnResponses(personRequests);

        // Act 
        List<PersonResponse> personsListFromSearch =
            _personService.GetFilteredPersons(nameof(Person.Name), "");

        // Log actual responses
        LogPersonResponses("Actual: ", personsListFromSearch);

        // Assert
        AssertPersonResponsesInList(personResponseListFromAdd,
            personsListFromSearch);
    }


    // First we will add few persons; and then 
    // we will search based on person name with some
    // search string. It should return the matching persons
    [Fact]
    public void GetFilteredPersons_SearchByPersonName()
    {
        // Arrange
        List<PersonAddRequest> personRequests = CreatePersonRequests();
        List<PersonResponse> personResponseListFromAdd =
            AddPersonsAndReturnResponses(personRequests);

        // Act 
        List<PersonResponse> personsListFromSearch =
            _personService.GetFilteredPersons(nameof(Person.Name), "ma");

        // Log expected responses
        LogPersonResponses("Expected: ",
            personResponseListFromAdd.Where(p =>
                    p.Name.Contains("ma", StringComparison.OrdinalIgnoreCase))
                .ToList());

        // Log actual responses
        LogPersonResponses("Actual: ", personsListFromSearch);

        // Assert
        // Ensure that each person in the filtered list matches the
        // search criteria and is present in the initial list
        foreach (PersonResponse personResponseFromAdd in
                 personResponseListFromAdd)
            if (personResponseFromAdd.Name != null &&
                personResponseFromAdd.Name.Contains("ma",
                    StringComparison.OrdinalIgnoreCase))
                Assert.Contains(personResponseFromAdd, personsListFromSearch);
    }

    #endregion
}
