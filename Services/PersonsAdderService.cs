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

public class PersonsAdderService : IPersonsAdderService
{
    private readonly IPersonsRepository _personsRepository;
    private readonly PersonsServiceHelper _personsServiceHelper;
    private readonly ILogger<PersonsGetterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    public PersonsAdderService(
        IPersonsRepository personsRepository,
        ILogger<PersonsGetterService> logger,
        IDiagnosticContext diagnosticContext
    )
    {
        _personsRepository = personsRepository;
        _personsServiceHelper = new PersonsServiceHelper();
        _logger = logger;
        _diagnosticContext = diagnosticContext;
    }

    public async Task<PersonResponse> AddPerson(
        PersonAddRequest? personAddRequest
    )
    {
        // Check if PersonAddRequest is not null
        if (personAddRequest == null)
            throw new ArgumentNullException(nameof(personAddRequest));

        // Model Validation
        ValidationHelper.ModelValidation(personAddRequest);

        // Convert personAddRequest into Person type
        Person person = personAddRequest.ToPerson();

        // Generate a new PersonID
        person.Id = Guid.NewGuid();

        // Add Person object to persons list
        await _personsRepository.AddPerson(person);

        // Convert Person object into PersonResponse type
        return person.ToPersonResponse();
    }
}
