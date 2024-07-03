using CRUDTests.Helpers;
using Entities;
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

    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
        _personService = new PersonsService();
        _countriesService = new CountriesService();
        _testOutputHelper = testOutputHelper;
        _personTestHelper = new PersonTestHelper(_personService,
            _countriesService, testOutputHelper);
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
            _personTestHelper.AddPersonAndReturnResponse(personAddRequest);
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
        PersonResponse personResponseFromAdd =
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
        List<PersonAddRequest> personRequests =
            _personTestHelper.CreatePersonRequests();
        List<PersonResponse> personResponseListFromAdd =
            _personTestHelper.AddPersonsAndReturnResponses(personRequests);

        // Act 
        List<PersonResponse> personsListFromGet =
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
    public void GetFilteredPersons_EmptySearchText()
    {
        // Arrange
        List<PersonAddRequest>
            personRequests =
                _personTestHelper.CreatePersonRequests();
        List<PersonResponse> personResponseListFromAdd =
            _personTestHelper.AddPersonsAndReturnResponses(personRequests);

        // Act 
        List<PersonResponse> personsListFromSearch =
            _personService.GetFilteredPersons(nameof(Person.Name), "");

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
    public void GetFilteredPersons_SearchByPersonName()
    {
        // Arrange
        List<PersonAddRequest> personRequests =
            _personTestHelper.CreatePersonRequests();
        List<PersonResponse> personResponseListFromAdd =
            _personTestHelper.AddPersonsAndReturnResponses(personRequests);

        // Act 
        List<PersonResponse> personsListFromSearch =
            _personService.GetFilteredPersons(nameof(Person.Name), "ma");

        // Log expected responses
        _personTestHelper.LogPersonResponses("Expected: ",
            personResponseListFromAdd.Where(p =>
                    p.Name.Contains("ma", StringComparison.OrdinalIgnoreCase))
                .ToList());

        // Log actual responses
        _personTestHelper.LogPersonResponses("Actual: ", personsListFromSearch);

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

    #region GetUpdatedPerson

    // When we sort based on PersonName in DESC,
    // it should return the persons in descending order
    [Fact]
    public void GetSortedPersons()
    {
        // Arrange
        List<PersonAddRequest> personRequests =
            _personTestHelper.CreatePersonRequests();
        List<PersonResponse> personResponseListFromAdd =
            _personTestHelper.AddPersonsAndReturnResponses(personRequests);

        List<PersonResponse> allPersons = _personService.GetAllPersons();
        // Act 
        List<PersonResponse> personsListFromSort =
            _personService.GetSortedPersons(allPersons, nameof(Person.Name),
                SortOderOptions.DESC);

        // Log expected responses
        _personTestHelper.LogPersonResponses("Expected: ",
            personResponseListFromAdd.Where(p =>
                    p.Name.Contains("ma", StringComparison.OrdinalIgnoreCase))
                .ToList());

        // Log actual responses
        _personTestHelper.LogPersonResponses("Actual: ", personsListFromSort);

        personResponseListFromAdd =
            personResponseListFromAdd.OrderByDescending(temp => temp.Name)
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
    public void UpdatePerson_NullPerson()
    {
        // Arrange
        PersonUpdateRequest? personUpdateRequest = null;

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            // Act
            _personService.UpdatePerson(personUpdateRequest);
        });
    }

    // When we supply invalid person ID,
    // // it should throw ArgumentException

    [Fact]
    public void UpdatePerson_InvalidPersonId()
    {
        // Arrange
        PersonUpdateRequest? personUpdateRequest = new()
        {
            Id = Guid.NewGuid()
        };

        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            // Act
            _personService.UpdatePerson(personUpdateRequest);
        });
    }

    // When PersonName is null
    // // it should throw ArgumentException

    [Fact]
    public void UpdatePerson_PersonNameIsNull()
    {
        // Arrange
        CountryAddRequest countryAddRequest = new()
        {
            CountryName = "UK"
        };

        CountryResponse countryResponseFromAdd =
            _countriesService.AddCountry(countryAddRequest);

        PersonAddRequest personAddRequest = new()
        {
            Name = "John",
            CountryID = countryResponseFromAdd.CountryId,
            Address = "Abc road",
            Gender = GenderOptions.Male,
            Email = "john@example.com"
        };

        PersonResponse personResponseFromAdd =
            _personService.AddPerson(personAddRequest);

        PersonUpdateRequest personUpdateRequest =
            personResponseFromAdd.ToPersonUpdateRequest();

        personUpdateRequest.Name = null;

        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            // Act
            _personService.UpdatePerson(personUpdateRequest);
        });
    }

    // First, add a new person; then try to update the 
    // personName and Email

    [Fact]
    public void UpdatePerson_PersonFullDetailsUpdate()
    {
        // Arrange
        CountryAddRequest countryAddRequest = new()
        {
            CountryName = "UK"
        };

        CountryResponse countryResponseFromAdd =
            _countriesService.AddCountry(countryAddRequest);

        PersonAddRequest personAddRequest = new()
        {
            Name = "John",
            CountryID = countryResponseFromAdd.CountryId,
            Address = "Abc road",
            DateOfBirth = DateTime.Parse("2000-01-01"),
            Email = "abc@example.com",
            Gender = GenderOptions.Male,
            ReceiveNewsLetters = true
        };

        PersonResponse personResponseFromAdd =
            _personService.AddPerson(personAddRequest);

        PersonUpdateRequest personUpdateRequest =
            personResponseFromAdd.ToPersonUpdateRequest();

        personUpdateRequest.Name = "William";
        personUpdateRequest.Email = "william@example.com";


        // Act
        PersonResponse personResponseFromUpdate =
            _personService.UpdatePerson(personUpdateRequest);

        PersonResponse personResponseFromGet =
            _personService.GetPersonById(personResponseFromAdd.ID);

        // Assert
        Assert.Equal(personResponseFromGet, personResponseFromUpdate);
    }

    #endregion

    #region DeletePerson

    // If you supply an valid PersonId, it should return true
    [Fact]
    public void DeletePerson_ValidPersonId()
    {
        // Arrange
        CountryAddRequest countryAddRequest = new()
        {
            CountryName = "USA"
        };

        CountryResponse countryResponseFromAdd =
            _countriesService.AddCountry(countryAddRequest);

        PersonAddRequest personAddRequest = new()
        {
            Name = "Jones",
            CountryID = countryResponseFromAdd.CountryId,
            Address = "address",
            DateOfBirth = DateTime.Parse("2010-01-01"),
            Email = "abc@example.pl",
            ReceiveNewsLetters = true,
            Gender = GenderOptions.Male
        };

        PersonResponse personResponseFromAdd =
            _personService.AddPerson(personAddRequest);

        // Act
        bool isDeleted = _personService.DeletePerson(personResponseFromAdd.ID);

        // Assert
        Assert.True(isDeleted);
    }

    // If you supply an invalid PersonId, it should return false
    [Fact]
    public void DeletePerson_InvalidPersonId()
    {
        // Act
        bool isDeleted = _personService.DeletePerson(Guid.NewGuid());

        // Assert
        Assert.False(isDeleted);
    }

    #endregion
}
