using BusinessLayer.Interfaces;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using ModelLayer.DTOs.Auth;
using ModelLayer.Helpers;

namespace BusinessLayer.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher _passwordHasher;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher();
        }

        public async Task RegisterAsync(RegisterRequestDto request)
        {
            var existingUser =
                await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser != null)
                throw new Exception("Email already registered");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash =
                    _passwordHasher.HashPassword(request.Password)
            };

            await _userRepository.AddAsync(user);
        }
    }
}
