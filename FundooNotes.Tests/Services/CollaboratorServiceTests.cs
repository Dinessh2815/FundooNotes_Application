using BusinessLayer.Services;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Implementations;
using NUnit.Framework;
using ModelLayer.DTOs;

namespace FundooNotes.Tests.Services
{
    [TestFixture]
    public class CollaboratorServiceTests : TestBase
    {
        private CollaboratorService _collaboratorService = null!;
        private NoteRepository _noteRepository = null!;
        private UserRepository _userRepository = null!;
        private CollaboratorRepository _collaboratorRepository = null!;
        private User _owner = null!;
        private User _collaboratorUser = null!;
        private Note _testNote = null!;

        protected override void OnSetUp()
        {
            _noteRepository = new NoteRepository(_context);
            _userRepository = new UserRepository(_context);
            _collaboratorRepository = new CollaboratorRepository(_context);
            _collaboratorService = new CollaboratorService(_noteRepository, _userRepository, _collaboratorRepository);
            
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
                Description = "This note can be shared",
                UserId = _owner.UserId,
                IsDeleted = false
            };
            
            _context.Notes.Add(_testNote);
            _context.SaveChanges();
        }

        [Test]
        public async Task AddAsync_ShouldAddCollaboratorToNote()
        {
            var request = new AddCollaboratorRequestDto
            {
                Email = "collaborator@example.com",
                CanEdit = true
            };

            await _collaboratorService.AddAsync(_testNote.NoteId, request, _owner.UserId);

            var exists = await _collaboratorRepository.ExistsAsync(_testNote.NoteId, _collaboratorUser.UserId);
            Assert.That(exists, Is.True);

            var collaborator = await _collaboratorRepository.GetAsync(_testNote.NoteId, _collaboratorUser.UserId);
            Assert.That(collaborator, Is.Not.Null);
            Assert.That(collaborator.CanEdit, Is.True);
        }

        [Test]
        public async Task AddAsync_ShouldAddCollaboratorWithoutEditPermission()
        {
            var request = new AddCollaboratorRequestDto
            {
                Email = "collaborator@example.com",
                CanEdit = false
            };

            await _collaboratorService.AddAsync(_testNote.NoteId, request, _owner.UserId);

            var collaborator = await _collaboratorRepository.GetAsync(_testNote.NoteId, _collaboratorUser.UserId);
            Assert.That(collaborator, Is.Not.Null);
            Assert.That(collaborator.CanEdit, Is.False);
        }

        [Test]
        public void AddAsync_ShouldThrowIfNoteNotFound()
        {
            var request = new AddCollaboratorRequestDto
            {
                Email = "collaborator@example.com",
                CanEdit = true
            };

            Assert.ThrowsAsync<Exception>(async () =>
                await _collaboratorService.AddAsync(999, request, _owner.UserId));
        }

        [Test]
        public void AddAsync_ShouldThrowIfUserNotOwner()
        {
            var request = new AddCollaboratorRequestDto
            {
                Email = "collaborator@example.com",
                CanEdit = true
            };

            Assert.ThrowsAsync<Exception>(async () =>
                await _collaboratorService.AddAsync(_testNote.NoteId, request, _collaboratorUser.UserId));
        }

        [Test]
        public void AddAsync_ShouldThrowIfCollaboratorEmailNotFound()
        {
            var request = new AddCollaboratorRequestDto
            {
                Email = "nonexistent@example.com",
                CanEdit = true
            };

            Assert.ThrowsAsync<Exception>(async () =>
                await _collaboratorService.AddAsync(_testNote.NoteId, request, _owner.UserId));
        }

        [Test]
        public void AddAsync_ShouldThrowIfOwnerTriesToAddThemselves()
        {
            var request = new AddCollaboratorRequestDto
            {
                Email = "owner@example.com",
                CanEdit = true
            };

            Assert.ThrowsAsync<Exception>(async () =>
                await _collaboratorService.AddAsync(_testNote.NoteId, request, _owner.UserId));
        }

