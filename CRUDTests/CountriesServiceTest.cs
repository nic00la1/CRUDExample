using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace CRUDTests;

public class CountriesServiceTest
{
    private readonly ICountriesService _countriesService;
    private readonly PersonsDbContext _dbContext;

    public CountriesServiceTest()
    {
        // Create an in-memory database context for testing
        DbContextOptions<PersonsDbContext> options =
            new DbContextOptionsBuilder<PersonsDbContext>()
                .UseInMemoryDatabase("TestDatabase")
                .Options;

        _dbContext = new PersonsDbContext(options);

        _countriesService = new CountriesService(_dbContext);
    }

    #region AddCountry

    // When CountryAddRequest is null,
    // AddCountry should throw an ArgumentNullException
    [Fact]
    public void AddCountry_NullCountry()
    {
        // Arrange
        CountryAddRequest? request = null;

        // Assert
        Assert.Throws<ArgumentNullException>(() =>
        {
            // Act
            _countriesService.AddCountry(request);
        });
    }

    // When the CountryName is null,
    // it should throw an ArgumentException
    [Fact]
    public void AddCountry_CountryNameIsNull()
    {
        // Arrange
        CountryAddRequest request = new()
        {
            CountryName = null
        };

        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            // Act
            _countriesService.AddCountry(request);
        });
    }

    // When the CountryName is duplicated,
    // it should throw an ArgumentException
    [Fact]
    public void AddCountry_DuplicateCountryName()
    {
        // Arrange
        CountryAddRequest request1 = new()
        {
            CountryName = "USA"
        };
        CountryAddRequest request2 = new()
        {
            CountryName = "USA"
        };

        // Assert
        Assert.Throws<ArgumentException>(() =>
        {
            // Act
            _countriesService.AddCountry(request1);
            _countriesService.AddCountry(request2);
        });
    }

    // When you supply proper CountryName,
    // it should insert (Add) the country
    // to the existing list of countries
    [Fact]
    public void AddCountry_ProperCountryDetails()
    {
        // Arrange
        CountryAddRequest request = new()
        {
            CountryName = "Poland"
        };

        // Act
        CountryResponse response = _countriesService.AddCountry(request);

        List<CountryResponse> countries_from_GetAllCountries =
            _countriesService.GetAllCountries();

        // Assert
        Assert.True(response.CountryId != Guid.Empty);
        Assert.Contains(response, countries_from_GetAllCountries);
    }

    #endregion

    #region GetAllCountries

    // The list of countries should be empty
    // by default (before adding any countries)
    [Fact]
    public void GetAllCountries_EmptyList()
    {
        // Act 
        List<CountryResponse> actual_country_response_list =
            _countriesService.GetAllCountries();

        // Assert
        Assert.Empty(actual_country_response_list);
    }

    // 
    [Fact]
    public void GetAllCountries_AddFewCountries()
    {
        // Arrange
        List<CountryAddRequest> country_request_list = new()
        {
            new CountryAddRequest { CountryName = "USA" },
            new CountryAddRequest { CountryName = "Poland" },
            new CountryAddRequest { CountryName = "India" }
        };

        // Act
        List<CountryResponse> countries_list_from_add_country = new();
        foreach (CountryAddRequest country_request in country_request_list)
            countries_list_from_add_country.Add(
                _countriesService.AddCountry(country_request));

        List<CountryResponse> actualCountryResponseList =
            _countriesService.GetAllCountries();

        // Read each element from the list
        foreach (CountryResponse expected_country in
                 countries_list_from_add_country)
            // Assert
            Assert.Contains(expected_country, actualCountryResponseList);
    }

    #endregion

    #region GetCountryById

    // If we supply null as CountryId, it should return null as CountryResponse
    [Fact]
    public void GetCountryById_NullCountryId()
    {
        // Arrange
        Guid? countryId = null;

        // Act
        CountryResponse? country_response_from_get_method =
            _countriesService.GetCountryByCountryId(countryId);

        // Assert
        Assert.Null(country_response_from_get_method);
    }

    // If we supply a valid CountryId, it should return the matching 
    // country details as CountryResponse object
    [Fact]
    public void GetCountryById_ValidCountryId()
    {
        // Arrange
        CountryAddRequest? country_add_request = new()
        {
            CountryName = "China"
        };

        CountryResponse country_response_from_add =
            _countriesService.AddCountry(country_add_request);

        // Act
        CountryResponse? country_response_from_get =
            _countriesService.GetCountryByCountryId(country_response_from_add
                .CountryId);

        // Assert
        Assert.Equal(country_response_from_add, country_response_from_get);
    }

    #endregion
}
