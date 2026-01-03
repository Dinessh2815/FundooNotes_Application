using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTOs.Auth;

namespace FundooNotes.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;

        public AuthController (IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            await _authService.RegisterAsync(request);
            return Ok(new { message = "User Registered Sucessfully" });
        }
    }
}
