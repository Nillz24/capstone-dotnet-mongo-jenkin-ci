using Microsoft.AspNetCore.Mvc;
using Moq;
using NoteApp.Controllers;
using NoteApp.Models;
using NoteApp.Services;
using Xunit;

namespace NoteApp.Tests.Controllers
{
    public class NotesControllerTests
    {
        private readonly Mock<NoteService> _mockService;
        private readonly NotesController _controller;

        public NotesControllerTests()
        {
            _mockService = new Mock<NoteService>();
            _controller = new NotesController(_mockService.Object);
        }

        [Fact]
        public void Index_ReturnsViewWithNotes()
        {
            // Arrange
            var notes = new List<Note> { new Note { Id = "1", Title = "Test", Content = "Content" } };
            _mockService.Setup(s => s.GetAll()).Returns(notes);

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(notes, result.Model);
            Assert.Equal(1, _controller.ViewData["NotesCount"]);
        }

        [Fact]
        public void Details_ReturnsView_WhenNoteExists()
        {
            // Arrange
            var note = new Note { Id = "1", Title = "Test", Content = "Content" };
            _mockService.Setup(s => s.GetById("1")).Returns(note);

            // Act
            var result = _controller.Details("1") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(note, result.Model);
        }

        [Fact]
        public void Details_ReturnsNotFound_WhenNoteNotExists()
        {
            // Arrange
            _mockService.Setup(s => s.GetById("1")).Returns((Note)null);

            // Act
            var result = _controller.Details("1");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            // Act
            var result = _controller.Create() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Create_Post_Redirects_WhenModelValid()
        {
            // Arrange
            var note = new Note { Title = "Test", Content = "Content" };
            _controller.ModelState.Clear(); // Valid

            // Act
            var result = _controller.Create(note) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            _mockService.Verify(s => s.Create(note), Times.Once);
        }

        [Fact]
        public void Create_Post_ReturnsView_WhenModelInvalid()
        {
            // Arrange
            var note = new Note { Title = "", Content = "Content" };
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = _controller.Create(note) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(note, result.Model);
            _mockService.Verify(s => s.Create(It.IsAny<Note>()), Times.Never);
        }

        [Fact]
        public void Edit_Get_ReturnsView_WhenNoteExists()
        {
            // Arrange
            var note = new Note { Id = "1", Title = "Test", Content = "Content" };
            _mockService.Setup(s => s.GetById("1")).Returns(note);

            // Act
            var result = _controller.Edit("1") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(note, result.Model);
        }

        [Fact]
        public void Edit_Get_ReturnsNotFound_WhenNoteNotExists()
        {
            // Arrange
            _mockService.Setup(s => s.GetById("1")).Returns((Note)null);

            // Act
            var result = _controller.Edit("1");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_Redirects_WhenValid()
        {
            // Arrange
            var note = new Note { Id = "1", Title = "Updated", Content = "Updated" };
            _controller.ModelState.Clear();

            // Act
            var result = _controller.Edit("1", note) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            _mockService.Verify(s => s.Update("1", note), Times.Once);
        }

        [Fact]
        public void Edit_Post_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var note = new Note { Id = "2", Title = "Updated", Content = "Updated" };

            // Act
            var result = _controller.Edit("1", note);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            _mockService.Verify(s => s.Update(It.IsAny<string>(), It.IsAny<Note>()), Times.Never);
        }

        [Fact]
        public void Edit_Post_ReturnsView_WhenModelInvalid()
        {
            // Arrange
            var note = new Note { Id = "1", Title = "", Content = "Updated" };
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = _controller.Edit("1", note) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(note, result.Model);
            _mockService.Verify(s => s.Update(It.IsAny<string>(), It.IsAny<Note>()), Times.Never);
        }

        [Fact]
        public void Delete_Get_ReturnsView_WhenNoteExists()
        {
            // Arrange
            var note = new Note { Id = "1", Title = "Test", Content = "Content" };
            _mockService.Setup(s => s.GetById("1")).Returns(note);

            // Act
            var result = _controller.Delete("1") as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(note, result.Model);
        }

        [Fact]
        public void Delete_Get_ReturnsNotFound_WhenNoteNotExists()
        {
            // Arrange
            _mockService.Setup(s => s.GetById("1")).Returns((Note)null);

            // Act
            var result = _controller.Delete("1");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteConfirmed_DeletesAndRedirects()
        {
            // Act
            var result = _controller.DeleteConfirmed("1") as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            _mockService.Verify(s => s.Delete("1"), Times.Once);
        }
    }
}