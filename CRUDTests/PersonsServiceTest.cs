using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;

namespace CRUDTests;

public class PersonsServiceTest
{
    private readonly IPersonService _personService;
    private readonly ICountriesService _countriesService;

    public PersonsServiceTest()
    {
        _personService = new PersonsService();
        _countriesService = new CountriesService();
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

        #endregion
    }
}
