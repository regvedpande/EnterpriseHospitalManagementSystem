using Xunit;
using Moq;
using HospitalManagementSystem.Services;
using HospitalManagementSystem.Models;
using HospitalManagementSystem.Repositories;

namespace HospitalManagementSystem.Tests
{
    public class PatientServiceTests
    {
        [Fact]
        public void RegisterPatient_ShouldCallRepositoryAddPatient()
        {
            // Arrange
            var mockRepo = new Mock<IPatientRepository>();
            var patientService = new PatientService(mockRepo.Object);

            var newPatient = new Patient
            {
                Id = 1,
                Name = "John Doe",
                Age = 30,
                Gender = "Male",
                ContactNumber = "9876543210"
            };

            // Act
            patientService.RegisterPatient(newPatient);

            // Assert
            mockRepo.Verify(repo => repo.AddPatient(It.Is<Patient>(
                p => p.Name == "John Doe" && p.Age == 30)), Times.Once);
        }
    }
}
