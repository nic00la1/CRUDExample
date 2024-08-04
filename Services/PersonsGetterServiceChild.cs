using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using Serilog;
using ServiceContracts.DTO;

namespace Services;

public class PersonsGetterServiceChild : PersonsGetterService
{
    public PersonsGetterServiceChild(IPersonsRepository personsRepository,
                                     ILogger<PersonsGetterService> logger,
                                     IDiagnosticContext diagnosticContext
    ) : base(personsRepository, logger, diagnosticContext)
    {
    }

    public override async Task<MemoryStream> GetPersonExcel()
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

        if (persons.Count == 0)
            throw new InvalidOperationException("No persons data");

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
