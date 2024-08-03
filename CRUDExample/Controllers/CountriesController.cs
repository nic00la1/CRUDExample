using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace CRUDExample.Controllers;

[Route("[controller]")]
public class CountriesController : Controller
{
    private readonly ICountriesExcelService _countriesExcelService;

    public CountriesController(
        ICountriesExcelService countriesExcelService
    )
    {
        _countriesExcelService = countriesExcelService;
    }

    [Route("[action]")]
    public IActionResult UploadFromExcel()
    {
        return View();
    }

    [HttpPost]
    [Route("[action]")]
    public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            ViewBag.ErrorMessage = "Please select an xlsx file to upload";
            return View();
        }

        if (!Path.GetExtension(excelFile.FileName)
                .Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.ErrorMessage = "Unsupported file. 'xlsx' file is expected";
            return View();
        }

        int countriesCountInserted =
            await _countriesExcelService.UploadCountriesFromExcelFile(
                excelFile);

        ViewBag.Message =
            $"{countriesCountInserted} countries uploaded successfully";

        return View();
    }
}
