using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

public class HospitalControllerTests
{
    [Fact]
    public async Task GetHospitalById_ReturnsOkObjectResult_WithHospital()
    {
        // Arrange
        var mockHospitalService = new Mock<IHospitalService>();
        var hospitalId = 1;
        var mockHospital = new Hospital { Id = 1, Name = "General Hospital" };

        mockHospitalService
            .Setup(service => service.GetHospitalByIdAsync(hospitalId))
            .ReturnsAsync(mockHospital);

        var controller = new HospitalController(mockHospitalService.Object);

        // Act
        var result = await controller.GetHospitalById(hospitalId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedHospital = Assert.IsType<Hospital>(okResult.Value);

        Assert.Equal(mockHospital.Id, returnedHospital.Id);
        Assert.Equal(mockHospital.Name, returnedHospital.Name);
    }

    [Fact]
    public async Task GetHospitalById_ReturnsNotFound_WhenHospitalDoesNotExist()
    {
        // Arrange
        var mockHospitalService = new Mock<IHospitalService>();
        var hospitalId = 999;

        mockHospitalService
            .Setup(service => service.GetHospitalByIdAsync(hospitalId))
            .ReturnsAsync((Hospital)null);

        var controller = new HospitalController(mockHospitalService.Object);

        // Act
        var result = await controller.GetHospitalById(hospitalId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}

// Supporting Models and Service Interface
public interface IHospitalService
{
    Task<Hospital> GetHospitalByIdAsync(int id);
}

public class Hospital
{
    public int Id { get; set; }
    public string Name { get; set; }
}