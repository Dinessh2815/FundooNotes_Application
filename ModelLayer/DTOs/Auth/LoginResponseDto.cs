using System;
using System.Collections.Generic;
using System.Text;

namespace ModelLayer.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }

}
