using Xunit;
using Moq;
using YourProject.Controllers;
using YourProject.Models;
using YourProject.Services;
using Microsoft.AspNetCore.Mvc;

public class HospitalControllerTests
{
    [Fact]
    public void GetPatient_ReturnsViewResult_WithPatientModel()
    {
        // Arrange
        var mockService = new Mock<IPatientService>();
        mockService.Setup(service => service.GetPatientById(1))
                   .Returns(new Patient { Id = 1, Name = "John Doe" });

        var controller = new HospitalController(mockService.Object);

        // Act
        var result = controller.GetPatient(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Patient>(viewResult.Model);
        Assert.Equal(1, model.Id);
        Assert.Equal("John Doe", model.Name);
    }
}