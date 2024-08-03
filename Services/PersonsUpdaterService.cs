using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Entities;
using Microsoft.VisualBasic;
using ServiceContracts;
using ServiceContracts.DTO;
using Services.Helpers;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using Serilog;
using SerilogTimings;
using ServiceContracts.Enums;

namespace Services;

public class PersonsUpdaterService : IPersonsUpdaterService
{
    private readonly IPersonsRepository _personsRepository;
    private readonly PersonsServiceHelper _personsServiceHelper;
    private readonly ILogger<PersonsUpdaterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    public PersonsUpdaterService(
        IPersonsRepository personsRepository,
        ILogger<PersonsUpdaterService> logger,
        IDiagnosticContext diagnosticContext
    )
    {
        _personsRepository = personsRepository;
        _personsServiceHelper = new PersonsServiceHelper();
        _logger = logger;
        _diagnosticContext = diagnosticContext;
    }

    public async Task<PersonResponse> UpdatePerson(
        PersonUpdateRequest? personUpdateRequest
    )
    {
        if (personUpdateRequest == null)
            throw new ArgumentNullException(nameof(personUpdateRequest));

        // Validation
        ValidationHelper.ModelValidation(personUpdateRequest);

        // Get matching person object to update
        Person? matchingPerson =
            await _personsRepository.GetPersonByPersonId(personUpdateRequest
                .Id);

        if (matchingPerson == null)
            throw new InvalidPersonIdException(
                "Given person id doesn't exist!");

        // Update all details
        matchingPerson.PersonName = personUpdateRequest.PersonName;
        matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
        matchingPerson.Email = personUpdateRequest.Email;
        matchingPerson.CountryId = personUpdateRequest.CountryID;
        matchingPerson.Gender = personUpdateRequest.Gender.ToString();
        matchingPerson.Address = personUpdateRequest.Address;
        matchingPerson.ReceiveNewsLetters =
            personUpdateRequest.ReceiveNewsLetters;

        // Save changes to the database
        await _personsRepository.UpdatePerson(matchingPerson);

        return matchingPerson.ToPersonResponse();
    }
}
