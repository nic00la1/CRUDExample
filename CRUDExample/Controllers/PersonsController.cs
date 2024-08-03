using CRUDExample.Filters;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.AuthorizationFilters;
using CRUDExample.Filters.ExceptionFilters;
using CRUDExample.Filters.ResourceFilters;
using CRUDExample.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using X.PagedList;
using X.PagedList.Extensions;

namespace CRUDExample.Controllers;

[Route("[controller]")]
[ResponseHeaderFilterFactory("My-Key-From-Controller",
    "My-Value-From-Controller", 3)]
[TypeFilter(typeof(HandleExceptionFilter))]
[TypeFilter(typeof(PersonsAlwaysRunResultFilter))]
public class PersonsController : Controller
{
    private readonly ICountriesAdderService _countriesAdderService;
    private readonly ICountriesGetterService _countriesGetterService;
    private readonly ICountriesExcelService _countriesExcelService;
    private readonly ILogger<PersonsController> _logger;
    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsUpdaterService _personsUpdaterService;
    private readonly IPersonsDeleterService _personsDeleterService;
    private readonly IPersonsGetterService _personsGetterService;
    private readonly IPersonsSorterService _personsSorterService;

    public PersonsController(
        ICountriesAdderService countriesAdderService,
        ICountriesGetterService countriesGetterService,
        ICountriesExcelService countriesExcelService,
        ILogger<PersonsController> logger,
        IPersonsAdderService personsAdderService,
        IPersonsUpdaterService personsUpdaterService,
        IPersonsDeleterService personsDeleterService,
        IPersonsGetterService personsGetterService,
        IPersonsSorterService personsSorterService
    )
    {
        _countriesAdderService = countriesAdderService;
        _countriesGetterService = countriesGetterService;
        _countriesExcelService = countriesExcelService;
        _personsAdderService = personsAdderService;
        _personsUpdaterService = personsUpdaterService;
        _personsDeleterService = personsDeleterService;
        _personsGetterService = personsGetterService;
        _personsSorterService = personsSorterService;
        _logger = logger;
    }

    [Route("[action]")]
    [Route("/")]
    [ServiceFilter(typeof(PersonsListActionFilter), Order = 4)]
    [ResponseHeaderFilterFactory("MyKey-From-Action", "MyValue-From-Action", 1)]
    [TypeFilter(typeof(PersonsListResultFilter))]
    [SkipFilter]
    public async Task<IActionResult> Index(string searchBy,
                                           string? searchString,
                                           string sortBy =
                                               nameof(PersonResponse
                                                   .PersonName),
                                           SortOderOptions sortOrder =
                                               SortOderOptions.ASC,
                                           int page = 1,
                                           int pageSize = 5
    )
    {
        _logger.LogInformation("Index action method of PersonsController");

        _logger.LogDebug(
            $"searchBy: {searchBy}, searchString: {searchString}, sortBy: {sortBy}, sortOrder: {sortOrder}");

        // Search
        List<PersonResponse> persons =
            await _personsGetterService.GetFilteredPersons(searchBy,
                searchString);

        // Sort
        List<PersonResponse> sortedPersons = await
            _personsSorterService.GetSortedPersons(persons, sortBy, sortOrder);

        // Paginate
        int currentPageSize = pageSize; // Default page size
        int pageNumber = page;

        IPagedList<PersonResponse> pagedPersons =
            sortedPersons.ToPagedList(pageNumber, currentPageSize);

        ViewBag.CurrentPageSize = currentPageSize;
        ViewBag.SearchBy = searchBy;
        ViewBag.SearchString = searchString;
        return View(pagedPersons);
    }

    // Executes when the user clicks on "Create Person" hyperlink
    // (while opening the create view)
    [Route("[action]")]
    [HttpGet]
    [ResponseHeaderFilterFactory("MyKey-From-Action", "MyValue-From-Action", 4)]
    public async Task<IActionResult> Create()
    {
        List<CountryResponse> countries =
            await _countriesGetterService.GetAllCountries();

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
    [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
    [TypeFilter(typeof(FeatureDisabledResourceFilter),
        Arguments = new object[] { false })]
    public async Task<IActionResult> Create(PersonAddRequest personRequest)
    {
        if (!ModelState.IsValid)
        {
            List<CountryResponse> countries = await
                _countriesGetterService.GetAllCountries();
            ViewBag.Countries = countries.Select(country => new SelectListItem
            {
                Text = country.CountryName,
                Value = country.CountryId.ToString()
            }).ToList();

            ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage).ToList();
            return
                View(personRequest); // Ensure to pass back the model to preserve user input
        }

        // Call the AddPerson method of the PersonService
        PersonResponse personResponse = await
            _personsAdderService.AddPerson(personRequest);


        // Redirect to the Index action of the PersonsController
        return RedirectToAction("Index", "Persons");
    }

    [HttpGet]
    [Route("[action]/{personID}")] // Eg: /persons/edit/1
    // [TypeFilter(typeof(TokenResultFilter))]
    public async Task<IActionResult> Edit(Guid personID)
    {
        PersonResponse? personResponse =
            await _personsGetterService.GetPersonById(personID);

        if (personResponse == null) return RedirectToAction("Index");

        PersonUpdateRequest personUpdateRequest =
            personResponse.ToPersonUpdateRequest();

        List<CountryResponse> countries = await
            _countriesGetterService.GetAllCountries();

        ViewBag.Countries = countries.Select(country => new SelectListItem
        {
            Text = country.CountryName,
            Value = country.CountryId.ToString()
        }).ToList();

        return View(personUpdateRequest);
    }

    [HttpPost]
    [Route("[action]/{personID}")]
    [TypeFilter(typeof(PersonCreateAndEditPostActionFilter))]
    [TypeFilter(typeof(TokenAuthorizationFilter))]
    public async Task<IActionResult> Edit(
        PersonUpdateRequest personRequest
    )
    {
        PersonResponse? personResponse = await
            _personsGetterService.GetPersonById(personRequest.Id);

        if (personResponse == null) return RedirectToAction("Index");

        PersonResponse updatedPerson = await
            _personsUpdaterService.UpdatePerson(personRequest);

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Route("[action]/{personID}")]
    public async Task<IActionResult> Delete(Guid? personID)
    {
        PersonResponse? personResponse =
            await _personsGetterService.GetPersonById(personID);

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
            _personsGetterService.GetPersonById(personUpdateRequest.Id);

        if (personResponse == null) return RedirectToAction("Index");

        await _personsDeleterService.DeletePerson(personUpdateRequest
            .Id); // Added await here
        return RedirectToAction("Index");
    }

    [Route("[action]")]
    public async Task<IActionResult> PersonsPDF()
    {
        // Get list of persons
        List<PersonResponse> persons =
            await _personsGetterService.GetAllPersons();

        // Sort persons by name in ascending order
        persons = persons.OrderBy(p => p.PersonName).ToList();


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
        MemoryStream memoryStream = await _personsGetterService.GetPersonCSV();

        return File(memoryStream, "application/octet-stream", "persons.csv");
    }

    [Route("[action]")]
    public async Task<IActionResult> PersonsExcel()
    {
        MemoryStream memoryStream =
            await _personsGetterService.GetPersonExcel();

        return File(memoryStream, "application/octet-stream", "persons.xlsx");
    }
}
