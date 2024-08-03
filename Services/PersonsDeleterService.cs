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

public class PersonsDeleterService : IPersonsDeleterService
{
    private readonly IPersonsRepository _personsRepository;
    private readonly PersonsServiceHelper _personsServiceHelper;
    private readonly ILogger<PersonsGetterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    public PersonsDeleterService(
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

    public async Task<bool> DeletePerson(Guid? personId)
    {
        if (personId == null) throw new ArgumentNullException(nameof(personId));

        Person? person =
            await _personsRepository.GetPersonByPersonId(personId.Value);
        if (person == null) return false;

        await _personsRepository.DeletePersonByPersonId(personId.Value);

        return true;
    }
}
