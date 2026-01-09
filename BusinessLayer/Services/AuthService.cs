using BusinessLayer.Interfaces;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using ModelLayer.DTOs.Auth;
using ModelLayer.Helpers;
using Microsoft.Extensions.Configuration;


namespace BusinessLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;



        public AuthService(IUserRepository userRepository, IConfiguration configuration, IEmailService emailService)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher();
            _configuration = configuration;
            _emailService = emailService;
        }


        public async Task RegisterAsync(RegisterRequestDto request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new Exception("Email already registered");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                IsEmailVerified = false,
                EmailVerificationToken = Guid.NewGuid().ToString(),
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            await _userRepository.AddAsync(user);

            var link = $"https://localhost:7278/api/auth/verify-email?token={user.EmailVerificationToken}";

            await _emailService.SendEmailAsync(
                user.Email,
                "Verify your email",
                $"Click here to verify your email: <a href='{link}'>Verify</a>"
            );
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            // Security best practice: do NOT reveal whether email exists
            if (user == null)
                return;

            user.ResetPasswordToken = Guid.NewGuid().ToString();
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddMinutes(30);

            await _userRepository.UpdateAsync(user);

            var resetLink =
                $"https://localhost:7278/api/auth/reset-password?token={user.ResetPasswordToken}";

            await _emailService.SendEmailAsync(
                user.Email,
                "Reset your password",
                $@"
            <p>You requested a password reset.</p>
            <p>
                <a href='{resetLink}'>Click here to reset your password</a>
            </p>
            <p>This link will expire in 30 minutes.</p>
        "
            );
        }


        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var user = await _userRepository.GetByResetPasswordTokenAsync(request.Token);

            if (user == null)
                throw new Exception("Invalid or expired reset token");

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;

            await _userRepository.UpdateAsync(user);
        }




        public async Task VerifyEmailAsync(string token)
        {
            var user = await _userRepository.GetByEmailVerificationTokenAsync(token);

            if (user == null)
                throw new Exception("Invalid or expired verification token");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;

            await _userRepository.UpdateAsync(user);
        }



        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (!user.IsEmailVerified)
                throw new Exception("Please verify your email before login");


            if (user == null)
                throw new Exception("Invalid credentials");

            var isValid = _passwordHasher
                .VerifyPassword(user.PasswordHash, request.Password);

            if (!isValid)
                throw new Exception("Invalid credentials");

                var secret = _configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(secret))
                throw new InvalidOperationException("JWT secret is not configured.");

            var jwt = new JwtHelper(secret: secret);

            var token = jwt.GenerateToken(user.UserId, user.Email);



            return new LoginResponseDto
            {
                Email = user.Email,
                Token = token
            };
        }
    }
}
