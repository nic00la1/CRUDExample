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
    public async Task<IActionResult> Index(string searchBy,
                                           string? searchString,
                                           string sortBy =
                                               nameof(PersonResponse
                                                   .PersonName),
                                           SortOderOptions sortOrder =
                                               SortOderOptions.ASC
    )
    {
        // Search
        ViewBag.SearchFields = new Dictionary<string, string>()
        {
            { nameof(PersonResponse.PersonName), "Person Name" },
            { nameof(PersonResponse.Email), "Email" },
            { nameof(PersonResponse.DateOfBirth), "Date of Birth" },
            { nameof(PersonResponse.Gender), "Gender" },
            { nameof(PersonResponse.CountryId), "Country" },
            { nameof(PersonResponse.Address), "Address" }
        };

        List<PersonResponse> persons = await
            _personService.GetFilteredPersons(searchBy, searchString);

        ViewBag.CurrentSearchBy = searchBy;
        ViewBag.CurrentSearchString = searchString;


        // Sort
        List<PersonResponse> sortedPersons = await
            _personService.GetSortedPersons(persons, sortBy, sortOrder);

        ViewBag.CurrentSortBy = sortBy;
        ViewBag.CurrentSortOrder = sortOrder.ToString();

        return View(sortedPersons);
    }

    // Executes when the user clicks on "Create Person" hyperlink
    // (while opening the create view)
    [Route("[action]")]
    [HttpGet]
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

        _personService.DeletePerson(personUpdateRequest.Id);
        return RedirectToAction("Index");
    }
}
