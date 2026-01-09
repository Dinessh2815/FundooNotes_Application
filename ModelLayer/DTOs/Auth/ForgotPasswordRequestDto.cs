using System.ComponentModel.DataAnnotations;

namespace ModelLayer.DTOs.Auth
{
    public class ForgotPasswordRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
