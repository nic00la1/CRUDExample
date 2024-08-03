using System.Linq.Expressions;
using CRUDTests.Helpers;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoryContracts;
using Serilog;
using Services;

namespace CRUDTests;

public class PersonsServiceTest
{
    private readonly IPersonsGetterService _personsGetterService;
    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsUpdaterService _personsUpdaterService;
    private readonly IPersonsDeleterService _personsDeleterService;
    private readonly IPersonsSorterService _personsSorterService;


    private readonly Mock<IPersonsRepository> _personsRepositoryMock;
    private readonly IPersonsRepository _personsRepository;

    private readonly PersonTestHelper _personTestHelper;
    private readonly IFixture _fixture;
    private readonly ICountriesAdderService _countriesService;

    public PersonsServiceTest(ITestOutputHelper testOutputHelper)
    {
        _fixture = new Fixture();
        _personsRepositoryMock = new Mock<IPersonsRepository>();
        _personsRepository = _personsRepositoryMock.Object;

        Mock<IDiagnosticContext> diagnosticContextMock =
            new();
        Mock<ILogger<PersonsGetterService>> loggerMock =
            new();

        _personsGetterService = new PersonsGetterService(_personsRepository,
            loggerMock.Object, diagnosticContextMock.Object);

        _personsAdderService = new PersonsAdderService(_personsRepository,
            loggerMock.Object, diagnosticContextMock.Object);

        _personsDeleterService = new PersonsDeleterService(_personsRepository,
            loggerMock.Object, diagnosticContextMock.Object);

        _personsSorterService = new PersonsSorterService(_personsRepository,
            loggerMock.Object, diagnosticContextMock.Object);

        _personsUpdaterService = new PersonsUpdaterService(_personsRepository,
            loggerMock.Object, diagnosticContextMock.Object);


        _personTestHelper = new PersonTestHelper(_personsGetterService,
            _personsAdderService, _personsSorterService, _personsUpdaterService,
            _personsDeleterService,
            _countriesService, testOutputHelper);
    }

    #region AddPerson

    // When we supply null value as PersonAddRequest,
    // it should throw ArgumentNullException
    [Fact]
    public async Task AddPerson_NullPerson_ToBeArgumentNullException()
    {
        // Arrange
        PersonAddRequest? personAddRequest = null;

        // Act
        Func<Task> action = async () =>
        {
            await _personsAdderService.AddPerson(personAddRequest);
        };

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    // When we supply null value as PersonName,
    // it should throw ArgumentException
    [Fact]
    public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
    {
        // Arrange
        PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.PersonName, null as string).Create();

        Person person = personAddRequest.ToPerson();

        // When PersonRepository.AddPerson is called, it has to
        // return the same "person" object
        _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
            .ReturnsAsync(person);

        // Assert
        Func<Task> action = async () =>
        {
            await _personsAdderService.AddPerson(personAddRequest);
        };

        await action.Should().ThrowAsync<ArgumentException>();
    }

    // When we supply proper person details,
    // it should insert the person into the person list,
    // and it should return an object of PersonResponse, 
    // which includes with the newly generated PersonID
    [Fact]
    public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
    {
        // Arrange
        PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone@example.com").Create();

        Person person = personAddRequest.ToPerson();
        PersonResponse personResponseExpected = person.ToPersonResponse();

        // If we supply any argument value to the AddPerson method,
        // it should return the same return value
        _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
            .ReturnsAsync(person);

        // Act
        PersonResponse personResponseFromAdd =
            await _personsAdderService.AddPerson(personAddRequest);

        personResponseExpected.ID = personResponseFromAdd.ID;

        // Fluent Assertion
        personResponseFromAdd.ID.Should().NotBe(Guid.Empty);

        personResponseFromAdd.Should().Be(personResponseExpected);
    }

    #endregion

    #region GetPersonById

    // When we supply null value as PersonID,
    // it should return null as PersonResponse
    [Fact]
    public async Task GetPersonById_NullPersonId_ToBeNull()
    {
        // Arrange
        Guid? personId = null;

        // Act
        PersonResponse? personResponseFromGet = await
            _personsGetterService.GetPersonById(personId);

        // Fluent Assertion
        personResponseFromGet.Should().BeNull();
    }