        [Test]
        public async Task AddAsync_ShouldNotThrowIfCollaboratorAlreadyExists()
        {
            var collaborator = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = _collaboratorUser.UserId,
                CanEdit = false
            };
            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            var request = new AddCollaboratorRequestDto
            {
                Email = "collaborator@example.com",
                CanEdit = true
            };

            Assert.DoesNotThrowAsync(async () =>
                await _collaboratorService.AddAsync(_testNote.NoteId, request, _owner.UserId));
        }

        [Test]
        public async Task RemoveAsync_ShouldRemoveCollaboratorFromNote()
        {
            var collaborator = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = _collaboratorUser.UserId,
                CanEdit = true
            };
            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            await _collaboratorService.RemoveAsync(_testNote.NoteId, _collaboratorUser.UserId, _owner.UserId);

            var exists = await _collaboratorRepository.ExistsAsync(_testNote.NoteId, _collaboratorUser.UserId);
            Assert.That(exists, Is.False);
        }

        [Test]
        public void RemoveAsync_ShouldThrowIfUserNotOwner()
        {
            Assert.ThrowsAsync<Exception>(async () =>
                await _collaboratorService.RemoveAsync(_testNote.NoteId, _collaboratorUser.UserId, _collaboratorUser.UserId));
        }

        [Test]
        public void RemoveAsync_ShouldThrowIfNoteNotFound()
        {
            Assert.ThrowsAsync<Exception>(async () =>
                await _collaboratorService.RemoveAsync(999, _collaboratorUser.UserId, _owner.UserId));
        }

        [Test]
        public async Task GetAsync_ShouldReturnAllCollaboratorsForNote()
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

            var collaborators = await _collaboratorService.GetAsync(_testNote.NoteId, _owner.UserId);

            Assert.That(collaborators.Count, Is.EqualTo(2));
            Assert.That(collaborators.Any(c => c.Email == "collaborator@example.com"), Is.True);
            Assert.That(collaborators.Any(c => c.Email == "user2@example.com"), Is.True);
        }

        [Test]
        public async Task GetAsync_ShouldReturnEmptyListIfNoCollaborators()
        {
            var collaborators = await _collaboratorService.GetAsync(_testNote.NoteId, _owner.UserId);

            Assert.That(collaborators, Is.Not.Null);
            Assert.That(collaborators.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task CanEditAsync_ShouldReturnTrueIfCollaboratorHasEditPermission()
        {
            var collaborator = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = _collaboratorUser.UserId,
                CanEdit = true
            };
            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            var canEdit = await _collaboratorService.CanEditAsync(_testNote.NoteId, _collaboratorUser.UserId);

            Assert.That(canEdit, Is.True);
        }

        [Test]
        public async Task CanEditAsync_ShouldReturnFalseIfCollaboratorDoesNotHaveEditPermission()
        {
            var collaborator = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = _collaboratorUser.UserId,
                CanEdit = false
            };
            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            var canEdit = await _collaboratorService.CanEditAsync(_testNote.NoteId, _collaboratorUser.UserId);

            Assert.That(canEdit, Is.False);
        }

        [Test]
        public async Task CanEditAsync_ShouldReturnFalseIfUserIsNotCollaborator()
        {
            var canEdit = await _collaboratorService.CanEditAsync(_testNote.NoteId, _collaboratorUser.UserId);

            Assert.That(canEdit, Is.False);
        }

        [Test]
        public async Task GetAsync_ShouldIncludeCanEditProperty()
        {
            var collaborator = new Collaborator
            {
                NoteId = _testNote.NoteId,
                UserId = _collaboratorUser.UserId,
                CanEdit = true
            };
            _context.Collaborators.Add(collaborator);
            await _context.SaveChangesAsync();

            var collaborators = await _collaboratorService.GetAsync(_testNote.NoteId, _owner.UserId);

            Assert.That(collaborators.Count, Is.EqualTo(1));
            Assert.That(collaborators[0].CanEdit, Is.True);
            Assert.That(collaborators[0].Email, Is.EqualTo("collaborator@example.com"));
            Assert.That(collaborators[0].UserId, Is.EqualTo(_collaboratorUser.UserId));
        }
    }
}
