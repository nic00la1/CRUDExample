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
    private readonly IPersonService _personsService;
    private readonly ICountriesService _countriesService;
    private readonly ILogger<PersonsController> _logger;

    private readonly Mock<IPersonService> _personsServiceMock;
    private readonly Mock<ICountriesService> _countriesServiceMock;
    private readonly Mock<ILogger<PersonsController>> _loggerMock;


    private readonly Fixture _fixture;

    public PersonsControllerTest()
    {
        _fixture = new Fixture();

        _personsServiceMock = new Mock<IPersonService>();
        _countriesServiceMock = new Mock<ICountriesService>();
        _loggerMock = new Mock<ILogger<PersonsController>>();


        _personsService = _personsServiceMock.Object;
        _countriesService = _countriesServiceMock.Object;
        _logger = _loggerMock.Object;
    }

    #region Index

    [Fact]
    public async Task Index_ShouldReturnIndexViewWithPersonsList()
    {
        // Arrange
        List<PersonResponse> personsResponseList =
            _fixture.Create<List<PersonResponse>>();

        PersonsController personsController = new(
            _personsService, _countriesService, _logger);

        _personsServiceMock.Setup(temp =>
                temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(personsResponseList);

        _personsServiceMock.Setup(temp =>
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

        _countriesServiceMock.Setup(temp => temp.GetAllCountries())
            .ReturnsAsync(countries);

        _personsServiceMock.Setup(temp =>
                temp.AddPerson(It.IsAny<PersonAddRequest>()))
            .ReturnsAsync(personResponse);

        PersonsController personsController = new(
            _personsService, _countriesService, _logger);

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

        _countriesServiceMock.Setup(temp => temp.GetAllCountries())
            .ReturnsAsync(countries);

        _personsServiceMock.Setup(temp =>
                temp.AddPerson(It.IsAny<PersonAddRequest>()))
            .ReturnsAsync(personResponse);

        PersonsController personsController = new(
            _personsService, _countriesService, _logger);

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
