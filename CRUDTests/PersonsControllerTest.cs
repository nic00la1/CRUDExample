using AutoFixture;
using Castle.Core.Logging;
using CRUDExample.Controllers;
using Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDTests;

public class PersonsControllerTest
{
    private readonly IFixture _fixture;
    private readonly Mock<ICountriesAdderService> _countriesAdderServiceMock;
    private readonly Mock<ICountriesGetterService> _countriesGetterServiceMock;
    private readonly Mock<ICountriesExcelService> _countriesExcelServiceMock;

    private readonly Mock<ILogger<PersonsController>> _loggerMock;
    private readonly Mock<IPersonsGetterService> _personsGetterServiceMock;
    private readonly Mock<IPersonsSorterService> _personsSorterServiceMock;
    private readonly Mock<IPersonsAdderService> _personsAdderServiceMock;
    private readonly Mock<IPersonsUpdaterService> _personsUpdaterServiceMock;
    private readonly Mock<IPersonsDeleterService> _personsDeleterServiceMock;

    private readonly ICountriesAdderService _countriesAdderService;
    private readonly ICountriesGetterService _countriesGetterService;
    private readonly ICountriesExcelService _countriesExcelService;

    private readonly ILogger<PersonsController> _logger;
    private readonly IPersonsGetterService _personsGetterService;
    private readonly IPersonsSorterService _personsSorterService;
    private readonly IPersonsAdderService _personsAdderService;
    private readonly IPersonsUpdaterService _personsUpdaterService;
    private readonly IPersonsDeleterService _personsDeleterService;

    public PersonsControllerTest()
    {
        _fixture = new Fixture();

        _countriesAdderServiceMock = new Mock<ICountriesAdderService>();
        _countriesGetterServiceMock = new Mock<ICountriesGetterService>();
        _countriesExcelServiceMock = new Mock<ICountriesExcelService>();

        _loggerMock = new Mock<ILogger<PersonsController>>();
        _personsGetterServiceMock = new Mock<IPersonsGetterService>();
        _personsSorterServiceMock = new Mock<IPersonsSorterService>();
        _personsAdderServiceMock = new Mock<IPersonsAdderService>();
        _personsUpdaterServiceMock = new Mock<IPersonsUpdaterService>();
        _personsDeleterServiceMock = new Mock<IPersonsDeleterService>();

        _countriesAdderService = _countriesAdderServiceMock.Object;
        _countriesGetterService = _countriesGetterServiceMock.Object;
        _countriesExcelService = _countriesExcelServiceMock.Object;

        _logger = _loggerMock.Object;
        _personsGetterService = _personsGetterServiceMock.Object;
        _personsSorterService = _personsSorterServiceMock.Object;
        _personsAdderService = _personsAdderServiceMock.Object;
        _personsUpdaterService = _personsUpdaterServiceMock.Object;
        _personsDeleterService = _personsDeleterServiceMock.Object;
    }

    #region Index

    [Fact]
    public async Task Index_ShouldReturnIndexViewWithPersonsList()
    {
        // Arrange
        List<PersonResponse> personsResponseList =
            _fixture.Create<List<PersonResponse>>();

        PersonsController personsController = new(
            _countriesAdderService,
            _countriesGetterService,
            _countriesExcelService,
            _logger,
            _personsAdderService,
            _personsUpdaterService,
            _personsDeleterService,
            _personsGetterService,
            _personsSorterService
        );

        _personsGetterServiceMock.Setup(temp =>
                temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(personsResponseList);

        _personsSorterServiceMock.Setup(temp =>
                temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(),
                    It.IsAny<string>(),
                    It.IsAny<SortOderOptions>()))
            .ReturnsAsync(personsResponseList);

        // Act 
        IActionResult result = await personsController.Index(
            _fixture.Create<string>(),
            _fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<SortOderOptions>());

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        viewResult.ViewData.Model.Should()
            .BeAssignableTo<IEnumerable<PersonResponse>>();

        viewResult.ViewData.Model.Should().BeEquivalentTo(personsResponseList);
    }

    #endregion

    #region Create

    [Fact]
    public async Task Create_IfModelErrors_ToReturnCreateView()
    {
        // Arrange
        PersonAddRequest personAddRequest =
            _fixture.Create<PersonAddRequest>();

        PersonResponse personResponse =
            _fixture.Create<PersonResponse>();

        List<CountryResponse> countries =
            _fixture.Create<List<CountryResponse>>();

        _countriesGetterServiceMock.Setup(temp => temp.GetAllCountries())
            .ReturnsAsync(countries);

        _personsAdderServiceMock.Setup(temp =>
                temp.AddPerson(It.IsAny<PersonAddRequest>()))
            .ReturnsAsync(personResponse);

        PersonsController personsController = new(
            _countriesAdderService,
            _countriesGetterService,
            _countriesExcelService,
            _logger,
            _personsAdderService,
            _personsUpdaterService,
            _personsDeleterService,
            _personsGetterService,
            _personsSorterService
        );

        // Act 
        personsController.ModelState.AddModelError("PersonName",
            "Person Name can't be blank!");

        IActionResult result = await personsController.Create(personAddRequest);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        viewResult.ViewData.Model.Should()
            .BeAssignableTo<PersonAddRequest>();

        viewResult.ViewData.Model.Should().Be(personAddRequest);
    }

    [Fact]
    public async Task Create_IfNoModelErrors_ToReturnRedirectToIndex()
    {
        // Arrange
        PersonAddRequest personAddRequest =
            _fixture.Create<PersonAddRequest>();

        PersonResponse personResponse =
            _fixture.Create<PersonResponse>();

        List<CountryResponse> countries =
            _fixture.Create<List<CountryResponse>>();

        _countriesGetterServiceMock.Setup(temp => temp.GetAllCountries())
            .ReturnsAsync(countries);

        _personsAdderServiceMock.Setup(temp =>
                temp.AddPerson(It.IsAny<PersonAddRequest>()))
            .ReturnsAsync(personResponse);

        PersonsController personsController = new(
            _countriesAdderService,
            _countriesGetterService,
            _countriesExcelService,
            _logger,
            _personsAdderService,
            _personsUpdaterService,
            _personsDeleterService,
            _personsGetterService,
            _personsSorterService
        );

        // Act 
        IActionResult result = await personsController.Create(personAddRequest);

        // Assert
        RedirectToActionResult redirectResult =
            Assert.IsType<RedirectToActionResult>(result);

        redirectResult.ActionName.Should()
            .Be("Index");
    }

    #endregion
}
