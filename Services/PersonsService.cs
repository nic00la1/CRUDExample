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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using Serilog;
using SerilogTimings;
using ServiceContracts.Enums;

namespace Services;

public class PersonsService : IPersonService
{
    private readonly IPersonsRepository _personsRepository;
    private readonly PersonsServiceHelper _personsServiceHelper;
    private readonly ILogger<PersonsService> _logger;
    private readonly IDiagnosticContext _diagnosticContext;

    public PersonsService(
        IPersonsRepository personsRepository,
        ILogger<PersonsService> logger,
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

    public async Task<List<PersonResponse>> GetAllPersons()
    {
        _logger.LogInformation("GetAllPersons of PersonsService");

        // SELECT * FROM Persons
        List<Person> persons = await _personsRepository.GetAllPersons() ??
            new List<Person>();

        return persons.Select(p => p.ToPersonResponse()).ToList();
    }

    public async Task<PersonResponse?> GetPersonById(Guid? personId)
    {
        if (personId == null) return null;

        Person? person =
            await _personsRepository.GetPersonByPersonId(personId.Value);

        if (person == null) return null;

        return person.ToPersonResponse();
    }

    public async Task<List<PersonResponse>> GetFilteredPersons(
        string searchBy,
        string? searchString
    )
    {
        _logger.LogInformation("GetFilteredPersons of PersonsService");

        List<Person> persons = null;

        using (Operation.Time("Time for Filtered Persons from Database"))
        {
            if (string.IsNullOrEmpty(searchBy) ||
                string.IsNullOrEmpty(searchString))
                persons = await _personsRepository.GetAllPersons();
            else
                persons = searchBy switch
                {
                    nameof(PersonResponse.PersonName) => await
                        _personsRepository
                            .GetFilteredPersons(temp =>
                                temp.PersonName.Contains(searchString)),
                    nameof(PersonResponse.Email) => await _personsRepository
                        .GetFilteredPersons(temp =>
                            temp.Email.Contains(searchString)),
                    nameof(PersonResponse.DateOfBirth) => await
                        _personsRepository
                            .GetFilteredPersons(temp =>
                                temp.DateOfBirth.HasValue),
                    nameof(PersonResponse.Gender) => await _personsRepository
                        .GetFilteredPersons(temp =>
                            temp.Gender.Contains(searchString)),
                    nameof(PersonResponse.CountryId) => await _personsRepository
                        .GetFilteredPersons(temp =>
                            temp.Country.CountryName.Contains(searchString)),
                    nameof(PersonResponse.Address) => await _personsRepository
                        .GetFilteredPersons(temp =>
                            temp.Address.Contains(searchString)),

                    _ => await _personsRepository.GetAllPersons()
                };
        } // End of using block

        if (searchBy == nameof(PersonResponse.DateOfBirth) &&
            !string.IsNullOrEmpty(searchString))
            persons = persons.Where(p => p.DateOfBirth.HasValue &&
                p.DateOfBirth.Value.ToString("dd MMMM yyyy")
                    .Contains(searchString)).ToList();

        _diagnosticContext.Set("Persons", persons);

        return persons.Select(p => p.ToPersonResponse()).ToList();
    }

    public async Task<List<PersonResponse>> GetSortedPersons(
        List<PersonResponse> allPersons,
        string sortBy,
        SortOderOptions sortOrder
    )
    {
        _logger.LogInformation("GetSortedPersons of PersonsService");

        if (string.IsNullOrEmpty(sortBy)) return allPersons;

        // Use the SortByProperty method from PersonsServiceHelper
        return _personsServiceHelper.SortByProperty(allPersons, sortBy,
            sortOrder);
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
            throw new ArgumentException("Given person id doesn't exist!");

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

    public async Task<bool> DeletePerson(Guid? personId)
    {
        if (personId == null) throw new ArgumentNullException(nameof(personId));

        Person? person =
            await _personsRepository.GetPersonByPersonId(personId.Value);
        if (person == null) return false;

        await _personsRepository.DeletePersonByPersonId(personId.Value);

        return true;
    }

    public async Task<MemoryStream> GetPersonCSV()
    {
        MemoryStream memoryStream = new();
        StreamWriter streamWriter = new(memoryStream);

        CsvConfiguration csvConfiguration = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        };

        CsvWriter csvWriter =
            new(streamWriter, csvConfiguration);

        // PersonName, Email, DateOfBirth, Age, Gender, Country, Address, ReceiveNewsLetters
        csvWriter.WriteField(nameof(PersonResponse.PersonName));
        csvWriter.WriteField(nameof(PersonResponse.Email));
        csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
        csvWriter.WriteField(nameof(PersonResponse.Age));
        csvWriter.WriteField(nameof(PersonResponse.Country));
        csvWriter.WriteField(nameof(PersonResponse.Address));
        csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));
        csvWriter.NextRecord();

