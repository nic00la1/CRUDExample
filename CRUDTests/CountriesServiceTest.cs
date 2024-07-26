using System.Collections;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Moq;

namespace CRUDTests;

public class CountriesServiceTest
{
    private readonly ICountriesService _countriesService;

    // constructor
    public CountriesServiceTest()
    {
        List<Country> countriesInitialData = new() { };

        DbContextMock<ApplicationDbContext> dbContextMock =
            new(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

        ApplicationDbContext dbContext = dbContextMock.Object;
        dbContextMock.CreateDbSetMock(temp => temp.Countries,
            countriesInitialData);

        _countriesService = new CountriesService(null);
    }

    #region AddCountry

    // When CountryAddRequest is null,
    // AddCountry should throw an ArgumentNullException
    [Fact]
    public async Task AddCountry_NullCountry()
    {
        // Arrange
        CountryAddRequest? request = null;

        // Act
        Func<Task> action = async () =>
        {
            await _countriesService.AddCountry(request);
        };

        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    // When the CountryName is null,
    // it should throw an ArgumentException
    [Fact]
    public async Task AddCountry_CountryNameIsNull()
    {
        // Arrange
        CountryAddRequest request = new()
        {
            CountryName = null
        };

        // Act
        Func<Task> action = async () =>
        {
            await _countriesService.AddCountry(request);
        };

        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentException>();
    }

    // When the CountryName is duplicated,
    // it should throw an ArgumentException
    [Fact]
    public async Task AddCountry_DuplicateCountryName()
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

        // Act
        Func<Task> action = async () =>
        {
            await _countriesService.AddCountry(request1);
            await _countriesService.AddCountry(request2);
        };
        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentException>();
    }

    // When you supply proper CountryName,
    // it should insert (Add) the country
    // to the existing list of countries
    [Fact]
    public async Task AddCountry_ProperCountryDetails()
    {
        // Arrange
        CountryAddRequest request = new()
        {
            CountryName = "Poland"
        };

        // Act
        CountryResponse response = await _countriesService.AddCountry(request);

        List<CountryResponse> countries_from_GetAllCountries = await
            _countriesService.GetAllCountries();

        // Fluent Assertion
        response.CountryId.Should().NotBe(Guid.Empty);
        countries_from_GetAllCountries.Should().Contain(response);
    }

    #endregion

    #region GetAllCountries

    // The list of countries should be empty
    // by default (before adding any countries)
    [Fact]
    public async Task GetAllCountries_EmptyList()
    {
        // Act 
        List<CountryResponse> actual_country_response_list = await
            _countriesService.GetAllCountries();

        // Fluent Assertion
        actual_country_response_list.Should().BeEmpty();
    }

    // 
    [Fact]
    public async Task GetAllCountries_AddFewCountries()
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
                await _countriesService.AddCountry(country_request));

        List<CountryResponse> actualCountryResponseList = await
            _countriesService.GetAllCountries();

        // Read each element from the list
        foreach (CountryResponse expected_country in
                 countries_list_from_add_country)
            // Fluent Assertion
            actualCountryResponseList.Should().Contain(expected_country);
    }

    #endregion

    #region GetCountryById

    // If we supply null as CountryId, it should return null as CountryResponse
    [Fact]
    public async Task GetCountryById_NullCountryId()
    {
        // Arrange
        Guid? countryId = null;

        // Act
        CountryResponse? country_response_from_get_method = await
            _countriesService.GetCountryByCountryId(countryId);

        // Fluent Assertion
        country_response_from_get_method.Should().BeNull();
    }

    // If we supply a valid CountryId, it should return the matching 
    // country details as CountryResponse object
    [Fact]
    public async Task GetCountryById_ValidCountryId()
    {
        // Arrange
        CountryAddRequest? country_add_request = new()
        {
            CountryName = "China"
        };

        CountryResponse country_response_from_add = await
            _countriesService.AddCountry(country_add_request);

        // Act
        CountryResponse? country_response_from_get = await
            _countriesService.GetCountryByCountryId(country_response_from_add
                .CountryId);

        // Fluent Assertion
        country_response_from_get.Should().Be(country_response_from_add);
    }

    #endregion
}
