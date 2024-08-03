using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesAdderService : ICountriesAdderService
{
    private readonly ICountriesRepository _countriesRepository;

    public CountriesAdderService(ICountriesRepository countriesRepository
    )
    {
        _countriesRepository = countriesRepository;
    }

    public async Task<CountryResponse> AddCountry(
        CountryAddRequest? countryAddRequest
    )
    {
        // Validation: countryAddRequest should not be null
        if (countryAddRequest == null)
            throw new ArgumentNullException(nameof(countryAddRequest));

        // Validation: CountryName can't be null
        if (countryAddRequest.CountryName == null)
            throw new ArgumentException(
                nameof(countryAddRequest.CountryName));

        // Validation: CountryName can't be duplicate
        if (await _countriesRepository.GetCountryByCountryName(countryAddRequest
                .CountryName) != null)
            throw new ArgumentException(
                $"Country with name {countryAddRequest.CountryName} already exists");

        // Convert object from CountryAddRequest to Country type
        Country country = countryAddRequest.ToCountry();

        // Generate CountryId
        country.CountryId = Guid.NewGuid();

        // Add country object into _countriesRepository list
        await _countriesRepository.AddCountry(country);

        return country.ToCountryResponse();
    }
}
