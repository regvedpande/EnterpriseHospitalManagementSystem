using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using YourNamespace.Controllers;
using YourNamespace.Models;
using YourNamespace.Services;

namespace YourNamespace.Tests.Controllers
{
    public class BlogControllerTests
    {
        private readonly Mock<IBlogService> _mockBlogService; // Mock for the Blog service
        private readonly BlogController _controller;

        public BlogControllerTests()
        {
            // Initialize mock service
            _mockBlogService = new Mock<IBlogService>();

            // Inject mock into the controller
            _controller = new BlogController(_mockBlogService.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfBlogs()
        {
            // Arrange
            var blogs = new List<Blog>
            {
                new Blog { Id = 1, Title = "Test Blog 1", Content = "Content 1" },
                new Blog { Id = 2, Title = "Test Blog 2", Content = "Content 2" }
            };
            _mockBlogService.Setup(service => service.GetAllBlogsAsync()).ReturnsAsync(blogs);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Blog>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithBlog()
        {
            // Arrange
            var testId = 1;
            var blog = new Blog { Id = testId, Title = "Test Blog", Content = "Content" };
            _mockBlogService.Setup(service => service.GetBlogByIdAsync(testId)).ReturnsAsync(blog);

            // Act
            var result = await _controller.Details(testId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Blog>(viewResult.ViewData.Model);
            Assert.Equal(blog.Id, model.Id);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenBlogDoesNotExist()
        {
            // Arrange
            var testId = 1;
            _mockBlogService.Setup(service => service.GetBlogByIdAsync(testId)).ReturnsAsync((Blog)null);

            // Act
            var result = await _controller.Details(testId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_RedirectsToIndex_WhenModelStateIsValid()
        {
            // Arrange
            var newBlog = new Blog { Title = "New Blog", Content = "New Content" };
            _mockBlogService.Setup(service => service.CreateBlogAsync(newBlog)).ReturnsAsync(true);

            // Act
            var result = await _controller.Create(newBlog);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.Create(new Blog());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Blog>(viewResult.ViewData.Model);
        }
    }
}