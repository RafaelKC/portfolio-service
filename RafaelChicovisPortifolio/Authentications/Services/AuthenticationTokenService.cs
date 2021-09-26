using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RafaelChicovisPortifolio.Models.Administrations.Entities;

namespace RafaelChicovisPortifolio.Authentications.Services
{
    public interface IAuthenticationTokenService
    {
        public string GenerateTokenAsync(User user);
    }
    
    public class AuthenticationTokenService : IAuthenticationTokenService
    {
        private readonly IConfiguration _configuration;

        public AuthenticationTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateTokenAsync(User user)
        {
            var key = _configuration.GetSection("PotifolioSettings").GetSection("Token_key").Value;
            var keyByte = Encoding.ASCII.GetBytes(_configuration.GetSection("PotifolioSettings").GetSection("Token_key").Value);
            var expirationTimeInMinutes = _configuration.GetSection("PotifolioSettings").GetSection("authenticationTimeoutInMinutes").Value;
            var frontendUrl = _configuration.GetSection("PotifolioSettings").GetSection("FrontendUrl").Value;
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescripotr = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new(ClaimTypes.Name, user.UserName),
                    new("Id", user.Id.ToString()),
                    new(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Int32.Parse(expirationTimeInMinutes)),
                SigningCredentials = new SigningCredentials( new SymmetricSecurityKey(keyByte), SecurityAlgorithms.HmacSha256Signature),
                Issuer = "RafaelChicovisPortifolioService",
                Audience = frontendUrl
            };
            
            var token = tokenHandler.CreateToken(tokenDescripotr);
            return tokenHandler.WriteToken(token);
        }
        
    }
}