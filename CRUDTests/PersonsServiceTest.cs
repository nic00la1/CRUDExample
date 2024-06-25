using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;

namespace CRUDTests;

public class PersonsServiceTest
{
    private readonly IPersonService _personService;

    public PersonsServiceTest()
    {
        _personService = new PersonsService();
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
}