        List<PersonResponse> persons = await GetAllPersons();

        foreach (PersonResponse person in persons)
        {
            csvWriter.WriteField(person.PersonName);
            csvWriter.WriteField(person.Email);
            if (person.DateOfBirth.HasValue)
                csvWriter.WriteField(
                    person.DateOfBirth.Value.ToString("yyyy-MM-dd"));
            else
                csvWriter.WriteField(string.Empty);
            csvWriter.WriteField(person.Age);
            csvWriter.WriteField(person.Country);
            csvWriter.WriteField(person.Address);
            csvWriter.WriteField(person.ReceiveNewsLetters);
            csvWriter.NextRecord();
            csvWriter.Flush();
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task<MemoryStream> GetPersonExcel()
    {
        MemoryStream memoryStream = new();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        ExcelPackage excelPackage = new(memoryStream);

        ExcelWorksheet workSheet =
            excelPackage.Workbook.Worksheets.Add("PersonsSheet");

        workSheet.Cells["A1"].Value = nameof(PersonResponse.PersonName);
        workSheet.Cells["B1"].Value = nameof(PersonResponse.Email);
        workSheet.Cells["C1"].Value = nameof(PersonResponse.DateOfBirth);
        workSheet.Cells["D1"].Value = nameof(PersonResponse.Age);
        workSheet.Cells["E1"].Value = nameof(PersonResponse.Gender);
        workSheet.Cells["F1"].Value = nameof(PersonResponse.Country);
        workSheet.Cells["G1"].Value = nameof(PersonResponse.Address);
        workSheet.Cells["H1"].Value = nameof(PersonResponse.ReceiveNewsLetters);

        ExcelRange headerCells = workSheet.Cells["A1:H1"];
        headerCells.Style.Font.Bold = true;
        headerCells.Style.Fill.PatternType =
            OfficeOpenXml.Style.ExcelFillStyle.Solid;
        headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color
            .LightGray);
        headerCells.AutoFitColumns();

        int row = 2;
        List<PersonResponse> persons = await GetAllPersons();

        foreach (PersonResponse person in persons)
        {
            workSheet.Cells[row, 1].Value = person.PersonName;
            workSheet.Cells[row, 2].Value = person.Email;
            if (person.DateOfBirth.HasValue)
                workSheet.Cells[row, 3].Value =
                    person.DateOfBirth.Value.ToString("yyyy-MM-dd");
            else
                workSheet.Cells[row, 3].Value = string.Empty;
            workSheet.Cells[row, 4].Value = person.Age;
            workSheet.Cells[row, 5].Value = person.Gender;
            workSheet.Cells[row, 6].Value = person.Country;
            workSheet.Cells[row, 7].Value = person.Address;
            workSheet.Cells[row, 8].Value = person.ReceiveNewsLetters;

            row++;
        }

        workSheet.Cells[$"A1:H{row}"].AutoFitColumns();

        await excelPackage.SaveAsync();

        excelPackage.Stream.Position = 0;
        return memoryStream;
    }
}
