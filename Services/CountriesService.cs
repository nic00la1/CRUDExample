using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesService : ICountriesService
{
    private readonly List<Country> _countries;

    public CountriesService(bool initialize = true)
    {
        _countries = new List<Country>();
        if (initialize)
            _countries.AddRange(new List<Country>()
            {
                new()
                {
                    CountryId =
                        Guid.Parse("F8B7F2A4-D571-44D9-A9C5-71F8165CB22C"),
                    CountryName = "India"
                },
                new()
                {
                    CountryId =
                        Guid.Parse("C35C19BD-01DD-4F3D-912C-F4C962646F7E"),
                    CountryName = "USA"
                },
                new()
                {
                    CountryId =
                        Guid.Parse("6340C2AD-A46F-4953-A318-A76CCEB19B1B"),
                    CountryName = "UK"
                },
                new()
                {
                    CountryId =
                        Guid.Parse("6CA1838D-346B-447A-A8C3-901A1B9147C6"),
                    CountryName = "Australia"
                },
                new()
                {
                    CountryId =
                        Guid.Parse("601C18BA-CC92-4D1E-9533-A97EE8C45A28"),
                    CountryName = "Canada"
                }
            });
    }

    public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
    {
        // Validation: countryAddRequest should not be null
        if (countryAddRequest == null)
            throw new ArgumentNullException(nameof(countryAddRequest));

        // Validation: CountryName can't be null
        if (countryAddRequest.CountryName == null)
            throw new ArgumentException(
                nameof(countryAddRequest.CountryName));

        // Validation: CountryName can't be duplicate
        if (_countries
                .Where(temp =>
                    temp.CountryName == countryAddRequest.CountryName).Count() >
            0)
            throw new ArgumentException(
                $"Country with name {countryAddRequest.CountryName} already exists");

        // Convert object from CountryAddRequest to Country type
        Country country = countryAddRequest.ToCountry();

        // Generate CountryId
        country.CountryId = Guid.NewGuid();

        // Add country object into _countries list
        _countries.Add(country);

        return country.ToCountryResponse();
    }

    public List<CountryResponse> GetAllCountries()
    {
        return _countries.Select(country => country.ToCountryResponse())
            .ToList();
    }

    public CountryResponse? GetCountryByCountryId(Guid? countryId)
    {
        if (countryId == null)
            return null;

        Country? country_response_from_list =
            _countries.FirstOrDefault(temp => temp.CountryId == countryId);

        if (country_response_from_list == null)
            return null;

        return country_response_from_list.ToCountryResponse();
    }
}
