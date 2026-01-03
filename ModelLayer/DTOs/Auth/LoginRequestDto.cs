using System.ComponentModel.DataAnnotations;

namespace ModelLayer.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
