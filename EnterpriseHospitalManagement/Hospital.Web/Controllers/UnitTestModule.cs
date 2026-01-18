using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using YourNamespace.Controllers;
using YourNamespace.Models;
using YourNamespace.Services;

namespace YourNamespace.Tests
{
    public class HospitalControllerTests
    {
        private readonly Mock<IHospitalService> _mockHospitalService;
        private readonly HospitalController _hospitalController;

        public HospitalControllerTests()
        {
            // Set up the mocked service
            _mockHospitalService = new Mock<IHospitalService>();

            // Set up the controller with the mocked service
            _hospitalController = new HospitalController(_mockHospitalService.Object);
        }

        [Fact]
        public void GetAllHospitals_ReturnsOkResult_WithListOfHospitals()
        {
            // Arrange
            var hospitals = new List<Hospital>
            {
                new Hospital { Id = 1, Name = "Hospital A", Location = "Location A" },
                new Hospital { Id = 2, Name = "Hospital B", Location = "Location B" },
            };

            _mockHospitalService
                .Setup(s => s.GetAllHospitals())
                .Returns(hospitals);

            // Act
            var result = _hospitalController.GetAllHospitals();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedHospitals = Assert.IsType<List<Hospital>>(okResult.Value);
            Assert.Equal(2, returnedHospitals.Count);
        }

        [Fact]
        public void GetHospitalById_HospitalExists_ReturnsOkResult()
        {
            // Arrange
            var hospitalId = 1;
            var hospital = new Hospital { Id = hospitalId, Name = "Hospital A", Location = "Location A" };

            _mockHospitalService
                .Setup(s => s.GetHospitalById(hospitalId))
                .Returns(hospital);

            // Act
            var result = _hospitalController.GetHospitalById(hospitalId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedHospital = Assert.IsType<Hospital>(okResult.Value);
            Assert.Equal(hospitalId, returnedHospital.Id);
        }

        [Fact]
        public void GetHospitalById_HospitalDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var hospitalId = 1;

            _mockHospitalService
                .Setup(s => s.GetHospitalById(hospitalId))
                .Returns((Hospital)null);

            // Act
            var result = _hospitalController.GetHospitalById(hospitalId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void AddHospital_ValidHospital_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var hospital = new Hospital { Id = 1, Name = "New Hospital", Location = "Location C" };

            _mockHospitalService
                .Setup(s => s.AddHospital(It.IsAny<Hospital>()))
                .Returns(hospital);

            // Act
            var result = _hospitalController.AddHospital(hospital);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnedHospital = Assert.IsType<Hospital>(createdAtActionResult.Value);
            Assert.Equal(hospital.Name, returnedHospital.Name);
        }

        [Fact]
        public void DeleteHospital_HospitalExists_ReturnsNoContent()
        {
            // Arrange
            var hospitalId = 1;

            _mockHospitalService
                .Setup(s => s.DeleteHospital(hospitalId))
                .Returns(true);

            // Act
            var result = _hospitalController.DeleteHospital(hospitalId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void DeleteHospital_HospitalDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var hospitalId = 1;

            _mockHospitalService
                .Setup(s => s.DeleteHospital(hospitalId))
                .Returns(false);

            // Act
            var result = _hospitalController.DeleteHospital(hospitalId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}