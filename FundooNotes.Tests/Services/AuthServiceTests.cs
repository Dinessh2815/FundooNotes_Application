using BusinessLayer.Services;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Implementations;
using Moq;
using NUnit.Framework;
using BusinessLayer.Interfaces;
using ModelLayer.DTOs.Auth;
using Microsoft.Extensions.Configuration;

namespace FundooNotes.Tests.Services
{
    [TestFixture]
    public class AuthServiceTests : TestBase
    {
        private AuthService _authService = null!;
        private UserRepository _userRepository = null!;
        private Mock<IEmailService> _mockEmailService = null!;

        protected override void OnSetUp()
        {
            _userRepository = new UserRepository(_context);
            _mockEmailService = new Mock<IEmailService>();
            _authService = new AuthService(_userRepository, _configuration, _mockEmailService.Object);
        }

        [Test]
        public async Task RegisterAsync_ShouldCreateUserWithHashedPassword()
        {
            var request = new RegisterRequestDto
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "SecurePassword123"
            };

            await _authService.RegisterAsync(request);

            var user = await _userRepository.GetByEmailAsync("john@example.com");
            Assert.That(user, Is.Not.Null);
            Assert.That(user.FullName, Is.EqualTo("John Doe"));
            Assert.That(user.PasswordHash, Is.Not.EqualTo("SecurePassword123"));
            Assert.That(user.IsEmailVerified, Is.False);
            Assert.That(user.EmailVerificationToken, Is.Not.Null);
        }

        [Test]
        public async Task RegisterAsync_ShouldSendVerificationEmail()
        {
            var request = new RegisterRequestDto
            {
                FullName = "Jane Doe",
                Email = "jane@example.com",
                Password = "Password123"
            };

            await _authService.RegisterAsync(request);

            _mockEmailService.Verify(
                e => e.SendEmailAsync(
                    "jane@example.com",
                    "Verify your email",
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }

        [Test]
        public void RegisterAsync_ShouldThrowIfEmailAlreadyExists()
        {
            var existingUser = new User
            {
                FullName = "Existing User",
                Email = "existing@example.com",
                PasswordHash = "hash",
                IsEmailVerified = true
            };
            _context.Users.Add(existingUser);
            _context.SaveChanges();

            var request = new RegisterRequestDto
            {
                FullName = "New User",
                Email = "existing@example.com",
                Password = "Password123"
            };

            Assert.ThrowsAsync<Exception>(async () => await _authService.RegisterAsync(request));
        }

        [Test]
        public async Task VerifyEmailAsync_ShouldMarkEmailAsVerified()
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

            await _authService.VerifyEmailAsync(token);

            var verifiedUser = await _userRepository.GetByEmailAsync("test@example.com");
            Assert.That(verifiedUser, Is.Not.Null);
            Assert.That(verifiedUser.IsEmailVerified, Is.True);
            Assert.That(verifiedUser.EmailVerificationToken, Is.Null);
            Assert.That(verifiedUser.EmailVerificationTokenExpiry, Is.Null);
        }

        [Test]
        public void VerifyEmailAsync_ShouldThrowIfTokenInvalid()
        {
            var invalidToken = Guid.NewGuid().ToString();

            Assert.ThrowsAsync<Exception>(async () => await _authService.VerifyEmailAsync(invalidToken));
        }

        [Test]
        public void VerifyEmailAsync_ShouldThrowIfTokenExpired()
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
            _context.SaveChanges();

            Assert.ThrowsAsync<Exception>(async () => await _authService.VerifyEmailAsync(token));
        }

        [Test]
        public async Task ForgotPasswordAsync_ShouldGenerateResetTokenAndSendEmail()
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

            var request = new ForgotPasswordRequestDto
            {
                Email = "test@example.com"
            };

            await _authService.ForgotPasswordAsync(request);

            var updatedUser = await _userRepository.GetByEmailAsync("test@example.com");
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.ResetPasswordToken, Is.Not.Null);
            Assert.That(updatedUser.ResetPasswordTokenExpiry, Is.Not.Null);

            _mockEmailService.Verify(
                e => e.SendEmailAsync(
                    "test@example.com",
                    "Reset your password",
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }

        [Test]
        public async Task ForgotPasswordAsync_ShouldNotThrowIfEmailDoesNotExist()
        {
            var request = new ForgotPasswordRequestDto
            {
                Email = "nonexistent@example.com"
            };

            Assert.DoesNotThrowAsync(async () => await _authService.ForgotPasswordAsync(request));
            
            _mockEmailService.Verify(
                e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Test]
        public async Task ResetPasswordAsync_ShouldUpdatePasswordAndClearToken()
        {
            var token = Guid.NewGuid().ToString();
            var user = new User
            {
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "oldHash",
                ResetPasswordToken = token,
                ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(30),
                IsEmailVerified = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new ResetPasswordRequestDto
            {
                Token = token,
                NewPassword = "NewSecurePassword123"
            };

            await _authService.ResetPasswordAsync(request);

            var updatedUser = await _userRepository.GetByEmailAsync("test@example.com");
            Assert.That(updatedUser, Is.Not.Null);
            Assert.That(updatedUser.PasswordHash, Is.Not.EqualTo("oldHash"));
            Assert.That(updatedUser.ResetPasswordToken, Is.Null);
            Assert.That(updatedUser.ResetPasswordTokenExpiry, Is.Null);
        }

        [Test]
        public void ResetPasswordAsync_ShouldThrowIfTokenInvalid()
        {
            var request = new ResetPasswordRequestDto
            {
                Token = Guid.NewGuid().ToString(),
                NewPassword = "NewPassword123"
            };

            Assert.ThrowsAsync<Exception>(async () => await _authService.ResetPasswordAsync(request));
        }

        [Test]
        public async Task LoginAsync_ShouldReturnTokenForValidCredentials()
        {
            var passwordHasher = new ModelLayer.Helpers.PasswordHasher();
            var user = new User
            {
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = passwordHasher.HashPassword("Password123"),
                IsEmailVerified = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            var response = await _authService.LoginAsync(request);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Email, Is.EqualTo("test@example.com"));
            Assert.That(response.Token, Is.Not.Null);
            Assert.That(response.Token, Is.Not.Empty);
        }

        [Test]
        public void LoginAsync_ShouldThrowIfEmailNotVerified()
        {
            var passwordHasher = new ModelLayer.Helpers.PasswordHasher();
            var user = new User
            {
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = passwordHasher.HashPassword("Password123"),
                IsEmailVerified = false
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "Password123"
            };

            Assert.ThrowsAsync<Exception>(async () => await _authService.LoginAsync(request));
        }

        [Test]
        public void LoginAsync_ShouldThrowIfPasswordIncorrect()
        {
            var passwordHasher = new ModelLayer.Helpers.PasswordHasher();
            var user = new User
            {
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = passwordHasher.HashPassword("CorrectPassword"),
                IsEmailVerified = true
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            var request = new LoginRequestDto
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            Assert.ThrowsAsync<Exception>(async () => await _authService.LoginAsync(request));
        }

        [Test]
        public void LoginAsync_ShouldThrowIfUserDoesNotExist()
        {
            var request = new LoginRequestDto
            {
                Email = "nonexistent@example.com",
                Password = "Password123"
            };

            Assert.ThrowsAsync<Exception>(async () => await _authService.LoginAsync(request));
        }
    }
}
