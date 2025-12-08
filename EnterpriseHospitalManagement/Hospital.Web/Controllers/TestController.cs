using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

// Import your namespaces (replace with actual namespaces)
using YourNamespace.Controllers;
using YourNamespace.Models;
using YourNamespace.Services;

namespace YourNamespace.Tests
{
    public class BlogControllerTests
    {
        private readonly Mock<IBlogService> _mockBlogService;
        private readonly BlogController _controller;

        public BlogControllerTests()
        {
            _mockBlogService = new Mock<IBlogService>();
            _controller = new BlogController(_mockBlogService.Object);
        }

        [Fact]
        public async Task GetBlogs_ShouldReturnOkResult_WithListOfBlogs()
        {
            // Arrange
            var mockBlogs = new List<Blog>
            {
                new Blog { Id = 1, Title = "Blog 1", Content = "Content 1" },
                new Blog { Id = 2, Title = "Blog 2", Content = "Content 2" }
            };
            _mockBlogService.Setup(service => service.GetBlogsAsync()).ReturnsAsync(mockBlogs);

            // Act
            var result = await _controller.GetBlogs();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Blog>>(okResult.Value);
            Assert.Equal(2, ((List<Blog>)returnValue).Count);
        }

        [Fact]
        public async Task GetBlogById_ShouldReturnNotFound_WhenBlogDoesNotExist()
        {
            // Arrange
            int blogId = 999;
            _mockBlogService.Setup(service => service.GetBlogByIdAsync(blogId)).ReturnsAsync((Blog)null);

            // Act
            var result = await _controller.GetBlog(blogId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetBlogById_ShouldReturnOkResult_WithBlog()
        {
            // Arrange
            int blogId = 1;
            var mockBlog = new Blog { Id = blogId, Title = "Test Blog", Content = "Test Content" };
            _mockBlogService.Setup(service => service.GetBlogByIdAsync(blogId)).ReturnsAsync(mockBlog);

            // Act
            var result = await _controller.GetBlog(blogId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Blog>(okResult.Value);
            Assert.Equal(blogId, returnValue.Id);
        }

        [Fact]
        public async Task CreateBlog_ShouldReturnCreatedResponse_WithNewBlog()
        {
            // Arrange
            var newBlog = new Blog { Title = "New Blog", Content = "New Content" };
            _mockBlogService.Setup(service => service.CreateBlogAsync(newBlog)).ReturnsAsync(newBlog);

            // Act
            var result = await _controller.CreateBlog(newBlog);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var returnValue = Assert.IsType<Blog>(createdResult.Value);
            Assert.Equal("New Blog", returnValue.Title);
        }

        [Fact]
        public async Task UpdateBlog_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            int blogId = 1;
            var updatedBlog = new Blog { Id = blogId, Title = "Updated Blog", Content = "Updated Content" };
            _mockBlogService.Setup(service => service.UpdateBlogAsync(blogId, updatedBlog)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateBlog(blogId, updatedBlog);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateBlog_ShouldReturnNotFound_WhenBlogDoesNotExist()
        {
            // Arrange
            int blogId = 999;
            var updatedBlog = new Blog { Id = blogId, Title = "Updated Blog", Content = "Updated Content" };
            _mockBlogService.Setup(service => service.UpdateBlogAsync(blogId, updatedBlog)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateBlog(blogId, updatedBlog);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteBlog_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            int blogId = 1;
            _mockBlogService.Setup(service => service.DeleteBlogAsync(blogId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteBlog(blogId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteBlog_ShouldReturnNotFound_WhenBlogDoesNotExist()
        {
            // Arrange
            int blogId = 999;
            _mockBlogService.Setup(service => service.DeleteBlogAsync(blogId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteBlog(blogId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}