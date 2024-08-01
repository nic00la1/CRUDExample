using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CRUDExample.Filters.ActionFilters;

public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
{
    private readonly ICountriesService _countriesService;

    public PersonCreateAndEditPostActionFilter(
        ICountriesService countriesService
    )
    {
        _countriesService = countriesService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context,
                                             ActionExecutionDelegate next
    )
    {
        // TO DO: before logic

        if (context.Controller is PersonsController personsController)
        {
            if (!personsController.ModelState.IsValid)
            {
                List<CountryResponse> countries = await
                    _countriesService.GetAllCountries();
                personsController.ViewBag.Countries = countries.Select(
                    country =>
                        new SelectListItem
                        {
                            Text = country.CountryName,
                            Value = country.CountryId.ToString()
                        }).ToList();

                personsController.ViewBag.Errors = personsController.ModelState
                    .Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage).ToList();

                object? personRequest =
                    context.ActionArguments["personRequest"];

                context.Result =
                    personsController.View(
                        personRequest); // Short-Circuits or skips the rest of the pipeline
            } else
                await next(); // invokes the subsequent filters and the action method
        } else
            await next(); // invokes the subsequent filters and the action method
        // TO DO: after logic
    }
}
