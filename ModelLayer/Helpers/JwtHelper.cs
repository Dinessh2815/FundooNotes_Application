using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace ModelLayer.Helpers
{
    public class JwtHelper
    {
        private readonly string _secret;

        public JwtHelper(string secret)
        {
            _secret = secret; 
        }

        public string GenerateToken(int userId, string email)
        {

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(2),
                    signingCredentials : creds);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

    }
}
