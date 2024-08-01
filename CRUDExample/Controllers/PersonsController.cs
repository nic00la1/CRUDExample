using CRUDExample.Filters.ActionFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers;

[Route("[controller]")]
[TypeFilter(typeof(ResponseHeaderActionFilter),
    Arguments = new object[]
    {
        "My-Key-From-Controller", "My-Value-From-Controller", 3
    }, Order = 3)]
public class PersonsController : Controller
{
    private readonly IPersonService _personService;
    private readonly ICountriesService _countriesService;
    private readonly ILogger<PersonsController> _logger;

    public PersonsController(IPersonService personService,
                             ICountriesService countriesService,
                             ILogger<PersonsController> logger
    )
    {
        _personService = personService;
        _countriesService = countriesService;
        _logger = logger;
    }

    [Route("[action]")]
    [Route("/")]
    [TypeFilter(typeof(PersonsListActionFilter), Order = 4)]
    [TypeFilter(typeof(ResponseHeaderActionFilter),
        Arguments = new object[]
        {
            "MyKey-From-Action", "MyValue-From-Action", 1
        }, Order = 1)]
    public async Task<IActionResult> Index(string searchBy,
                                           string? searchString,
                                           string sortBy =
                                               nameof(PersonResponse
                                                   .PersonName),
                                           SortOderOptions sortOrder =
                                               SortOderOptions.ASC
    )
    {
        _logger.LogInformation("Index action method of PersonsController");

        _logger.LogDebug(
            $"searchBy: {searchBy}, searchString: {searchString}, sortBy: {sortBy}, sortOrder: {sortOrder}");

        // Search

        List<PersonResponse> persons = await
            _personService.GetFilteredPersons(searchBy, searchString);

        // Sort
        List<PersonResponse> sortedPersons = await
            _personService.GetSortedPersons(persons, sortBy, sortOrder);

        return View(sortedPersons);
    }

    // Executes when the user clicks on "Create Person" hyperlink
    // (while opening the create view)
    [Route("[action]")]
    [HttpGet]
    [TypeFilter(typeof(ResponseHeaderActionFilter),
        Arguments = new object[]
        {
            "my-key", "my-value"
        })]
    public async Task<IActionResult> Create()
    {
        List<CountryResponse> countries =
            await _countriesService.GetAllCountries();

        ViewBag.Countries = countries.Select(country => new SelectListItem
        {
            Text = country.CountryName,
            Value = country.CountryId.ToString()
        }).ToList();


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
    public async Task<IActionResult> Create(PersonAddRequest personAddRequest)
    {
        if (!ModelState.IsValid)
        {
            List<CountryResponse> countries = await
                _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(country => new SelectListItem
            {
                Text = country.CountryName,
                Value = country.CountryId.ToString()
            }).ToList();

            ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage).ToList();
            return
                View(personAddRequest); // Ensure to pass back the model to preserve user input
        }

        // Call the AddPerson method of the PersonService
        PersonResponse personResponse = await
            _personService.AddPerson(personAddRequest);


        // Redirect to the Index action of the PersonsController
        return RedirectToAction("Index", "Persons");
    }

    [HttpGet]
    [Route("[action]/{personID}")] // Eg: /persons/edit/1
    public async Task<IActionResult> Edit(Guid personID)
    {
        PersonResponse? personResponse =
            await _personService.GetPersonById(personID);

        if (personResponse == null) return RedirectToAction("Index");

        PersonUpdateRequest personUpdateRequest =
            personResponse.ToPersonUpdateRequest();

        List<CountryResponse> countries = await
            _countriesService.GetAllCountries();

        ViewBag.Countries = countries.Select(country => new SelectListItem
        {
            Text = country.CountryName,
            Value = country.CountryId.ToString()
        }).ToList();

        return View(personUpdateRequest);
    }

    [HttpPost]
    [Route("[action]/{personID}")]
    public async Task<IActionResult> Edit(
        PersonUpdateRequest personUpdateRequest
    )
    {
        PersonResponse? personResponse = await
            _personService.GetPersonById(personUpdateRequest.Id);

        if (personResponse == null) return RedirectToAction("Index");

        if (ModelState.IsValid)
        {
            PersonResponse updatedPerson = await
                _personService.UpdatePerson(personUpdateRequest);

            return RedirectToAction("Index");
        }

        List<CountryResponse> countries = await
            _countriesService.GetAllCountries();
        ViewBag.Countries = countries.Select(country => new SelectListItem
        {
            Text = country.CountryName,
            Value = country.CountryId.ToString()
        }).ToList();

        ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage).ToList();
        return
            View(personResponse
                .ToPersonUpdateRequest()); // Ensure to pass back the model to preserve user input
    }

    [HttpGet]
    [Route("[action]/{personID}")]
    public async Task<IActionResult> Delete(Guid? personID)
    {
        PersonResponse? personResponse =
            await _personService.GetPersonById(personID);

        if (personResponse == null) return RedirectToAction("Index");

        return View(personResponse);
    }

    [HttpPost]
    [Route("[action]/{personID}")]
    public async Task<IActionResult> Delete(
        PersonUpdateRequest personUpdateRequest
    )
    {
        PersonResponse? personResponse = await
            _personService.GetPersonById(personUpdateRequest.Id);

        if (personResponse == null) return RedirectToAction("Index");

        await _personService.DeletePerson(personUpdateRequest
            .Id); // Added await here
        return RedirectToAction("Index");
    }

    [Route("[action]")]
    public async Task<IActionResult> PersonsPDF()
    {
        // Get list of persons
        List<PersonResponse> persons = await _personService.GetAllPersons();

        // Return view as pdf
        return new ViewAsPdf("PersonsPDF", persons, ViewData)
        {
            PageMargins = new Margins(20, 20, 20, 20),
            PageOrientation = Orientation.Landscape
        };
    }

    [Route("[action]")]
    public async Task<IActionResult> PersonsCSV()
    {
        MemoryStream memoryStream = await _personService.GetPersonCSV();

        return File(memoryStream, "application/octet-stream", "persons.csv");
    }

    [Route("[action]")]
    public async Task<IActionResult> PersonsExcel()
    {
        MemoryStream memoryStream = await _personService.GetPersonExcel();

        return File(memoryStream, "application/octet-stream", "persons.xlsx");
    }
}
