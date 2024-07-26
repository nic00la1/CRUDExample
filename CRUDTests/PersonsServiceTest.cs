using CRUDTests.Helpers;
using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using Xunit.Abstractions;
using AutoFixture;
using FluentAssertions;

namespace CRUDTests;

public class PersonsServiceTest
{
    private readonly IPersonService _personService;
    private readonly ICountriesService _countriesService;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly PersonTestHelper _personTestHelper;
    private readonly ApplicationDbContext _dbContext;
    private readonly IFixture _fixture;

    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
        _fixture = new Fixture();
        List<Country> countriesInitialData = new() { };
        List<Person> personsInitialData = new() { };

        DbContextMock<ApplicationDbContext> dbContextMock =
            new(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

        ApplicationDbContext dbContext = dbContextMock.Object;
        dbContextMock.CreateDbSetMock(temp => temp.Countries,
            countriesInitialData);
        dbContextMock.CreateDbSetMock(temp => temp.Persons,
            personsInitialData);

        _countriesService = new CountriesService(null);

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
        Func<Task> action = async () =>
        {
            await _personService.AddPerson(personAddRequest);
        };

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    // When we supply null value as PersonName,
    // it should throw ArgumentException
    [Fact]
    public async Task AddPerson_PersonNameIsNull()
    {
        // Arrange
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, null as string).Create();

        // Assert
        Func<Task> action = async () =>
        {
            await _personService.AddPerson(personAddRequest);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }

    // When we supply proper person details,
    // it should insert the person into the person list,
    // and it should return an object of PersonResponse, 
    // which includes with the newly generated PersonID
    [Fact]
    public async Task AddPerson_ProperPersonDetails()
    {
        // Arrange
        PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone@example.com").Create();

        // Act
        PersonResponse personResponseFromAdd =
            await _personService.AddPerson(personAddRequest);
        List<PersonResponse> personList = await _personService.GetAllPersons();

        // Fluent Assertion
        personResponseFromAdd.ID.Should().NotBe(Guid.Empty);

        personList.Should().Contain(personResponseFromAdd);
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

        // Fluent Assertion
        personResponseFromGet.Should().BeNull();
    }

    // If we supply a valid PersonID,
    // it should return the VALID person details as 
    // PersonResponse object
    [Fact]
    public async Task GetPersonById_WithPersonID()
    {
        // Arrange
        CountryAddRequest countryRequest = _fixture.Create<CountryAddRequest>();

        CountryResponse countryResponse =
            await _countriesService.AddCountry(countryRequest);

        PersonAddRequest personRequest =
            _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "email@sample.com").Create();

        PersonResponse personResponseFromAdd =
            await _personService.AddPerson(personRequest);

        PersonResponse? personResponseFromGet = await
            _personService.GetPersonById(personResponseFromAdd.ID);

        // Fluent Assertion
        personResponseFromGet.Should().Be(personResponseFromAdd);
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

        // Fluent Assertion
        personFromGet.Should().BeEmpty();
    }

    // First, we will add few persons; and then we call
    // GetAllPersons method, it should return the same
    // persons that were added
    [Fact]
    public async Task GetAllPersons_AddFewPersons()
    {
        // Arrange
        CountryAddRequest countryRequest1 =
            _fixture.Create<CountryAddRequest>();
        CountryAddRequest countryRequest2 =
            _fixture.Create<CountryAddRequest>();

        CountryResponse countryResponse1 =
            await _countriesService.AddCountry(countryRequest1);

        CountryResponse countryResponse2 =
            await _countriesService.AddCountry(countryRequest2);

        PersonAddRequest personRequest1 = _fixture
            .Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone_1@example.com").Create();

        PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone_2@example.com").Create();

        PersonAddRequest personRequest3 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone_3@example.com").Create();

        List<PersonAddRequest> personRequests = new()
        {
            personRequest1,
            personRequest2,
            personRequest3
        };

        List<PersonResponse> personResponseListFromAdd = new();

        foreach (PersonAddRequest personRequest in personRequests)
        {
            PersonResponse personResponse =
                await _personService.AddPerson(personRequest);

            personResponseListFromAdd.Add(personResponse);
        }

        List<PersonResponse> personsListFromGet = await
            _personService.GetAllPersons();

        // Log expected responses
        _personTestHelper.LogPersonResponses("Expected: ",
            personResponseListFromAdd);

        // Log actual responses
        _personTestHelper.LogPersonResponses("Actual: ", personsListFromGet);

        // Fluent Assertion
        personsListFromGet.Should()
            .BeEquivalentTo(personResponseListFromAdd);
    }

