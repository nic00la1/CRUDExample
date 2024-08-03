using System.Collections;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Moq;
using RepositoryContracts;

namespace CRUDTests;

public class CountriesServiceTest
{
    private readonly ICountriesAdderService _countriesAdderService;
    private readonly ICountriesGetterService _countriesGetterService;
    private readonly ICountriesExcelService _countriesExcelService;

    private readonly Mock<ICountriesRepository> _countriesRepositoryMock;

    // constructor
    public CountriesServiceTest()
    {
        _countriesRepositoryMock = new Mock<ICountriesRepository>();
        _countriesAdderService =
            new CountriesAdderService(_countriesRepositoryMock.Object);

        _countriesGetterService =
            new CountriesGetterService(_countriesRepositoryMock.Object);

        _countriesExcelService =
            new CountriesExcelService(_countriesRepositoryMock.Object);
    }

    #region AddCountry

    // When CountryAddRequest is null,
    // AddCountry should throw an ArgumentNullException
    [Fact]
    public async Task AddCountry_NullCountry_ToBeArgumentNullException()
    {
        // Arrange
        CountryAddRequest? request = null;

        // Act
        Func<Task> action = async () =>
        {
            await _countriesAdderService.AddCountry(request);
        };

        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    // When the CountryName is null,
    // it should throw an ArgumentException
    [Fact]
    public async Task AddCountry_CountryNameIsNull_ToBeArgumentException()
    {
        // Arrange
        CountryAddRequest request = new()
        {
            CountryName = null
        };

        // Act
        Func<Task> action = async () =>
        {
            await _countriesAdderService.AddCountry(request);
        };

        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentException>();
    }

    // When the CountryName is duplicated,
    // it should throw an ArgumentException
    [Fact]
    public async Task AddCountry_DuplicateCountryName_ToBeArgumentException()
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

        _countriesRepositoryMock
            .Setup(repo => repo.GetCountryByCountryName("USA"))
            .ReturnsAsync(new Country { CountryName = "USA" });

        // Act
        Func<Task> action = async () =>
        {
            await _countriesAdderService.AddCountry(request1);
            await _countriesAdderService.AddCountry(request2);
        };
        // Fluent Assertion
        await action.Should().ThrowAsync<ArgumentException>();
    }

    // When you supply proper CountryName,
    // it should insert (Add) the country
    // to the existing list of countries
    [Fact]
    public async Task AddCountry_ProperCountryDetails_ToBeSuccessful()
    {
        // Arrange
        CountryAddRequest request = new()
        {
            CountryName = "Poland"
        };

        _countriesRepositoryMock
            .Setup(repo => repo.AddCountry(It.IsAny<Country>()))
            .ReturnsAsync(new Country
                { CountryId = Guid.NewGuid(), CountryName = "Poland" });


        // Act
        CountryResponse response =
            await _countriesAdderService.AddCountry(request);

        // Fluent Assertion
        response.CountryId.Should().NotBe(Guid.Empty);
        response.CountryName.Should().Be("Poland");
    }

    #endregion

    #region GetAllCountries

    // The list of countries should be empty
    // by default (before adding any countries)
    [Fact]
    public async Task GetAllCountries_EmptyList_ToBeSuccessful()
    {
        // Arrange
        _countriesRepositoryMock.Setup(repo => repo.GetAllCountries())
            .ReturnsAsync(new List<Country>());

        // Act 
        List<CountryResponse> actual_country_response_list = await
            _countriesGetterService.GetAllCountries();

        // Fluent Assertion
        actual_country_response_list.Should().BeEmpty();
    }

    // 
    [Fact]
    public async Task GetAllCountries_AddFewCountries_ToBeSuccessful()
    {
        // Arrange
        List<Country> countries = new()
        {
            new Country { CountryId = Guid.NewGuid(), CountryName = "USA" },
            new Country { CountryId = Guid.NewGuid(), CountryName = "Poland" },
            new Country { CountryId = Guid.NewGuid(), CountryName = "India" }
        };

        _countriesRepositoryMock.Setup(repo => repo.GetAllCountries())
            .ReturnsAsync(countries);

        // Act
        List<CountryResponse> actualCountryResponseList =
            await _countriesGetterService.GetAllCountries();

        // Fluent Assertion
        actualCountryResponseList.Should().HaveCount(3);
        actualCountryResponseList.Should().Contain(c => c.CountryName == "USA");
        actualCountryResponseList.Should()
            .Contain(c => c.CountryName == "Poland");
        actualCountryResponseList.Should()
            .Contain(c => c.CountryName == "India");
    }

    #endregion

    #region GetCountryById

    // If we supply null as CountryId, it should return null as CountryResponse
    [Fact]
    public async Task GetCountryById_NullCountryId_ToBeNull()
    {
        // Arrange
        Guid? countryId = null;

        // Act
        CountryResponse? country_response_from_get_method = await
            _countriesGetterService.GetCountryByCountryId(countryId);

        // Fluent Assertion
        country_response_from_get_method.Should().BeNull();
    }

    // If we supply a valid CountryId, it should return the matching 
    // country details as CountryResponse object
    [Fact]
    public async Task GetCountryById_ValidCountryId_ToBeSuccessful()
    {
        // Arrange
        Guid countryId = Guid.NewGuid();
        Country country = new()
            { CountryId = countryId, CountryName = "China" };

        _countriesRepositoryMock
            .Setup(repo => repo.GetCountryByCountryId(countryId))
            .ReturnsAsync(country);

        // Act
        CountryResponse? country_response_from_get =
            await _countriesGetterService.GetCountryByCountryId(countryId);

        // Fluent Assertion
        country_response_from_get.Should().NotBeNull();
        country_response_from_get.CountryId.Should().Be(countryId);
        country_response_from_get.CountryName.Should().Be("China");
    }

    #endregion
}
