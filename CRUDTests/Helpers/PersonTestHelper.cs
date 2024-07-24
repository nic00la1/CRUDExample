using ServiceContracts.DTO;
using ServiceContracts.Enums;
using ServiceContracts;
using Xunit.Abstractions;

namespace CRUDTests.Helpers;

public class PersonTestHelper
{
    private readonly IPersonService _personService;
    private readonly ICountriesService _countriesService;
    private readonly ITestOutputHelper _testOutputHelper;

    public PersonTestHelper(IPersonService personService,
                            ICountriesService countriesService,
                            ITestOutputHelper testOutputHelper
    )
    {
        _personService = personService;
        _countriesService = countriesService;
        _testOutputHelper = testOutputHelper;
    }

    public async Task<PersonResponse> AddPersonAndReturnResponse(
        PersonAddRequest request
    )
    {
        return await _personService.AddPerson(request);
    }

    public PersonAddRequest CreatePersonAddRequest(
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

    public async Task<List<PersonAddRequest>> CreatePersonRequests()
    {
        CountryResponse countryResponse1 = await
            _countriesService.AddCountry(new CountryAddRequest
                { CountryName = "Poland" });
        CountryResponse countryResponse2 = await
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

    public async Task<List<PersonResponse>> AddPersonsAndReturnResponses(
        List<PersonAddRequest> personRequests
    )
    {
        List<Task<PersonResponse>> personResponseTasks = personRequests
            .Select(request => _personService.AddPerson(request)).ToList();

        List<PersonResponse> personResponses =
            (await Task.WhenAll(personResponseTasks)).ToList();

        LogPersonResponses("Expected: ", personResponses);
        return personResponses;
    }

    public void LogPersonResponses(string message,
                                   List<PersonResponse> personResponses
    )
    {
        _testOutputHelper.WriteLine(message);
        foreach (PersonResponse personResponse in personResponses)
            _testOutputHelper.WriteLine(personResponse.ToString());
    }

    public void AssertPersonResponsesInList(
        List<PersonResponse> expectedResponses,
        List<PersonResponse> actualResponses
    )
    {
        foreach (PersonResponse expectedResponse in expectedResponses)
            Assert.Contains(expectedResponse, actualResponses);
    }

    public async Task<PersonResponse> CreateAndAddPerson(string name,
            string email,
            string address,
            string countryName,
            DateTime dateOfBirth,
            GenderOptions gender,
            bool receiveNewsLetters
    )
    {
        CountryResponse countryResponse = await
            _countriesService.AddCountry(new CountryAddRequest
                { CountryName = countryName });
        PersonAddRequest personRequest = CreatePersonAddRequest(name, email,
            address, countryResponse.CountryId, dateOfBirth, gender,
            receiveNewsLetters);
        return await _personService.AddPerson(personRequest);
    }
}
