using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Hospital.API.Controllers;
using Hospital.API.Dtos;
using Hospital.API.Models;
using Hospital.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hospital.API.Tests.Controllers
{
    public class HospitalControllerTests
    {
        private readonly Mock<IHospitalService> _hospitalServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly HospitalController _controller;

        public HospitalControllerTests()
        {
            _hospitalServiceMock = new Mock<IHospitalService>();
            _mapperMock = new Mock<IMapper>();
            _controller = new HospitalController(_hospitalServiceMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnOkWithListOfHospitals()
        {
            // Arrange
            var hospitals = new List<Hospital>
            {
                new() { Id = 1, Name = "City General", City = "Warsaw", BedsTotal = 420 },
                new() { Id = 2, Name = "Memorial", City = "Kraków", BedsTotal = 280 }
            };

            var hospitalDtos = new List<HospitalDto>
            {
                new() { Id = 1, Name = "City General", City = "Warsaw", BedsTotal = 420 },
                new() { Id = 2, Name = "Memorial", City = "Kraków", BedsTotal = 280 }
            };

            _hospitalServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(hospitals);
            _mapperMock.Setup(m => m.Map<List<HospitalDto>>(hospitals)).Returns(hospitalDtos);

            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedDtos = okResult.Value.Should().BeAssignableTo<IEnumerable<HospitalDto>>().Subject;

            returnedDtos.Should().HaveCount(2);
            returnedDtos.Should().Contain(d => d.Name == "City General");
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ShouldReturnOkWithHospital()
        {
            // Arrange
            var hospital = new Hospital { Id = 5, Name = "Central Clinical", BedsAvailable = 87 };
            var dto = new HospitalDto { Id = 5, Name = "Central Clinical", BedsAvailable = 87 };

            _hospitalServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(hospital);
            _mapperMock.Setup(m => m.Map<HospitalDto>(hospital)).Returns(dto);

            // Act
            var result = await _controller.GetByIdAsync(5);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedDto = okResult.Value.Should().BeOfType<HospitalDto>().Subject;

            returnedDto.Name.Should().Be("Central Clinical");
            returnedDto.BedsAvailable.Should().Be(87);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            _hospitalServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Hospital?)null);

            // Act
            var result = await _controller.GetByIdAsync(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateAsync_ValidData_ShouldReturnCreatedAtAction()
        {
            // Arrange
            var createDto = new HospitalCreateDto
            {
                Name = "New Hope Hospital",
                Address = "Medical 12",
                City = "Gdańsk",
                BedsTotal = 250
            };

            var hospital = new Hospital { Id = 42, Name = "New Hope Hospital", ... };
            var returnedDto = new HospitalDto { Id = 42, Name = "New Hope Hospital", ... };

            _mapperMock.Setup(m => m.Map<Hospital>(createDto)).Returns(hospital);
            _hospitalServiceMock.Setup(s => s.CreateAsync(hospital)).ReturnsAsync(hospital);
            _mapperMock.Setup(m => m.Map<HospitalDto>(hospital)).Returns(returnedDto);

            // Act
            var result = await _controller.CreateAsync(createDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;

            createdResult.StatusCode.Should().Be(201);
            createdResult.RouteValues!["id"].Should().Be(42);
            createdResult.Value.Should().BeEquivalentTo(returnedDto);
        }

        [Fact]
        public async Task UpdateAsync_ValidData_ShouldReturnNoContent()
        {
            // Arrange
            var updateDto = new HospitalUpdateDto
            {
                Name = "Updated Name",
                BedsAvailable = 65
            };

            var existingHospital = new Hospital { Id = 10, Name = "Old Name", BedsAvailable = 120 };

            _hospitalServiceMock.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(existingHospital);
            _hospitalServiceMock.Setup(s => s.UpdateAsync(It.IsAny<Hospital>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateAsync(10, updateDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task UpdateAsync_NonExistingHospital_ShouldReturnNotFound()
        {
            // Arrange
            _hospitalServiceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Hospital?)null);

            // Act
            var result = await _controller.UpdateAsync(999, new HospitalUpdateDto());

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteAsync_ExistingId_ShouldReturnNoContent()
        {
            // Arrange
            _hospitalServiceMock.Setup(s => s.DeleteAsync(7)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAsync(7);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
    }
}