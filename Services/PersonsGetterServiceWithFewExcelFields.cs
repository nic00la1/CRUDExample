using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class PersonsGetterServiceWithFewExcelFields : IPersonsGetterService
{
    private readonly PersonsGetterService _personsGetterService;

    public PersonsGetterServiceWithFewExcelFields(
        PersonsGetterService personsGetterService
    )
    {
        _personsGetterService = personsGetterService;
    }

    public async Task<List<PersonResponse>> GetAllPersons()
    {
        return await _personsGetterService.GetAllPersons();
    }

    public async Task<PersonResponse?> GetPersonById(Guid? personId)
    {
        return await _personsGetterService.GetPersonById(personId);
    }

    public async Task<List<PersonResponse>> GetFilteredPersons(
        string searchBy,
        string? searchString
    )
    {
        return await _personsGetterService.GetFilteredPersons(searchBy,
            searchString);
    }

    public async Task<MemoryStream> GetPersonCSV()
    {
        return await _personsGetterService.GetPersonCSV();
    }

    public async Task<MemoryStream> GetPersonExcel()
    {
        MemoryStream memoryStream = new();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        ExcelPackage excelPackage = new(memoryStream);

        ExcelWorksheet workSheet =
            excelPackage.Workbook.Worksheets.Add("PersonsSheet");

        workSheet.Cells["A1"].Value = nameof(PersonResponse.PersonName);
        workSheet.Cells["B1"].Value = nameof(PersonResponse.Age);
        workSheet.Cells["C1"].Value = nameof(PersonResponse.Gender);


        ExcelRange headerCells = workSheet.Cells["A1:C1"];
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
            workSheet.Cells[row, 2].Value = person.Age;
            workSheet.Cells[row, 3].Value = person.Gender;

            row++;
        }

        workSheet.Cells[$"A1:C{row}"].AutoFitColumns();

        await excelPackage.SaveAsync();

        excelPackage.Stream.Position = 0;
        return memoryStream;
    }
}
