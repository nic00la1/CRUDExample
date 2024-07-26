using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesService : ICountriesService
{
    private readonly ApplicationDbContext _db;

    public CountriesService(ApplicationDbContext applicationDbContext
    )
    {
        _db = applicationDbContext;
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
        if (await _db
                .Countries.CountAsync(temp =>
                    temp.CountryName == countryAddRequest.CountryName) > 0)
            throw new ArgumentException(
                $"Country with name {countryAddRequest.CountryName} already exists");

        // Convert object from CountryAddRequest to Country type
        Country country = countryAddRequest.ToCountry();

        // Generate CountryId
        country.CountryId = Guid.NewGuid();

        // Add country object into _db list
        _db.Countries.Add(country);
        await _db.SaveChangesAsync();

        return country.ToCountryResponse();
    }

    public async Task<List<CountryResponse>> GetAllCountries()
    {
        return await _db.Countries
            .Select(country => country.ToCountryResponse())
            .ToListAsync();
    }

    public async Task<CountryResponse?> GetCountryByCountryId(Guid? countryId)
    {
        if (countryId == null)
            return null;

        Country? country_response_from_list = await
            _db.Countries.FirstOrDefaultAsync(temp =>
                temp.CountryId == countryId);

        if (country_response_from_list == null)
            return null;

        return country_response_from_list.ToCountryResponse();
    }

    public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
    {
        MemoryStream memoryStream = new();
        await formFile.CopyToAsync(memoryStream);
        int countriesInserted = 0;

        using (ExcelPackage excelPackage = new(memoryStream))
        {
            ExcelWorksheets? worksheets = excelPackage.Workbook.Worksheets;
            if (worksheets.Count == 0)
                throw new Exception(
                    "The uploaded file does not contain any worksheets. Please check the file and try again.");

            // Log available worksheet names
            List<string> worksheetNames =
                worksheets.Select(ws => ws.Name).ToList();
            Console.WriteLine("Available worksheets: " +
                string.Join(", ", worksheetNames));

            ExcelWorksheet workSheet = worksheets.FirstOrDefault(ws =>
                ws.Name.Equals("Countries",
                    StringComparison.OrdinalIgnoreCase));

            if (workSheet == null)
                throw new Exception(
                    "The uploaded file does not contain a worksheet named 'Countries'. Please check the file and try again.");

            int rowCount = workSheet.Dimension?.Rows ?? 0;

            for (int row = 2; row <= rowCount; row++)
            {
                string? cellValue =
                    Convert.ToString(workSheet.Cells[row, 1].Value);

                if (!string.IsNullOrEmpty(cellValue))
                {
                    string? countryName = cellValue;

                    if (_db.Countries
                            .Where(temp => temp.CountryName == countryName)
                            .Count() == 0)
                    {
                        Country country = new()
                        {
                            CountryName = countryName
                        };

                        _db.Countries.Add(country);
                        await _db.SaveChangesAsync();

                        countriesInserted++;
                    }
                }
            }
        }

        return countriesInserted;
    }
}
