using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Implementations;
using NUnit.Framework;

namespace FundooNotes.Tests.Repositories
{
    [TestFixture]
    public class LabelRepositoryTests : TestBase
    {
        private LabelRepository _labelRepository = null!;
        private User _testUser = null!;

        protected override void OnSetUp()
        {
            _labelRepository = new LabelRepository(_context);
            
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
        public async Task AddAsync_ShouldAddLabelToDatabase()
        {
            var label = new Label
            {
                Name = "Work",
                UserId = _testUser.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _labelRepository.AddAsync(label);

            var savedLabel = await _context.Labels.FindAsync(label.LabelId);
            Assert.That(savedLabel, Is.Not.Null);
            Assert.That(savedLabel.Name, Is.EqualTo("Work"));
            Assert.That(savedLabel.UserId, Is.EqualTo(_testUser.UserId));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllLabelsForUser()
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

            var label1 = new Label { Name = "Personal", UserId = _testUser.UserId };
            var label2 = new Label { Name = "Work", UserId = _testUser.UserId };
            var label3 = new Label { Name = "Other User Label", UserId = user2.UserId };
            
            _context.Labels.AddRange(label1, label2, label3);
            await _context.SaveChangesAsync();

            var labels = await _labelRepository.GetAllAsync(_testUser.UserId);

            Assert.That(labels.Count, Is.EqualTo(2));
            Assert.That(labels.All(l => l.UserId == _testUser.UserId), Is.True);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnLabelIfExistsForUser()
        {
            var label = new Label
            {
                Name = "Important",
                UserId = _testUser.UserId
            };
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            var retrievedLabel = await _labelRepository.GetByIdAsync(label.LabelId, _testUser.UserId);

            Assert.That(retrievedLabel, Is.Not.Null);
            Assert.That(retrievedLabel.Name, Is.EqualTo("Important"));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNullIfLabelDoesNotBelongToUser()
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

            var label = new Label
            {
                Name = "Private Label",
                UserId = user2.UserId
            };
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            var retrievedLabel = await _labelRepository.GetByIdAsync(label.LabelId, _testUser.UserId);

            Assert.That(retrievedLabel, Is.Null);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateExistingLabel()
        {
            var label = new Label
            {
                Name = "Original Name",
                UserId = _testUser.UserId
            };
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            label.Name = "Updated Name";
            await _labelRepository.UpdateAsync(label);

            var updatedLabel = await _context.Labels.FindAsync(label.LabelId);
            Assert.That(updatedLabel, Is.Not.Null);
            Assert.That(updatedLabel.Name, Is.EqualTo("Updated Name"));
        }

        [Test]
        public async Task DeleteAsync_ShouldRemoveLabelFromDatabase()
        {
            var label = new Label
            {
                Name = "Label to Delete",
                UserId = _testUser.UserId
            };
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            await _labelRepository.DeleteAsync(label);

            var deletedLabel = await _context.Labels.FindAsync(label.LabelId);
            Assert.That(deletedLabel, Is.Null);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnEmptyListIfNoLabels()
        {
            var labels = await _labelRepository.GetAllAsync(_testUser.UserId);

            Assert.That(labels, Is.Not.Null);
            Assert.That(labels.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnNullIfLabelDoesNotExist()
        {
            var retrievedLabel = await _labelRepository.GetByIdAsync(999, _testUser.UserId);

            Assert.That(retrievedLabel, Is.Null);
        }
    }
}
