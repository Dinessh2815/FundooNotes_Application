using BusinessLayer.Services;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Implementations;
using NUnit.Framework;
using ModelLayer.DTOs;

namespace FundooNotes.Tests.Services
{
    [TestFixture]
    public class LabelServiceTests : TestBase
    {
        private LabelService _labelService = null!;
        private LabelRepository _labelRepository = null!;
        private User _testUser = null!;

        protected override void OnSetUp()
        {
            _labelRepository = new LabelRepository(_context);
            _labelService = new LabelService(_labelRepository);
            
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
        public async Task CreateAsync_ShouldCreateLabel()
        {
            var request = new CreateLabelRequestDto
            {
                Name = "Work"
            };

            await _labelService.CreateAsync(request, _testUser.UserId);

            var labels = await _labelRepository.GetAllAsync(_testUser.UserId);
            Assert.That(labels.Count, Is.EqualTo(1));
            Assert.That(labels[0].Name, Is.EqualTo("Work"));
            Assert.That(labels[0].UserId, Is.EqualTo(_testUser.UserId));
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnAllLabelsForUser()
        {
            var label1 = new Label { Name = "Personal", UserId = _testUser.UserId };
            var label2 = new Label { Name = "Work", UserId = _testUser.UserId };
            _context.Labels.AddRange(label1, label2);
            await _context.SaveChangesAsync();

            var labels = await _labelService.GetAllAsync(_testUser.UserId);

            Assert.That(labels.Count, Is.EqualTo(2));
            Assert.That(labels.Any(l => l.Name == "Personal"), Is.True);
            Assert.That(labels.Any(l => l.Name == "Work"), Is.True);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnEmptyListIfNoLabels()
        {
            var labels = await _labelService.GetAllAsync(_testUser.UserId);

            Assert.That(labels, Is.Not.Null);
            Assert.That(labels.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllAsync_ShouldNotReturnOtherUsersLabels()
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

            var myLabel = new Label { Name = "My Label", UserId = _testUser.UserId };
            var otherLabel = new Label { Name = "Other Label", UserId = user2.UserId };
            _context.Labels.AddRange(myLabel, otherLabel);
            await _context.SaveChangesAsync();

            var labels = await _labelService.GetAllAsync(_testUser.UserId);

            Assert.That(labels.Count, Is.EqualTo(1));
            Assert.That(labels[0].Name, Is.EqualTo("My Label"));
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateLabelName()
        {
            var label = new Label
            {
                Name = "Original Name",
                UserId = _testUser.UserId
            };
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateLabelRequestDto
            {
                Name = "Updated Name"
            };

            await _labelService.UpdateAsync(label.LabelId, updateRequest, _testUser.UserId);

            var updatedLabel = await _labelRepository.GetByIdAsync(label.LabelId, _testUser.UserId);
            Assert.That(updatedLabel, Is.Not.Null);
            Assert.That(updatedLabel.Name, Is.EqualTo("Updated Name"));
        }

        [Test]
        public void UpdateAsync_ShouldThrowIfLabelNotFound()
        {
            var updateRequest = new UpdateLabelRequestDto
            {
                Name = "New Name"
            };

            Assert.ThrowsAsync<Exception>(async () =>
                await _labelService.UpdateAsync(999, updateRequest, _testUser.UserId));
        }

        [Test]
        public void UpdateAsync_ShouldThrowIfLabelBelongsToAnotherUser()
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

            var label = new Label
            {
                Name = "User 2 Label",
                UserId = user2.UserId
            };
            _context.Labels.Add(label);
            _context.SaveChanges();

            var updateRequest = new UpdateLabelRequestDto
            {
                Name = "Hacked Name"
            };

            Assert.ThrowsAsync<Exception>(async () =>
                await _labelService.UpdateAsync(label.LabelId, updateRequest, _testUser.UserId));
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
            var labelId = label.LabelId;

            await _labelService.DeleteAsync(labelId, _testUser.UserId);

            var deletedLabel = await _context.Labels.FindAsync(labelId);
            Assert.That(deletedLabel, Is.Null);
        }

        [Test]
        public void DeleteAsync_ShouldThrowIfLabelNotFound()
        {
            Assert.ThrowsAsync<Exception>(async () =>
                await _labelService.DeleteAsync(999, _testUser.UserId));
        }

        [Test]
        public void DeleteAsync_ShouldThrowIfLabelBelongsToAnotherUser()
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

            var label = new Label
            {
                Name = "User 2 Label",
                UserId = user2.UserId
            };
            _context.Labels.Add(label);
            _context.SaveChanges();

            Assert.ThrowsAsync<Exception>(async () =>
                await _labelService.DeleteAsync(label.LabelId, _testUser.UserId));
        }

        [Test]
        public async Task CreateAsync_ShouldAllowDuplicateNamesForDifferentUsers()
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

            var request = new CreateLabelRequestDto { Name = "Shared Name" };

            await _labelService.CreateAsync(request, _testUser.UserId);
            await _labelService.CreateAsync(request, user2.UserId);

            var user1Labels = await _labelRepository.GetAllAsync(_testUser.UserId);
            var user2Labels = await _labelRepository.GetAllAsync(user2.UserId);

            Assert.That(user1Labels.Count, Is.EqualTo(1));
            Assert.That(user2Labels.Count, Is.EqualTo(1));
            Assert.That(user1Labels[0].Name, Is.EqualTo("Shared Name"));
            Assert.That(user2Labels[0].Name, Is.EqualTo("Shared Name"));
        }
    }
}
