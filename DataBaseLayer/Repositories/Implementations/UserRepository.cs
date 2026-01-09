using DataBaseLayer.DbContexts;
using DataBaseLayer.Entities;
using DataBaseLayer.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataBaseLayer.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly FundooNotesDbContext _context;

        public UserRepository(FundooNotesDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByEmailVerificationTokenAsync(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(u =>
                u.EmailVerificationToken == token &&
                u.EmailVerificationTokenExpiry > DateTime.UtcNow);
        }

        public async Task<User?> GetByResetPasswordTokenAsync(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(u =>
                u.ResetPasswordToken == token &&
                u.ResetPasswordTokenExpiry > DateTime.UtcNow);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}

