using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HospitalApi.Controllers;
using HospitalApi.Models;
using HospitalApi.Services;

namespace HospitalApi.Tests
{
    public class HospitalControllerTests
    {
        [Fact]
        public void GetPatientById_ReturnsOkResult_WithPatient()
        {
            // Arrange
            var mockService = new Mock<IHospitalService>();
            mockService.Setup(s => s.GetPatientById(1))
                       .Returns(new Patient { Id = 1, Name = "John Doe" });

            var controller = new HospitalController(mockService.Object);

            // Act
            var result = controller.GetPatientById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var patient = Assert.IsType<Patient>(okResult.Value);
            Assert.Equal(1, patient.Id);
            Assert.Equal("John Doe", patient.Name);
        }
    }
}
