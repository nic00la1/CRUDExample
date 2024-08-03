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

public class PersonsSorterService : IPersonsSorterService
{
    private readonly IPersonsRepository _personsRepository;
    private readonly PersonsServiceHelper _personsServiceHelper;
    private readonly ILogger<PersonsGetterService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    public PersonsSorterService(
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


    public async Task<List<PersonResponse>> GetSortedPersons(
        List<PersonResponse> allPersons,
        string sortBy,
        SortOderOptions sortOrder
    )
    {
        _logger.LogInformation("GetSortedPersons of PersonsGetterService");

        if (string.IsNullOrEmpty(sortBy)) return allPersons;

        // Use the SortByProperty method from PersonsServiceHelper
        return _personsServiceHelper.SortByProperty(allPersons, sortBy,
            sortOrder);
    }
}
