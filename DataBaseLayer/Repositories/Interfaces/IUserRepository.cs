using DataBaseLayer.Entities;

namespace DataBaseLayer.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task AddAsync(User user);
        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByEmailVerificationTokenAsync(string token);

        Task<User?> GetByResetPasswordTokenAsync(string token);

        Task UpdateAsync(User user);
    }
}
