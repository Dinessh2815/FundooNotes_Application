using BusinessLayer.Services;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using BusinessLayer.Interfaces;
using ModelLayer.DTOs;

namespace FundooNotes.Tests.Services
{
    [TestFixture]
    public class NoteServiceTests : TestBase
    {
        private NoteService _noteService = null!;
        private NoteRepository _noteRepository = null!;
        private Mock<ICollaboratorService> _mockCollaboratorService = null!;
        private User _testUser = null!;

        protected override void OnSetUp()
        {
            _noteRepository = new NoteRepository(_context);
            _mockCollaboratorService = new Mock<ICollaboratorService>();
            _noteService = new NoteService(_noteRepository, _context, _mockCollaboratorService.Object);
            
            _testUser = new User
            {
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            _context.Users.Add(_testUser);
            _context.SaveChanges();
        }

        [Test]
        public async Task CreateAsync_ShouldCreateNoteAndHistory()
        {
            var request = new CreateNoteRequestDto
            {
                Title = "Test Note",
                Description = "Test Description",
                Color = "#FFFFFF",
                IsPinned = false,
                IsArchived = false
            };

            await _noteService.CreateAsync(request, _testUser.UserId);

            var notes = await _noteRepository.GetAllAsync(_testUser.UserId);
            Assert.That(notes.Count, Is.EqualTo(1));
            Assert.That(notes[0].Title, Is.EqualTo("Test Note"));
            Assert.That(notes[0].Description, Is.EqualTo("Test Description"));

            var history = _context.NoteHistories.Where(h => h.NoteId == notes[0].NoteId).ToList();
            Assert.That(history.Count, Is.EqualTo(1));
            Assert.That(history[0].Action, Is.EqualTo("Created"));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllNotesForUser()
        {
            var note1 = new Note { Title = "Note 1", UserId = _testUser.UserId, IsDeleted = false };
            var note2 = new Note { Title = "Note 2", UserId = _testUser.UserId, IsDeleted = false };
            _context.Notes.AddRange(note1, note2);
            await _context.SaveChangesAsync();

            var notes = await _noteService.GetAllAsync(_testUser.UserId);

            Assert.That(notes.Count, Is.EqualTo(2));
            Assert.That(notes.Any(n => n.Title == "Note 1"), Is.True);
            Assert.That(notes.Any(n => n.Title == "Note 2"), Is.True);
        }

        [Test]
        public async Task UpdateAsync_OwnerShouldUpdateNote()
        {
            var note = new Note
            {
                Title = "Original Title",
                Description = "Original Description",
                UserId = _testUser.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateNoteRequestDto
            {
                Title = "Updated Title",
                Description = "Updated Description"
            };

            await _noteService.UpdateAsync(note.NoteId, updateRequest, _testUser.UserId);

            var updatedNote = await _noteRepository.GetByIdAsync(note.NoteId, _testUser.UserId);
            Assert.That(updatedNote, Is.Not.Null);
            Assert.That(updatedNote.Title, Is.EqualTo("Updated Title"));
            Assert.That(updatedNote.Description, Is.EqualTo("Updated Description"));
            Assert.That(updatedNote.UpdatedAt, Is.Not.Null);

            var history = _context.NoteHistories.Where(h => h.NoteId == note.NoteId).ToList();
            Assert.That(history.Any(h => h.Action == "Updated"), Is.True);
        }

        [Test]
        public async Task UpdateAsync_CollaboratorWithEditPermissionShouldUpdateNote()
        {
            var owner = new User
            {
                FullName = "Owner",
                Email = "owner@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            _context.Users.Add(owner);
            await _context.SaveChangesAsync();

            var note = new Note
            {
                Title = "Shared Note",
                UserId = owner.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            _mockCollaboratorService.Setup(s => s.CanEditAsync(note.NoteId, _testUser.UserId))
                .ReturnsAsync(true);

            var updateRequest = new UpdateNoteRequestDto
            {
                Title = "Updated by Collaborator"
            };

            await _noteService.UpdateAsync(note.NoteId, updateRequest, _testUser.UserId);

            var updatedNote = await _noteRepository.GetByIdAsync(note.NoteId);
            Assert.That(updatedNote, Is.Not.Null);
            Assert.That(updatedNote.Title, Is.EqualTo("Updated by Collaborator"));
        }

        [Test]
        public void UpdateAsync_CollaboratorWithoutEditPermissionShouldThrow()
        {
            var owner = new User
            {
                FullName = "Owner",
                Email = "owner@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            _context.Users.Add(owner);
            _context.SaveChanges();

            var note = new Note
            {
                Title = "Private Note",
                UserId = owner.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(note);
            _context.SaveChanges();

            _mockCollaboratorService.Setup(s => s.CanEditAsync(note.NoteId, _testUser.UserId))
                .ReturnsAsync(false);

            var updateRequest = new UpdateNoteRequestDto
            {
                Title = "Unauthorized Update"
            };

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await _noteService.UpdateAsync(note.NoteId, updateRequest, _testUser.UserId));
        }

        [Test]
        public async Task DeleteAsync_ShouldMarkNoteAsDeleted()
        {
            var note = new Note
            {
                Title = "Note to Delete",
                UserId = _testUser.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            await _noteService.DeleteAsync(note.NoteId, _testUser.UserId);

            var deletedNote = await _context.Notes.FindAsync(note.NoteId);
            Assert.That(deletedNote, Is.Not.Null);
            Assert.That(deletedNote.IsDeleted, Is.True);
            Assert.That(deletedNote.UpdatedAt, Is.Not.Null);

            var history = _context.NoteHistories.Where(h => h.NoteId == note.NoteId).ToList();
            Assert.That(history.Any(h => h.Action == "Deleted"), Is.True);
        }

        [Test]
        public void DeleteAsync_ShouldThrowIfNoteNotFound()
        {
            Assert.ThrowsAsync<Exception>(async () =>
                await _noteService.DeleteAsync(999, _testUser.UserId));
        }

        [Test]
        public async Task GetDeletedAsync_ShouldReturnOnlyDeletedNotes()
        {
            var activeNote = new Note { Title = "Active Note", UserId = _testUser.UserId, IsDeleted = false };
            var deletedNote = new Note { Title = "Deleted Note", UserId = _testUser.UserId, IsDeleted = true };
            _context.Notes.AddRange(activeNote, deletedNote);
            await _context.SaveChangesAsync();

            var deletedNotes = await _noteService.GetDeletedAsync(_testUser.UserId);

            Assert.That(deletedNotes.Count, Is.EqualTo(1));
            Assert.That(deletedNotes[0].Title, Is.EqualTo("Deleted Note"));
            Assert.That(deletedNotes[0].IsDeleted, Is.True);
        }

        [Test]
        public async Task RestoreAsync_ShouldRestoreDeletedNote()
        {
            var note = new Note
            {
                Title = "Deleted Note",
                UserId = _testUser.UserId,
                IsDeleted = true
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            await _noteService.RestoreAsync(note.NoteId, _testUser.UserId);

            var restoredNote = await _context.Notes.FindAsync(note.NoteId);
            Assert.That(restoredNote, Is.Not.Null);
            Assert.That(restoredNote.IsDeleted, Is.False);

            var history = _context.NoteHistories.Where(h => h.NoteId == note.NoteId).ToList();
            Assert.That(history.Any(h => h.Action == "Restored"), Is.True);
        }

        [Test]
        public void RestoreAsync_ShouldThrowIfNoteNotDeleted()
        {
            var note = new Note
            {
                Title = "Active Note",
                UserId = _testUser.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(note);
            _context.SaveChanges();

            Assert.ThrowsAsync<Exception>(async () =>
                await _noteService.RestoreAsync(note.NoteId, _testUser.UserId));
        }

        [Test]
        public async Task PermanentDeleteAsync_ShouldRemoveNoteFromDatabase()
        {
            var note = new Note
            {
                Title = "Note to Permanently Delete",
                UserId = _testUser.UserId,
                IsDeleted = true
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            var noteId = note.NoteId;

            await _noteService.PermanentDeleteAsync(noteId, _testUser.UserId);

            var deletedNote = await _context.Notes.FindAsync(noteId);
            Assert.That(deletedNote, Is.Null);

            var history = _context.NoteHistories.Where(h => h.NoteId == noteId).ToList();
            Assert.That(history.Any(h => h.Action == "Permanently Deleted"), Is.True);
        }

        [Test]
        public void PermanentDeleteAsync_ShouldThrowIfNoteNotDeleted()
        {
            var note = new Note
            {
                Title = "Active Note",
                UserId = _testUser.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(note);
            _context.SaveChanges();

            Assert.ThrowsAsync<Exception>(async () =>
                await _noteService.PermanentDeleteAsync(note.NoteId, _testUser.UserId));
        }

        [Test]
        public async Task UpdateAsync_ShouldOnlyUpdateProvidedFields()
        {
            var note = new Note
            {
                Title = "Original Title",
                Description = "Original Description",
                Color = "#FFFFFF",
                UserId = _testUser.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateNoteRequestDto
            {
                Title = "Updated Title"
            };

            await _noteService.UpdateAsync(note.NoteId, updateRequest, _testUser.UserId);

            var updatedNote = await _noteRepository.GetByIdAsync(note.NoteId, _testUser.UserId);
            Assert.That(updatedNote, Is.Not.Null);
            Assert.That(updatedNote.Title, Is.EqualTo("Updated Title"));
            Assert.That(updatedNote.Description, Is.EqualTo("Original Description"));
            Assert.That(updatedNote.Color, Is.EqualTo("#FFFFFF"));
        }
    }
}