    #endregion

    #region GetFilteredPersons

    // If the search text is empty and search by
    // is "PersonName", it should return all persons
    [Fact]
    public async Task GetFilteredPersons_EmptySearchText()
    {
        // Arrange
        CountryAddRequest countryRequest1 =
            _fixture.Create<CountryAddRequest>();
        CountryAddRequest countryRequest2 =
            _fixture.Create<CountryAddRequest>();

        CountryResponse countryResponse1 =
            await _countriesService.AddCountry(countryRequest1);

        CountryResponse countryResponse2 =
            await _countriesService.AddCountry(countryRequest2);

        PersonAddRequest personRequest1 = _fixture
            .Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone_1@example.com").Create();

        PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone_2@example.com").Create();

        PersonAddRequest personRequest3 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone_3@example.com").Create();

        List<PersonAddRequest> personRequests = new()
        {
            personRequest1,
            personRequest2,
            personRequest3
        };

        List<PersonResponse> personResponseListFromAdd = new();

        foreach (PersonAddRequest personRequest in personRequests)
        {
            PersonResponse personResponse =
                await _personService.AddPerson(personRequest);

            personResponseListFromAdd.Add(personResponse);
        }

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
        CountryAddRequest countryRequest1 =
            _fixture.Create<CountryAddRequest>();
        CountryAddRequest countryRequest2 =
            _fixture.Create<CountryAddRequest>();

        CountryResponse countryResponse1 =
            await _countriesService.AddCountry(countryRequest1);

        CountryResponse countryResponse2 =
            await _countriesService.AddCountry(countryRequest2);

        PersonAddRequest personRequest1 = _fixture
            .Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Rahman")
            .With(temp => temp.Email, "someone_1@example.com")
            .With(temp => temp.CountryId, countryResponse1.CountryId).Create();

        PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Mary")
            .With(temp => temp.Email, "someone_2@example.com")
            .With(temp => temp.CountryId, countryResponse1.CountryId).Create();


        PersonAddRequest personRequest3 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Scott")
            .With(temp => temp.Email, "someone_3@example.com")
            .With(temp => temp.CountryId, countryResponse2.CountryId).Create();

        List<PersonAddRequest> personRequests = new()
        {
            personRequest1,
            personRequest2,
            personRequest3
        };

        List<PersonResponse> personResponseListFromAdd = new();

        foreach (PersonAddRequest personRequest in personRequests)
        {
            PersonResponse personResponse =
                await _personService.AddPerson(personRequest);

            personResponseListFromAdd.Add(personResponse);
        }

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
        CountryAddRequest countryRequest1 =
            _fixture.Create<CountryAddRequest>();
        CountryAddRequest countryRequest2 =
            _fixture.Create<CountryAddRequest>();

        CountryResponse countryResponse1 =
            await _countriesService.AddCountry(countryRequest1);

        CountryResponse countryResponse2 =
            await _countriesService.AddCountry(countryRequest2);

        PersonAddRequest personRequest1 = _fixture
            .Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Smith")
            .With(temp => temp.Email, "someone_1@example.com")
            .With(temp => temp.CountryId, countryResponse1.CountryId).Create();

        PersonAddRequest personRequest2 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Mary")
            .With(temp => temp.Email, "someone_2@example.com")
            .With(temp => temp.CountryId, countryResponse1.CountryId).Create();


        PersonAddRequest personRequest3 = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Rahman")
            .With(temp => temp.Email, "someone_3@example.com")
            .With(temp => temp.CountryId, countryResponse2.CountryId).Create();

        List<PersonAddRequest> personRequests = new()
        {
            personRequest1,
            personRequest2,
            personRequest3
        };

        List<PersonResponse> personResponseListFromAdd = new();

        foreach (PersonAddRequest personRequest in personRequests)
        {
            PersonResponse personResponse =
                await _personService.AddPerson(personRequest);

            personResponseListFromAdd.Add(personResponse);
        }

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

        Func<Task> action = async () =>
        {
            // Act
            await _personService.UpdatePerson(personUpdateRequest);
        };

        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    // When we supply invalid person ID,
    // // it should throw ArgumentException

    [Fact]
    public async Task UpdatePerson_InvalidPersonId()
    {
        // Arrange
        PersonUpdateRequest? personUpdateRequest =
            _fixture.Build<PersonUpdateRequest>().Create();

        Func<Task> action = async () =>
        {
            // Act
            await _personService.UpdatePerson(personUpdateRequest);
        };

        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentException>();
    }

    // When PersonName is null
    // // it should throw ArgumentException

    [Fact]
    public async Task UpdatePerson_PersonNameIsNull()
    {
        // Arrange
        CountryAddRequest countryRequest =
            _fixture.Create<CountryAddRequest>();

        CountryResponse countryResponse = await
            _countriesService.AddCountry(countryRequest);

        PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Rahman")
            .With(temp => temp.Email, "someone_1@example.com")
            .With(temp => temp.CountryId, countryResponse.CountryId).Create();

        PersonResponse personResponseFromAdd = await
            _personService.AddPerson(personAddRequest);

        PersonUpdateRequest personUpdateRequest =
            personResponseFromAdd.ToPersonUpdateRequest();

        personUpdateRequest.PersonName = null;

        Func<Task> action = async () =>
        {
            // Act
            await _personService.UpdatePerson(personUpdateRequest);
        };

        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentException>();
    }

    // First, add a new person; then try to update the 
    // personName and Email

    [Fact]
    public async Task UpdatePerson_PersonFullDetailsUpdate()
    {
        // Arrange
        CountryAddRequest countryRequest =
            _fixture.Create<CountryAddRequest>();

        CountryResponse countryResponse = await
            _countriesService.AddCountry(countryRequest);

        PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Rahman")
            .With(temp => temp.Email, "someone_1@example.com")
            .With(temp => temp.CountryId, countryResponse.CountryId).Create();

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

        // Fluent Assertion
        personResponseFromUpdate.Should().Be(personResponseFromGet);
    }

    #endregion

    #region DeletePerson

    // If you supply an valid PersonId, it should return true
    [Fact]
    public async Task DeletePerson_ValidPersonId()
    {
        // Arrange
        CountryAddRequest countryRequest =
            _fixture.Create<CountryAddRequest>();

        CountryResponse countryResponse = await
            _countriesService.AddCountry(countryRequest);

        PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, "Rahman")
            .With(temp => temp.Email, "someone_1@example.com")
            .With(temp => temp.CountryId, countryResponse.CountryId).Create();

        PersonResponse personResponseFromAdd = await
            _personService.AddPerson(personAddRequest);

        // Act
        bool isDeleted =
            await _personService.DeletePerson(personResponseFromAdd.ID);

        // Fluent Assertion
        isDeleted.Should().BeTrue();
    }

    // If you supply an invalid PersonId, it should return false
    [Fact]
    public async Task DeletePerson_InvalidPersonId()
    {
        // Act
        bool isDeleted = await _personService.DeletePerson(Guid.NewGuid());

        // Fluent Assertion
        isDeleted.Should().BeFalse();
    }

    #endregion
}
