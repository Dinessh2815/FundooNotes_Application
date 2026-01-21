using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Implementations;
using NUnit.Framework;

namespace FundooNotes.Tests.Repositories
{
    [TestFixture]
    public class CollaboratorRepositoryTests : TestBase
    {
        private CollaboratorRepository _collaboratorRepository = null!;
        private User _owner = null!;
        private User _collaboratorUser = null!;
        private Note _testNote = null!;

        protected override void OnSetUp()
        {
            _collaboratorRepository = new CollaboratorRepository(_context);
            
            _owner = new User
            {
                FullName = "Owner User",
                Email = "owner@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            
            _collaboratorUser = new User
            {
                FullName = "Collaborator User",
                Email = "collaborator@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            
            _context.Users.AddRange(_owner, _collaboratorUser);
            _context.SaveChanges();

            _testNote = new Note
            {
                Title = "Shared Note",
                Description = "This note is shared",
                UserId = _owner.UserId,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Notes.Add(_testNote);
            _context.SaveChanges();
        }

        [Test]
        public async Task AddAsync_ShouldAddCollaboratorToDatabase()
        {
            var collaborator = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = _collaboratorUser.UserId,
                CanEdit = true,
                CreatedAt = DateTime.UtcNow
            };

            await _collaboratorRepository.AddAsync(collaborator);

            var savedCollaborator = await _context.Collaborators.FindAsync(collaborator.CollaboratorId);
            Assert.That(savedCollaborator, Is.Not.Null);
            Assert.That(savedCollaborator.NoteId, Is.EqualTo(_testNote.NoteId));
            Assert.That(savedCollaborator.UserId, Is.EqualTo(_collaboratorUser.UserId));
            Assert.That(savedCollaborator.CanEdit, Is.True);
        }

        [Test]
        public async Task ExistsAsync_ShouldReturnTrueIfCollaboratorExists()
        {
            var collaborator = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = _collaboratorUser.UserId,
                CanEdit = false
            };
            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            var exists = await _collaboratorRepository.ExistsAsync(_testNote.NoteId, _collaboratorUser.UserId);

            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task ExistsAsync_ShouldReturnFalseIfCollaboratorDoesNotExist()
        {
            var exists = await _collaboratorRepository.ExistsAsync(_testNote.NoteId, 999);

            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task GetAsync_ShouldReturnCollaboratorIfExists()
        {
            var collaborator = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = _collaboratorUser.UserId,
                CanEdit = true
            };
            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            var retrievedCollaborator = await _collaboratorRepository.GetAsync(_testNote.NoteId, _collaboratorUser.UserId);

            Assert.That(retrievedCollaborator, Is.Not.Null);
            Assert.That(retrievedCollaborator.CanEdit, Is.True);
        }

        [Test]
        public async Task GetAsync_ShouldReturnNullIfCollaboratorDoesNotExist()
        {
            var retrievedCollaborator = await _collaboratorRepository.GetAsync(_testNote.NoteId, 999);

            Assert.That(retrievedCollaborator, Is.Null);
        }

        [Test]
        public async Task GetByNoteIdAsync_ShouldReturnAllCollaboratorsForNote()
        {
            var user2 = new User
            {
                FullName = "User 2",
                Email = "user2@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            var user3 = new User
            {
                FullName = "User 3",
                Email = "user3@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            _context.Users.AddRange(user2, user3);
            await _context.SaveChangesAsync();

            var collaborator1 = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = _collaboratorUser.UserId,
                CanEdit = true
            };
            var collaborator2 = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = user2.UserId,
                CanEdit = false
            };
            _context.Collaborators.AddRange(collaborator1, collaborator2);
            await _context.SaveChangesAsync();

            var collaborators = await _collaboratorRepository.GetByNoteIdAsync(_testNote.NoteId);

            Assert.That(collaborators.Count, Is.EqualTo(2));
            Assert.That(collaborators.Any(c => c.UserId == _collaboratorUser.UserId), Is.True);
            Assert.That(collaborators.Any(c => c.UserId == user2.UserId), Is.True);
        }

        [Test]
        public async Task GetByNoteIdAsync_ShouldIncludeUserInformation()
        {
            var collaborator = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = _collaboratorUser.UserId,
                CanEdit = false
            };
            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            var collaborators = await _collaboratorRepository.GetByNoteIdAsync(_testNote.NoteId);

            Assert.That(collaborators.Count, Is.EqualTo(1));
            Assert.That(collaborators[0].User, Is.Not.Null);
            Assert.That(collaborators[0].User.Email, Is.EqualTo("collaborator@example.com"));
        }

        [Test]
        public async Task RemoveAsync_ShouldRemoveCollaboratorFromDatabase()
        {
            var collaborator = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = _collaboratorUser.UserId,
                CanEdit = false
            };
            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            await _collaboratorRepository.RemoveAsync(_testNote.NoteId, _collaboratorUser.UserId);

            var exists = await _collaboratorRepository.ExistsAsync(_testNote.NoteId, _collaboratorUser.UserId);
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task RemoveAsync_ShouldNotThrowIfCollaboratorDoesNotExist()
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                await _collaboratorRepository.RemoveAsync(_testNote.NoteId, 999);
            });
        }

        [Test]
        public async Task GetByNoteIdAsync_ShouldReturnEmptyListIfNoCollaborators()
        {
            var collaborators = await _collaboratorRepository.GetByNoteIdAsync(_testNote.NoteId);

            Assert.That(collaborators, Is.Not.Null);
            Assert.That(collaborators.Count, Is.EqualTo(0));
        }
    }
}
