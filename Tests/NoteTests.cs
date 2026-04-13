using System.ComponentModel.DataAnnotations;
using NoteApp.Models;
using Xunit;

namespace NoteApp.Tests.Models
{
    public class NoteTests
    {
        [Fact]
        public void Note_IsValid_WhenTitleAndContentProvided()
        {
            // Arrange
            var note = new Note { Title = "Test Title", Content = "Test Content" };
            var context = new ValidationContext(note);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(note, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void Note_IsInvalid_WhenTitleMissing()
        {
            // Arrange
            var note = new Note { Content = "Test Content" };
            var context = new ValidationContext(note);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(note, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.Single(results);
            Assert.Contains("Title", results[0].MemberNames);
        }

        [Fact]
        public void Note_IsInvalid_WhenContentMissing()
        {
            // Arrange
            var note = new Note { Title = "Test Title" };
            var context = new ValidationContext(note);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(note, context, results, true);

            // Assert
            Assert.False(isValid);
            Assert.Single(results);
            Assert.Contains("Content", results[0].MemberNames);
        }

        [Fact]
        public void Note_CreatedAt_IsSetByDefault()
        {
            // Arrange & Act
            var note = new Note();

            // Assert
            Assert.True(note.CreatedAt > DateTime.MinValue);
        }
    }
}