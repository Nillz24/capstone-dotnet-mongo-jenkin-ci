using Moq;
using MongoDB.Driver;
using NoteApp.Models;
using NoteApp.Services;
using Xunit;


namespace NoteApp.Tests.Services
{
    public class NoteServiceTests
    {
        private readonly Mock<IMongoCollection<Note>> _mockCollection;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly NoteService _service;

        public NoteServiceTests()
        {
            _mockCollection = new Mock<IMongoCollection<Note>>();
            _mockDatabase = new Mock<IMongoDatabase>();

            _mockDatabase.Setup(d => d.GetCollection<Note>("Notes", null))
                         .Returns(_mockCollection.Object);

            _service = new NoteService(_mockDatabase.Object);
        }

        [Fact]
        public void GetAll_ReturnsAllNotes()
        {
            // Arrange
            var notes = new List<Note> { new Note { Id = "1", Title = "Test", Content = "Content" } };
            



            var mockFindFluent = new Mock<IFindFluent<Note, Note>>();

            mockFindFluent.Setup(f => f.ToList(It.IsAny<CancellationToken>()))
              .Returns(notes);

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<Note>>()))
               .Returns(mockFindFluent.Object);               

            // Act
            var result = _service.GetAll();

            // Assert
            Assert.Single(result);
            Assert.Equal("1", result[0].Id);
        }

       
        [Fact]
        public void GetById_ReturnsNote_WhenExists()
        {
            // Arrange
            var note = new Note { Id = "1", Title = "Test", Content = "Content" };

            var mockFindFluent = new Mock<IFindFluent<Note, Note>>();

            mockFindFluent.Setup(f => f.FirstOrDefault(It.IsAny<CancellationToken>()))
                        .Returns(note);

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<Note>>()))
                        .Returns(mockFindFluent.Object);

            // Act
            var result = _service.GetById("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
        }

        [Fact]
        public void GetById_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var mockFindFluent = new Mock<IFindFluent<Note, Note>>();

            mockFindFluent.Setup(f => f.FirstOrDefault(It.IsAny<CancellationToken>()))
                        .Returns((Note)null);

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<Note>>()))
                        .Returns(mockFindFluent.Object);

            // Act
            var result = _service.GetById("1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Create_SetsCreatedAt_AndInserts()
        {
            // Arrange
            var note = new Note { Title = "Test", Content = "Content" };
            var originalTime = DateTime.Now;

            // Act
            _service.Create(note);

            // Assert
            Assert.True(note.CreatedAt >= originalTime);
            _mockCollection.Verify(c => c.InsertOne(note, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Update_CallsReplaceOne()
        {
            // Arrange
            var updatedNote = new Note { Id = "1", Title = "Updated", Content = "Updated Content" };

            // Act
            _service.Update("1", updatedNote);

            // Assert
            _mockCollection.Verify(c => c.ReplaceOne(It.IsAny<FilterDefinition<Note>>(), updatedNote, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void Delete_CallsDeleteOne()
        {
            // Arrange

            // Act
            _service.Delete("1");

            // Assert
            _mockCollection.Verify(c => c.DeleteOne(It.IsAny<FilterDefinition<Note>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}