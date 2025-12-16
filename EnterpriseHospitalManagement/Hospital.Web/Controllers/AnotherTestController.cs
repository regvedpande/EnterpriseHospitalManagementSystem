using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace HospitalControllerTests
{
    public class HospitalControllerTests
    {
        private readonly Mock<IPatientService> _mockPatientService;
        private readonly HospitalController _controller;

        public HospitalControllerTests()
        {
            // Initialize Mock service and controller
            _mockPatientService = new Mock<IPatientService>();
            _controller = new HospitalController(_mockPatientService.Object);
        }

        [Fact]
        public void GetPatient_ReturnsOk_WhenPatientFound()
        {
            // Arrange
            int patientId = 1;
            var patient = new Patient { Id = patientId, Name = "John Doe" };
            _mockPatientService.Setup(service => service.GetPatientById(patientId)).Returns(patient);

            // Act
            var result = _controller.GetPatient(patientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(patient, okResult.Value);
        }

        [Fact]
        public void GetPatient_ReturnsNotFound_WhenPatientNotFound()
        {
            // Arrange
            int patientId = 1;
            _mockPatientService.Setup(service => service.GetPatientById(patientId)).Returns((Patient)null);

            // Act
            var result = _controller.GetPatient(patientId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void AddPatient_ReturnsCreatedAtAction_WhenPatientIsAdded()
        {
            // Arrange
            var newPatient = new Patient { Name = "John Doe" };
            var createdPatient = new Patient { Id = 1, Name = "John Doe" };
            _mockPatientService.Setup(service => service.AddPatient(newPatient)).Returns(createdPatient);

            // Act
            var result = _controller.AddPatient(newPatient);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetPatient", createdAtActionResult.ActionName);
            Assert.Equal(createdPatient.Id, ((Patient)createdAtActionResult.Value).Id);
        }
    }

    // THESE ARE DUMMY CLASSES. REPLACE WITH ACTUAL ONES IN YOUR APP.
    public interface IPatientService
    {
        Patient GetPatientById(int id);
        Patient AddPatient(Patient patient);
    }

    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}