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
    // it should insert the person into the person list
    // and it should return an object of PersonResponse, 
    // which includes with the newly generated PersonID
    [Fact]
    public void AddPerson_ProperPersonDetails()
    {
        // Arrange
        PersonAddRequest? personAddRequest = new()
        {
            Name = "Nicola Kaleta",
            Email = "nicola.kaleta@test.com",
            Address = "sample address",
            CountryID = Guid.NewGuid(),
            Gender = GenderOptions.Female,
            DateOfBirth = DateTime.Parse("2006-08-16"),
            ReceiveNewsLetters = true
        };

        // Act
        PersonResponse person_response_from_add =
            _personService.AddPerson(personAddRequest);

        List<PersonResponse> person_list = _personService.GetAllPersons();

        // Assert
        Assert.True(person_response_from_add.ID != Guid.Empty);

        Assert.Contains(person_response_from_add, person_list);
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
        PersonResponse? person_response_from_get =
            _personService.GetPersonById(personId);

        // Assert
        Assert.Null(person_response_from_get);
    }

    // If we supply a valid PersonID,
    // it should return the VALID person details as 
    // PersonResponse object
    [Fact]
    public void GetPersonById_WithPersonID()
    {
        // Arrange
        CountryAddRequest country_request = new()
        {
            CountryName = "Poland"
        };

        CountryResponse country_response =
            _countriesService.AddCountry(country_request);

        // Act
        PersonAddRequest person_request = new()
        {
            Name = "Nicola Kaleta",
            Email = "email@sample.com",
            Address = "address",
            CountryID = country_response.CountryId,
            DateOfBirth = DateTime.Parse("2000-08-16"),
            Gender = GenderOptions.Female,
            ReceiveNewsLetters = false
        };

        PersonResponse person_respone_from_add =
            _personService.AddPerson(person_request);

        PersonResponse? person_response_from_get =
            _personService.GetPersonById(person_respone_from_add.ID);

        // Assert
        Assert.Equal(person_respone_from_add, person_response_from_get);
    }

    #endregion

    #region GetAllPersons

    // The GetAllPersons method should return an
    // empty list by deafult 
    [Fact]
    public void GetAllPersons_EmptyList()
    {
        //Act
        List<PersonResponse> person_from_get =
            _personService.GetAllPersons();

        //Assert
        Assert.Empty(person_from_get);
    }

    // First, we will add few persons; and tthen we call
    // GetAllPersons method, it should return the same
    // persons that were added
    [Fact]
    public void GetAllPersons_AddFewPersons()
    {
        //Arrange
        CountryAddRequest country_request_1 = new()
        {
            CountryName = "Poland"
        };
        CountryAddRequest country_request_2 = new()
        {
            CountryName = "Germany"
        };

        CountryResponse country_response_1 =
            _countriesService.AddCountry(country_request_1);

        CountryResponse country_response_2 =
            _countriesService.AddCountry(country_request_2);

        PersonAddRequest person_request_1 = new()
        {
            Name = "Smith",
            Email = "smith@example.com",
            Address = "address",
            CountryID = country_response_1.CountryId,
            DateOfBirth = DateTime.Parse("1999-04-20"),
            Gender = GenderOptions.Male,
            ReceiveNewsLetters = true
        };

        PersonAddRequest person_request_2 = new()
        {
            Name = "John",
            Email = "john@gmail.com",
            Address = "address",
            CountryID = country_response_2.CountryId,
            DateOfBirth = DateTime.Parse("1998-04-20"),
            Gender = GenderOptions.Male,
            ReceiveNewsLetters = false
        };

        PersonAddRequest person_request_3 = new()
        {
            Name = "Hannah",
            Email = "Hannah@gmail.com",
            Address = "address",
            CountryID = country_response_2.CountryId,
            DateOfBirth = DateTime.Parse("2007-02-3"),
            Gender = GenderOptions.Female,
            ReceiveNewsLetters = true
        };

        List<PersonAddRequest> person_requests = new()
        {
            person_request_1,
            person_request_2,
            person_request_3
        };

        List<PersonResponse> person_response_list_from_add = new();

        foreach (PersonAddRequest person_request in person_requests)
        {
            PersonResponse person_response =
                _personService.AddPerson(person_request);

            person_response_list_from_add.Add(person_response);
        }

        // print the person_response_list_from_add
        _testOutputHelper.WriteLine("Expected: ");

        foreach (PersonResponse person_response_from_add in
                 person_response_list_from_add)
            _testOutputHelper.WriteLine(person_response_from_add.ToString());

        // Act 
        List<PersonResponse> persons_list_from_get =
            _personService.GetAllPersons();

        // print the persons_list_from_get
        _testOutputHelper.WriteLine("Actual: ");
        foreach (PersonResponse person_response_from_get in
                 persons_list_from_get
                )
            _testOutputHelper.WriteLine(person_response_from_get.ToString());

        // Assert
        foreach (PersonResponse person_response_from_add in
                 person_response_list_from_add)
            Assert.Contains(person_response_from_add, persons_list_from_get);
    }

    #endregion
}
