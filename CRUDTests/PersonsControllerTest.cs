using AutoFixture;
using CRUDExample.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDTests;

public class PersonsControllerTest
{
    private readonly IPersonService _personsService;
    private readonly ICountriesService _countriesService;

    private readonly Mock<IPersonService> _personsServiceMock;

    private readonly Fixture _fixture;

    public PersonsControllerTest()
    {
        _fixture = new Fixture();

        _personsServiceMock = new Mock<IPersonService>();
        Mock<ICountriesService> countriesServiceMock = new();

        _personsService = _personsServiceMock.Object;
        _countriesService = countriesServiceMock.Object;
    }

    #region Index

    [Fact]
    public async Task Index_ShouldReturnIndexViewWithPersonsList()
    {
        // Arrange
        List<PersonResponse> personsResponseList =
            _fixture.Create<List<PersonResponse>>();

        PersonsController personsController = new(
            _personsService, _countriesService);

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
}
