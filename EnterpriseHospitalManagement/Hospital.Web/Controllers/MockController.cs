using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystem.Controllers;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Services;

namespace HospitalManagementSystem.Tests
{
    public class PatientControllerTests
    {
        private readonly Mock<IPatientService> _mockService;
        private readonly PatientController _controller;

        public PatientControllerTests()
        {
            // Arrange: Mock the service dependency
            _mockService = new Mock<IPatientService>();
            _controller = new PatientController(_mockService.Object);
        }

        [Fact]
        public void GetPatientById_ReturnsOkResult_WithValidPatient()
        {
            // Arrange
            var patientId = 1;
            var patient = new Patient { Id = patientId, Name = "John Doe", Age = 30 };
            _mockService.Setup(s => s.GetPatientById(patientId)).Returns(patient);

            // Act
            var result = _controller.GetPatientById(patientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnPatient = Assert.IsType<Patient>(okResult.Value);
            Assert.Equal(patientId, returnPatient.Id);
            Assert.Equal("John Doe", returnPatient.Name);
        }

        [Fact]
        public void GetPatientById_ReturnsNotFound_WhenPatientDoesNotExist()
        {
            // Arrange
            var patientId = 99;
            _mockService.Setup(s => s.GetPatientById(patientId)).Returns((Patient)null);

            // Act
            var result = _controller.GetPatientById(patientId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