    // If we supply a valid PersonID,
    // it should return the VALID person details as 
    // PersonResponse object
    [Fact]
    public async Task GetPersonById_WithPersonID_ToBeSuccessful()
    {
        // Arrange
        Person person =
            _fixture.Build<Person>()
                .With(temp => temp.Email, "email@sample.com")
                .With(temp => temp.Country, null as Country).Create();

        PersonResponse personResponseExpected =
            person.ToPersonResponse();

        _personsRepositoryMock
            .Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>()))
            .ReturnsAsync(person);

        // Act
        PersonResponse? personResponseFromGet = await
            _personsGetterService.GetPersonById(person.Id);

        // Fluent Assertion
        personResponseFromGet.Should().Be(personResponseExpected);
    }

    #endregion

    #region GetAllPersons

    // The GetAllPersons method should return an
    // empty list by default 
    [Fact]
    public async Task GetAllPersons_EmptyList()
    {
        // Arrange 
        List<Person> persons = new();
        _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
            .ReturnsAsync(persons);

        //Act
        List<PersonResponse> personFromGet = await
            _personsGetterService.GetAllPersons();

        // Fluent Assertion
        personFromGet.Should().BeEmpty();
    }

    // First, we will add few persons; and then we call
    // GetAllPersons method, it should return the same
    // persons that were added
    [Fact]
    public async Task
        GetAllPersons_AddFewPersons_WithFewPersons_ToBeSuccessful()
    {
        // Arrange
        List<Person> persons = new()
        {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country).Create()
        };

        List<PersonResponse> personResponseListExpected =
            persons.Select(temp => temp.ToPersonResponse()).ToList();

        _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
            .ReturnsAsync(persons);

        // Act
        List<PersonResponse> personsListFromGet = await
            _personsGetterService.GetAllPersons();

        // Log expected responses
        _personTestHelper.LogPersonResponses("Expected: ",
            personResponseListExpected);

        // Log actual responses
        _personTestHelper.LogPersonResponses("Actual: ", personsListFromGet);

        // Fluent Assertion
        personsListFromGet.Should()
            .BeEquivalentTo(personResponseListExpected);
    }

    #endregion

    #region GetFilteredPersons

    // If the search text is empty and search by
    // is "PersonName", it should return all persons
    [Fact]
    public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
    {
        // Arrange
        List<Person> persons = new()
        {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country).Create()
        };

        _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
            .ReturnsAsync(persons);

        List<PersonResponse> personResponseListExpected =
            persons.Select(temp => temp.ToPersonResponse()).ToList();

        // Act
        List<PersonResponse> personsListFromSearch =
            await _personsGetterService.GetFilteredPersons(
                nameof(Person.PersonName),
                "");

        // Log actual responses
        _personTestHelper.LogPersonResponses("Actual: ", personsListFromSearch);

        // Assert
        _personTestHelper.AssertPersonResponsesInList(
            personResponseListExpected, personsListFromSearch);
    }


    // First we will add few persons; and then 
    // we will search based on person name with some
    // search string. It should return the matching persons
    [Fact]
    public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
    {
        // Arrange
        List<Person> persons = new()
        {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country).Create()
        };

        List<PersonResponse> personResponseListExpected = persons
            .Where(p =>
                p.PersonName.Contains("sa", StringComparison.OrdinalIgnoreCase))
            .Select(temp => temp.ToPersonResponse())
            .ToList();

        _personsRepositoryMock.Setup(temp =>
                temp.GetFilteredPersons(
                    It.IsAny<Expression<Func<Person, bool>>>()))
            .ReturnsAsync(persons.Where(p =>
                    p.PersonName.Contains("sa",
                        StringComparison.OrdinalIgnoreCase))
                .ToList());

        // Act 
        List<PersonResponse> personsListFromSearch =
            await _personsGetterService.GetFilteredPersons(
                nameof(Person.PersonName),
                "sa");

        // Log actual responses
        _personTestHelper.LogPersonResponses("Actual: ", personsListFromSearch);

        // Assert
        _personTestHelper.AssertPersonResponsesInList(
            personResponseListExpected, personsListFromSearch);
    }

    #endregion

    #region GetSortedPersons

    // When we sort based on PersonName in DESC,
    // it should return the persons in descending order
    [Fact]
    public async Task GetSortedPersons_ToBeSuccessful()
    {
        // Arrange
        List<Person> persons = new()
        {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country).Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country).Create()
        };

        _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
            .ReturnsAsync(persons);

        List<PersonResponse> personResponseListExpected =
            persons.Select(temp => temp.ToPersonResponse()).ToList();

        List<PersonResponse> allPersons =
            await _personsGetterService.GetAllPersons();

        // Act 
        List<PersonResponse> personsListFromSort = await
            _personsSorterService.GetSortedPersons(allPersons,
                nameof(Person.PersonName),
                SortOderOptions.DESC);

        // Log expected responses
        _personTestHelper.LogPersonResponses("Expected: ",
            personResponseListExpected.Where(p =>
                    p.PersonName.Contains("ma",
                        StringComparison.OrdinalIgnoreCase))
                .ToList());

        // Log actual responses
        _personTestHelper.LogPersonResponses("Actual: ", personsListFromSort);

        personResponseListExpected =
            personResponseListExpected
                .OrderByDescending(temp => temp.PersonName)
                .ToList();

        // Assert
        for (int i = 0; i < personResponseListExpected.Count; i++)
            Assert.Equal(personResponseListExpected[i], personsListFromSort[i]);
    }

    #endregion

    #region UpdatePerson

    // When we supply null value as PersonUpdateRequest,
    // it should throw ArgumentNullException

    [Fact]
    public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
    {
        // Arrange
        PersonUpdateRequest? personUpdateRequest = null;

        Func<Task> action = async () =>
        {
            // Act
            await _personsUpdaterService.UpdatePerson(personUpdateRequest);
        };

        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    // When we supply invalid person ID,
    // // it should throw ArgumentException

    [Fact]
    public async Task UpdatePerson_InvalidPersonId_ToBeArgumentException()
    {
        // Arrange
        PersonUpdateRequest? personUpdateRequest =
            _fixture.Build<PersonUpdateRequest>().Create();

        Func<Task> action = async () =>
        {
            // Act
            await _personsUpdaterService.UpdatePerson(personUpdateRequest);
        };

        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentException>();
    }

    // When PersonName is null
    // // it should throw ArgumentException

    [Fact]
    public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
    {
        // Arrange
        Person person = _fixture.Build<Person>()
            .With(temp => temp.PersonName, null as string)
            .With(temp => temp.Email, "someone@example.com")
            .With(temp => temp.Country, null as Country)
            .With(temp => temp.Gender, "Male").Create();

        PersonResponse personResponseFromAdd = person.ToPersonResponse();

        PersonUpdateRequest personUpdateRequest =
            personResponseFromAdd.ToPersonUpdateRequest();

        Func<Task> action = async () =>
        {
            // Act
            await _personsUpdaterService.UpdatePerson(personUpdateRequest);
        };

        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentException>();
    }

    // First, add a new person; then try to update the 
    // personName and Email

    [Fact]
    public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful()
    {
        // Arrange
        Person person = _fixture.Build<Person>()
            .With(temp => temp.Email, "someone@example.com")
            .With(temp => temp.Country, null as Country)
            .With(temp => temp.Gender, "Male").Create();

        PersonResponse personResponseExpected = person.ToPersonResponse();

        PersonUpdateRequest personUpdateRequest =
            personResponseExpected.ToPersonUpdateRequest();

        _personsRepositoryMock
            .Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
            .ReturnsAsync(person);

        _personsRepositoryMock
            .Setup(temp => temp.GetPersonByPersonId(It.IsAny<Guid>()))
            .ReturnsAsync(person);

        // Act
        PersonResponse personResponseFromUpdate = await
            _personsUpdaterService.UpdatePerson(personUpdateRequest);

        // Fluent Assertion
        personResponseFromUpdate.Should().Be(personResponseExpected);
    }

    #endregion

    #region DeletePerson

    // If you supply a valid PersonId, it should return true
    [Fact]
    public async Task DeletePerson_ValidPersonId_ToBeSuccessful()
    {
        // Arrange
        Person person = _fixture.Build<Person>()
            .With(temp => temp.PersonName, "Rahman")
            .With(temp => temp.Email, "someone_1@example.com")
            .With(temp => temp.Country, null as Country).Create();

        _personsRepositoryMock.Setup(temp =>
                temp.DeletePersonByPersonId(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        _personsRepositoryMock.Setup(temp =>
                temp.GetPersonByPersonId(It.IsAny<Guid>()))
            .ReturnsAsync(person);

        // Act
        bool isDeleted =
            await _personsDeleterService.DeletePerson(person.Id);

        // Fluent Assertion
        isDeleted.Should().BeTrue();
    }

    // If you supply an invalid PersonId, it should return false
    [Fact]
    public async Task DeletePerson_InvalidPersonId()
    {
        // Act
        bool isDeleted =
            await _personsDeleterService.DeletePerson(Guid.NewGuid());

        // Fluent Assertion
        isDeleted.Should().BeFalse();
    }

    #endregion
}
