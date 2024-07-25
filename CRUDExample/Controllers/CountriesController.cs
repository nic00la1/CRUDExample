using Microsoft.AspNetCore.Mvc;

namespace CRUDExample.Controllers;

[Route("[controller]")]
public class CountriesController : Controller
{
    [Route("[action]")]
    public IActionResult UploadFromExcel()
    {
        return View();
    }
}
