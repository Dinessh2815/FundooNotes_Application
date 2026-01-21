using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Implementations;
using NUnit.Framework;

namespace FundooNotes.Tests.Repositories
{
    [TestFixture]
    public class NoteLabelRepositoryTests : TestBase
    {
        private NoteLabelRepository _noteLabelRepository = null!;
        private User _testUser = null!;
        private Note _testNote = null!;
        private Label _testLabel1 = null!;
        private Label _testLabel2 = null!;

        protected override void OnSetUp()
        {
            _noteLabelRepository = new NoteLabelRepository(_context);
            
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
                UserId = _testUser.UserId
            };
            _context.Notes.Add(_testNote);
            _context.SaveChanges();

            _testLabel1 = new Label
            {
                Name = "Work",
                UserId = _testUser.UserId
            };
            
            _testLabel2 = new Label
            {
                Name = "Personal",
                UserId = _testUser.UserId
            };
            
            _context.Labels.AddRange(_testLabel1, _testLabel2);
            _context.SaveChanges();
        }

        [Test]
        public async Task AddAsync_ShouldCreateNoteLabelMapping()
        {
            var noteLabel = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel1.LabelId
            };

            await _noteLabelRepository.AddAsync(noteLabel);

            var exists = await _noteLabelRepository.ExistsAsync(_testNote.NoteId, _testLabel1.LabelId);
            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task ExistsAsync_ShouldReturnTrueIfMappingExists()
        {
            var noteLabel = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel1.LabelId
            };
            _context.NoteLabels.Add(noteLabel);
            await _context.SaveChangesAsync();

            var exists = await _noteLabelRepository.ExistsAsync(_testNote.NoteId, _testLabel1.LabelId);

            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task ExistsAsync_ShouldReturnFalseIfMappingDoesNotExist()
        {
            var exists = await _noteLabelRepository.ExistsAsync(_testNote.NoteId, _testLabel1.LabelId);

            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task RemoveAsync_ShouldRemoveNoteLabelMapping()
        {
            var noteLabel = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel1.LabelId
            };
            _context.NoteLabels.Add(noteLabel);
            await _context.SaveChangesAsync();

            await _noteLabelRepository.RemoveAsync(_testNote.NoteId, _testLabel1.LabelId);

            var exists = await _noteLabelRepository.ExistsAsync(_testNote.NoteId, _testLabel1.LabelId);
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task RemoveAsync_ShouldNotThrowIfMappingDoesNotExist()
        {
            Assert.DoesNotThrowAsync(async () =>
            {
                await _noteLabelRepository.RemoveAsync(_testNote.NoteId, 999);
            });
        }

        [Test]
        public async Task GetLabelsByNoteIdAsync_ShouldReturnAllLabelsForNote()
        {
            var noteLabel1 = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel1.LabelId
            };
            var noteLabel2 = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel2.LabelId
            };
            _context.NoteLabels.AddRange(noteLabel1, noteLabel2);
            await _context.SaveChangesAsync();

            var labels = await _noteLabelRepository.GetLabelsByNoteIdAsync(_testNote.NoteId, _testUser.UserId);

            Assert.That(labels.Count, Is.EqualTo(2));
            Assert.That(labels.Any(l => l.Name == "Work"), Is.True);
            Assert.That(labels.Any(l => l.Name == "Personal"), Is.True);
        }

        [Test]
        public async Task GetLabelsByNoteIdAsync_ShouldReturnEmptyListIfNoLabels()
        {
            var labels = await _noteLabelRepository.GetLabelsByNoteIdAsync(_testNote.NoteId, _testUser.UserId);

            Assert.That(labels, Is.Not.Null);
            Assert.That(labels.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetLabelsByNoteIdAsync_ShouldFilterByUserId()
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

            var noteLabel = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel1.LabelId
            };
            _context.NoteLabels.Add(noteLabel);
            await _context.SaveChangesAsync();

            var labels = await _noteLabelRepository.GetLabelsByNoteIdAsync(_testNote.NoteId, user2.UserId);

            Assert.That(labels.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task AddAsync_ShouldAllowMultipleLabelsForSameNote()
        {
            var noteLabel1 = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel1.LabelId
            };
            var noteLabel2 = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel2.LabelId
            };

            await _noteLabelRepository.AddAsync(noteLabel1);
            await _noteLabelRepository.AddAsync(noteLabel2);

            var exists1 = await _noteLabelRepository.ExistsAsync(_testNote.NoteId, _testLabel1.LabelId);
            var exists2 = await _noteLabelRepository.ExistsAsync(_testNote.NoteId, _testLabel2.LabelId);

            Assert.That(exists1, Is.True);
            Assert.That(exists2, Is.True);
        }

        [Test]
        public async Task AddAsync_ShouldAllowSameLabelForMultipleNotes()
        {
            var note2 = new Note
            {
                Title = "Another Note",
                UserId = _testUser.UserId
            };
            _context.Notes.Add(note2);
            await _context.SaveChangesAsync();

            var noteLabel1 = new NoteLabel
            {
                NoteId = _testNote.NoteId,
                LabelId = _testLabel1.LabelId
            };
            var noteLabel2 = new NoteLabel
            {
                NoteId = note2.NoteId,
                LabelId = _testLabel1.LabelId
            };

            await _noteLabelRepository.AddAsync(noteLabel1);
            await _noteLabelRepository.AddAsync(noteLabel2);

            var exists1 = await _noteLabelRepository.ExistsAsync(_testNote.NoteId, _testLabel1.LabelId);
            var exists2 = await _noteLabelRepository.ExistsAsync(note2.NoteId, _testLabel1.LabelId);

            Assert.That(exists1, Is.True);
            Assert.That(exists2, Is.True);
        }
    }
}
