using ModelLayer.DTOs.Auth;

namespace BusinessLayer.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequestDto request);
    }
}
