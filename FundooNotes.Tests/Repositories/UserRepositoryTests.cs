using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Implementations;
using NUnit.Framework;

namespace FundooNotes.Tests.Repositories
{
    [TestFixture]
    public class UserRepositoryTests : TestBase
    {
        private UserRepository _userRepository = null!;

        protected override void OnSetUp()
        {
            _userRepository = new UserRepository(_context);
        }

        [Test]
        public async Task AddAsync_ShouldAddUserToDatabase()
        {
            var user = new User
            {
                FullName = "John Doe",
                Email = "john.doe@example.com",
                PasswordHash = "hashedpassword123",
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            var savedUser = await _context.Users.FindAsync(user.UserId);
            Assert.That(savedUser, Is.Not.Null);
            Assert.That(savedUser.FullName, Is.EqualTo("John Doe"));
            Assert.That(savedUser.Email, Is.EqualTo("john.doe@example.com"));
        }

        [Test]
        public async Task GetByEmailAsync_ShouldReturnUserIfExists()
        {
            var user = new User
            {
                FullName = "Jane Smith",
                Email = "jane.smith@example.com",
                PasswordHash = "hashedpassword",
                IsEmailVerified = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var retrievedUser = await _userRepository.GetByEmailAsync("jane.smith@example.com");

            Assert.That(retrievedUser, Is.Not.Null);
            Assert.That(retrievedUser.FullName, Is.EqualTo("Jane Smith"));
        }

        [Test]
        public async Task GetByEmailAsync_ShouldReturnNullIfUserDoesNotExist()
        {
            var retrievedUser = await _userRepository.GetByEmailAsync("nonexistent@example.com");

            Assert.That(retrievedUser, Is.Null);
        }

        [Test]
        public async Task GetByEmailVerificationTokenAsync_ShouldReturnUserWithValidToken()
        {
            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash",
                EmailVerificationToken = token,
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
                IsEmailVerified = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var retrievedUser = await _userRepository.GetByEmailVerificationTokenAsync(token);

            Assert.That(retrievedUser, Is.Not.Null);
            Assert.That(retrievedUser.Email, Is.EqualTo("test@example.com"));
        }

        [Test]
        public async Task GetByEmailVerificationTokenAsync_ShouldReturnNullIfTokenExpired()
        {
            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash",
                EmailVerificationToken = token,
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(-1),
                IsEmailVerified = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var retrievedUser = await _userRepository.GetByEmailVerificationTokenAsync(token);

            Assert.That(retrievedUser, Is.Null);
        }

        [Test]
        public async Task GetByResetPasswordTokenAsync_ShouldReturnUserWithValidToken()
        {
            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                FullName = "Reset User",
                Email = "reset@example.com",
                PasswordHash = "hash",
                ResetPasswordToken = token,
                ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(30),
                IsEmailVerified = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var retrievedUser = await _userRepository.GetByResetPasswordTokenAsync(token);

            Assert.That(retrievedUser, Is.Not.Null);
            Assert.That(retrievedUser.Email, Is.EqualTo("reset@example.com"));
        }

        [Test]
        public async Task GetByResetPasswordTokenAsync_ShouldReturnNullIfTokenExpired()
        {
            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                FullName = "Reset User",
                Email = "reset@example.com",
                PasswordHash = "hash",
                ResetPasswordToken = token,
                ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(-10),
                IsEmailVerified = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var retrievedUser = await _userRepository.GetByResetPasswordTokenAsync(token);

            Assert.That(retrievedUser, Is.Null);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateUserInDatabase()
        {
            var user = new User
            {
                FullName = "Original Name",
                Email = "original@example.com",
                PasswordHash = "hash",
                IsEmailVerified = false
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.FullName = "Updated Name";
            user.IsEmailVerified = true;
            await _userRepository.UpdateAsync(user);

            var updatedUser = await _context.Users.FindAsync(user.UserId);
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.FullName, Is.EqualTo("Updated Name"));
            Assert.That(updatedUser.IsEmailVerified, Is.True);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdatePasswordResetToken()
        {
            var user = new User
            {
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var resetToken = Guid.NewGuid().ToString();
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(30);
            await _userRepository.UpdateAsync(user);

            var updatedUser = await _context.Users.FindAsync(user.UserId);
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.ResetPasswordToken, Is.EqualTo(resetToken));
            Assert.That(updatedUser.ResetPasswordTokenExpiry, Is.Not.Null);
        }
    }
}
