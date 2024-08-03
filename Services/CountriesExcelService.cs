using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesExcelService : ICountriesExcelService
{
    private readonly ICountriesRepository _countriesRepository;

    public CountriesExcelService(ICountriesRepository countriesRepository
    )
    {
        _countriesRepository = countriesRepository;
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

                    if (await _countriesRepository.GetCountryByCountryName(
                            countryName) == null)
                    {
                        Country country = new()
                        {
                            CountryName = countryName
                        };

                        await _countriesRepository.AddCountry(country);

                        countriesInserted++;
                    }
                }
            }
        }

        return countriesInserted;
    }
}
