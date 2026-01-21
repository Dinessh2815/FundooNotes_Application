using BusinessLayer.Services;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Implementations;
using NUnit.Framework;

namespace FundooNotes.Tests.Services
{
    [TestFixture]
    public class NoteLabelServiceTests : TestBase
    {
        private NoteLabelService _noteLabelService = null!;
        private NoteRepository _noteRepository = null!;
        private LabelRepository _labelRepository = null!;
        private NoteLabelRepository _noteLabelRepository = null!;
        private User _testUser = null!;
        private Note _testNote = null!;
        private Label _testLabel = null!;

        protected override void OnSetUp()
        {
            _noteRepository = new NoteRepository(_context);
            _labelRepository = new LabelRepository(_context);
            _noteLabelRepository = new NoteLabelRepository(_context);
            _noteLabelService = new NoteLabelService(_noteRepository, _labelRepository, _noteLabelRepository);
            
            _testUser = new User
            {
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            _context.Users.Add(_testUser);
            _context.SaveChanges();

            _testNote = new Note
            {
                Title = "Test Note",
                Description = "Test Description",
                UserId = _testUser.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(_testNote);
            _context.SaveChanges();

            _testLabel = new Label
            {
                Name = "Work",
                UserId = _testUser.UserId
            };
            _context.Labels.Add(_testLabel);
            _context.SaveChanges();
        }

        [Test]
        public async Task AddLabelAsync_ShouldAssignLabelToNote()
        {
            await _noteLabelService.AddLabelAsync(_testNote.NoteId, _testLabel.LabelId, _testUser.UserId);

            var exists = await _noteLabelRepository.ExistsAsync(_testNote.NoteId, _testLabel.LabelId);
            Assert.That(exists, Is.True);
        }

        [Test]
        public void AddLabelAsync_ShouldThrowIfNoteNotFound()
        {
            Assert.ThrowsAsync<Exception>(async () =>
                await _noteLabelService.AddLabelAsync(999, _testLabel.LabelId, _testUser.UserId));
        }

        [Test]
        public void AddLabelAsync_ShouldThrowIfLabelNotFound()
        {
            Assert.ThrowsAsync<Exception>(async () =>
                await _noteLabelService.AddLabelAsync(_testNote.NoteId, 999, _testUser.UserId));
        }

        [Test]
        public void AddLabelAsync_ShouldThrowIfNoteDoesNotBelongToUser()
        {
            var user2 = new User
            {
                FullName = "User 2",
                Email = "user2@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            _context.Users.Add(user2);
            _context.SaveChanges();

            Assert.ThrowsAsync<Exception>(async () =>
                await _noteLabelService.AddLabelAsync(_testNote.NoteId, _testLabel.LabelId, user2.UserId));
        }

        [Test]
        public void AddLabelAsync_ShouldThrowIfLabelDoesNotBelongToUser()
        {
            var user2 = new User
            {
                FullName = "User 2",
                Email = "user2@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            _context.Users.Add(user2);
            _context.SaveChanges();

            var label2 = new Label
            {
                Name = "User 2 Label",
                UserId = user2.UserId
            };
            _context.Labels.Add(label2);
            _context.SaveChanges();

            Assert.ThrowsAsync<Exception>(async () =>
                await _noteLabelService.AddLabelAsync(_testNote.NoteId, label2.LabelId, _testUser.UserId));
        }

        [Test]
        public async Task AddLabelAsync_ShouldNotThrowIfMappingAlreadyExists()
        {
            var noteLabel = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel.LabelId
            };
            _context.NoteLabels.Add(noteLabel);
            await _context.SaveChangesAsync();

            Assert.DoesNotThrowAsync(async () =>
                await _noteLabelService.AddLabelAsync(_testNote.NoteId, _testLabel.LabelId, _testUser.UserId));
        }

        [Test]
        public async Task RemoveLabelAsync_ShouldRemoveLabelFromNote()
        {
            var noteLabel = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel.LabelId
            };
            _context.NoteLabels.Add(noteLabel);
            await _context.SaveChangesAsync();

            await _noteLabelService.RemoveLabelAsync(_testNote.NoteId, _testLabel.LabelId, _testUser.UserId);

            var exists = await _noteLabelRepository.ExistsAsync(_testNote.NoteId, _testLabel.LabelId);
            Assert.That(exists, Is.False);
        }

        [Test]
        public void RemoveLabelAsync_ShouldThrowIfNoteNotFound()
        {
            Assert.ThrowsAsync<Exception>(async () =>
                await _noteLabelService.RemoveLabelAsync(999, _testLabel.LabelId, _testUser.UserId));
        }

        [Test]
        public void RemoveLabelAsync_ShouldThrowIfNoteDoesNotBelongToUser()
        {
            var user2 = new User
            {
                FullName = "User 2",
                Email = "user2@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            _context.Users.Add(user2);
            _context.SaveChanges();

            Assert.ThrowsAsync<Exception>(async () =>
                await _noteLabelService.RemoveLabelAsync(_testNote.NoteId, _testLabel.LabelId, user2.UserId));
        }

        [Test]
        public async Task GetLabelsAsync_ShouldReturnAllLabelsForNote()
        {
            var label2 = new Label
            {
                Name = "Personal",
                UserId = _testUser.UserId
            };
            _context.Labels.Add(label2);
            await _context.SaveChangesAsync();

            var noteLabel1 = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel.LabelId
            };
            var noteLabel2 = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = label2.LabelId
            };
            _context.NoteLabels.AddRange(noteLabel1, noteLabel2);
            await _context.SaveChangesAsync();

            var labels = await _noteLabelService.GetLabelsAsync(_testNote.NoteId, _testUser.UserId);

            Assert.That(labels.Count, Is.EqualTo(2));
            Assert.That(labels.Any(l => l.Name == "Work"), Is.True);
            Assert.That(labels.Any(l => l.Name == "Personal"), Is.True);
        }

        [Test]
        public async Task GetLabelsAsync_ShouldReturnEmptyListIfNoLabels()
        {
            var labels = await _noteLabelService.GetLabelsAsync(_testNote.NoteId, _testUser.UserId);

            Assert.That(labels, Is.Not.Null);
            Assert.That(labels.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task AddLabelAsync_ShouldAllowMultipleLabelsOnSameNote()
        {
            var label2 = new Label
            {
                Name = "Personal",
                UserId = _testUser.UserId
            };
            _context.Labels.Add(label2);
            await _context.SaveChangesAsync();

            await _noteLabelService.AddLabelAsync(_testNote.NoteId, _testLabel.LabelId, _testUser.UserId);
            await _noteLabelService.AddLabelAsync(_testNote.NoteId, label2.LabelId, _testUser.UserId);

            var labels = await _noteLabelService.GetLabelsAsync(_testNote.NoteId, _testUser.UserId);
            Assert.That(labels.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task AddLabelAsync_ShouldAllowSameLabelOnMultipleNotes()
        {
            var note2 = new Note
            {
                Title = "Another Note",
                UserId = _testUser.UserId,
                IsDeleted = false
            };
            _context.Notes.Add(note2);
            await _context.SaveChangesAsync();

            await _noteLabelService.AddLabelAsync(_testNote.NoteId, _testLabel.LabelId, _testUser.UserId);
            await _noteLabelService.AddLabelAsync(note2.NoteId, _testLabel.LabelId, _testUser.UserId);

            var labels1 = await _noteLabelService.GetLabelsAsync(_testNote.NoteId, _testUser.UserId);
            var labels2 = await _noteLabelService.GetLabelsAsync(note2.NoteId, _testUser.UserId);

            Assert.That(labels1.Count, Is.EqualTo(1));
            Assert.That(labels2.Count, Is.EqualTo(1));
        }
    }
}
