using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Implementations;
using NUnit.Framework;

namespace FundooNotes.Tests.Repositories
{
    [TestFixture]
    public class NoteRepositoryTests : TestBase
    {
        private NoteRepository _noteRepository = null!;
        private User _testUser = null!;

        protected override void OnSetUp()
        {
            _noteRepository = new NoteRepository(_context);
            
            _testUser = new User
            {
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(_testUser);
            _context.SaveChanges();
        }

        [Test]
        public async Task AddAsync_ShouldAddNoteToDatabase()
        {
            var note = new Note
            {
                Title = "Test Note",
                Description = "Test Description",
                UserId = _testUser.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _noteRepository.AddAsync(note);

            var savedNote = await _context.Notes.FindAsync(note.NoteId);
            Assert.That(savedNote, Is.Not.Null);
            Assert.That(savedNote.Title, Is.EqualTo("Test Note"));
            Assert.That(savedNote.Description, Is.EqualTo("Test Description"));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateExistingNote()
        {
            var note = new Note
            {
                Title = "Original Title",
                Description = "Original Description",
                UserId = _testUser.UserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            note.Title = "Updated Title";
            note.Description = "Updated Description";
            note.UpdatedAt = DateTime.UtcNow;
            await _noteRepository.UpdateAsync(note);

            var updatedNote = await _context.Notes.FindAsync(note.NoteId);
            Assert.That(updatedNote, Is.Not.Null);
            Assert.That(updatedNote.Title, Is.EqualTo("Updated Title"));
            Assert.That(updatedNote.Description, Is.EqualTo("Updated Description"));
            Assert.That(updatedNote.UpdatedAt, Is.Not.Null);
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveNoteFromDatabase()
        {
            var note = new Note
            {
                Title = "Note to Delete",
                UserId = _testUser.UserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            await _noteRepository.DeleteAsync(note);

            var deletedNote = await _context.Notes.FindAsync(note.NoteId);
            Assert.That(deletedNote, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnOnlyNonDeletedNotesForUser()
        {
            var note1 = new Note { Title = "Note 1", UserId = _testUser.UserId, IsDeleted = false };
            var note2 = new Note { Title = "Note 2", UserId = _testUser.UserId, IsDeleted = false };
            var note3 = new Note { Title = "Note 3", UserId = _testUser.UserId, IsDeleted = true };
            
            _context.Notes.AddRange(note1, note2, note3);
            await _context.SaveChangesAsync();

            var notes = await _noteRepository.GetAllAsync(_testUser.UserId);

            Assert.That(notes.Count, Is.EqualTo(2));
            Assert.That(notes.All(n => !n.IsDeleted), Is.True);
        }

        [Test]
        public async Task GetByIdAsync_WithUserId_ShouldReturnNoteIfExists()
        {
            var note = new Note
            {
                Title = "Test Note",
                UserId = _testUser.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var retrievedNote = await _noteRepository.GetByIdAsync(note.NoteId, _testUser.UserId);

            Assert.That(retrievedNote, Is.Not.Null);
            Assert.That(retrievedNote.Title, Is.EqualTo("Test Note"));
        }

        [Test]
        public async Task GetByIdAsync_WithUserId_ShouldReturnNullIfDeleted()
        {
            var note = new Note
            {
                Title = "Deleted Note",
                UserId = _testUser.UserId,
                IsDeleted = true
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var retrievedNote = await _noteRepository.GetByIdAsync(note.NoteId, _testUser.UserId);

            Assert.That(retrievedNote, Is.Null);
        }

        [Test]
        public async Task GetByIdAsync_WithoutUserId_ShouldReturnNoteIfNotDeleted()
        {
            var note = new Note
            {
                Title = "Test Note",
                UserId = _testUser.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var retrievedNote = await _noteRepository.GetByIdAsync(note.NoteId);

            Assert.That(retrievedNote, Is.Not.Null);
            Assert.That(retrievedNote.Title, Is.EqualTo("Test Note"));
        }

        [Test]
        public async Task GetAllIncludingCollaborationsAsync_ShouldReturnOwnedAndCollaboratedNotes()
        {
            var user2 = new User
            {
                FullName = "User 2",
                Email = "user2@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            _context.Users.Add(user2);
            await _context.SaveChangesAsync();

            var ownedNote = new Note { Title = "Owned Note", UserId = _testUser.UserId, IsDeleted = false };
            var collaboratedNote = new Note { Title = "Collaborated Note", UserId = user2.UserId, IsDeleted = false };
            
            _context.Notes.AddRange(ownedNote, collaboratedNote);
            await _context.SaveChangesAsync();

            var collaborator = new Collaborator
            {
                NoteId = collaboratedNote.NoteId,
                UserId = _testUser.UserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            var notes = await _noteRepository.GetAllIncludingCollaborationsAsync(_testUser.UserId);

            Assert.That(notes.Count, Is.EqualTo(2));
            Assert.That(notes.Any(n => n.Title == "Owned Note"), Is.True);
            Assert.That(notes.Any(n => n.Title == "Collaborated Note"), Is.True);
        }

        [Test]
        public async Task GetDeletedAsync_ShouldReturnOnlyDeletedNotes()
        {
            var note1 = new Note { Title = "Active Note", UserId = _testUser.UserId, IsDeleted = false };
            var note2 = new Note { Title = "Deleted Note", UserId = _testUser.UserId, IsDeleted = true };
            
            _context.Notes.AddRange(note1, note2);
            await _context.SaveChangesAsync();

            var deletedNotes = await _noteRepository.GetDeletedAsync(_testUser.UserId);

            Assert.That(deletedNotes.Count, Is.EqualTo(1));
            Assert.That(deletedNotes[0].Title, Is.EqualTo("Deleted Note"));
        }

        [Test]
        public async Task GetDeletedByIdAsync_ShouldReturnDeletedNoteById()
        {
            var note = new Note
            {
                Title = "Deleted Note",
                UserId = _testUser.UserId,
                IsDeleted = true
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var retrievedNote = await _noteRepository.GetDeletedByIdAsync(note.NoteId, _testUser.UserId);

            Assert.That(retrievedNote, Is.Not.Null);
            Assert.That(retrievedNote.Title, Is.EqualTo("Deleted Note"));
        }

        [Test]
        public async Task GetDeletedByIdAsync_ShouldReturnNullForNonDeletedNote()
        {
            var note = new Note
            {
                Title = "Active Note",
                UserId = _testUser.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            var retrievedNote = await _noteRepository.GetDeletedByIdAsync(note.NoteId, _testUser.UserId);

            Assert.That(retrievedNote, Is.Null);
        }
    }
}
