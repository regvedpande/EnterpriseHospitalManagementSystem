using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using YourNamespace.Controllers;
using YourNamespace.Models;
using YourNamespace.Services;

public class HospitalControllerTests
{
    private readonly Mock<IHospitalService> _hospitalServiceMock;
    private readonly HospitalController _controller;

    public HospitalControllerTests()
    {
        _hospitalServiceMock = new Mock<IHospitalService>();
        _controller = new HospitalController(_hospitalServiceMock.Object);
    }

    [Fact]
    public void GetHospitalById_ReturnsOk_WithHospital()
    {
        // Arrange
        var hospital = new Hospital { Id = 1, Name = "City Hospital" };
        _hospitalServiceMock.Setup(s => s.GetHospitalById(1)).Returns(hospital);

        // Act
        var result = _controller.GetHospitalById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedHospital = Assert.IsType<Hospital>(okResult.Value);
        Assert.Equal(1, returnedHospital.Id);
        Assert.Equal("City Hospital", returnedHospital.Name);
    }
}
