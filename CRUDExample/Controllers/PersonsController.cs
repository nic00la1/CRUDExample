using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers;

[Route("[controller]")]
public class PersonsController : Controller
{
    private readonly IPersonService _personService;
    private readonly ICountriesService _countriesService;

    public PersonsController(IPersonService personService,
                             ICountriesService countriesService
    )
    {
        _personService = personService;
        _countriesService = countriesService;
    }

    [Route("[action]")]
    [Route("/")]
    public IActionResult Index(string searchBy,
                               string? searchString,
                               string sortBy = nameof(PersonResponse.Name),
                               SortOderOptions sortOrder = SortOderOptions.ASC
    )
    {
        // Search
        ViewBag.SearchFields = new Dictionary<string, string>()
        {
            { nameof(PersonResponse.Name), "Person Name" },
            { nameof(PersonResponse.Email), "Email" },
            { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
            { nameof(PersonResponse.Gender), "Gender" },
            { nameof(PersonResponse.CountryId), "Country" },
            { nameof(PersonResponse.Address), "Address" }
        };

        List<PersonResponse> persons =
            _personService.GetFilteredPersons(searchBy, searchString);

        ViewBag.CurrentSearchBy = searchBy;
        ViewBag.CurrentSearchString = searchString;


        // Sort
        List<PersonResponse> sortedPersons =
            _personService.GetSortedPersons(persons, sortBy, sortOrder);

        ViewBag.CurrentSortBy = sortBy;
        ViewBag.CurrentSortOrder = sortOrder.ToString();

        return View(sortedPersons);
    }

    // Executes when the user clicks on "Create Person" hyperlink
    // (while opening the create view)
    [Route("[action]")]
    [HttpGet]
    public IActionResult Create()
    {
        List<CountryResponse> countries = _countriesService.GetAllCountries();
        ViewBag.Countries = countries.Select(c => new SelectListItem()
            { Text = c.CountryName, Value = c.CountryId.ToString() });


        //new SelectListItem()
        //{
        //    Text = "Nicola",
        //    Value = "1"
        //};

        // <option value="1">Nicola</option>
        return View();
    }

    [HttpPost]
    [Route("[action]")]
    public IActionResult Create(PersonAddRequest personAddRequest)
    {
        if (!ModelState.IsValid)
        {
            List<CountryResponse> countries =
                _countriesService.GetAllCountries();
            ViewBag.Countries = countries;

            ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage).ToList();
            return View();
        }

        // Call the AddPerson method of the PersonService
        PersonResponse personResponse =
            _personService.AddPerson(personAddRequest);


        // Redirect to the Index action of the PersonsController
        return RedirectToAction("Index", "Persons");
    }
}
