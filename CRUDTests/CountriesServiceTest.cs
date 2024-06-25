using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace CRUDTests;

public class CountriesServiceTest
{
    private readonly ICountriesService _countriesService;

    public CountriesServiceTest()
    {
        _countriesService = new CountriesService();
    }

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

        // Assert
        Assert.True(response.CountryId != Guid.Empty);
    }
}
