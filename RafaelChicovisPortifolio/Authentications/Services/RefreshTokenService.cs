using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RafaelChicovisPortifolio.Contexts;
using RafaelChicovisPortifolio.Models.Administrations.Entities;

namespace RafaelChicovisPortifolio.Authentications.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;
        private readonly PortifolioContext _context;
        
        public RefreshTokenService(
            IConfiguration configuration,
            IDistributedCache distributedCache,
            PortifolioContext context)
        {
            _configuration = configuration;
            _distributedCache = distributedCache;
            _context = context;
        }

        public async Task<string> AuthByRefreshToken(string refreshToken, string token)
        {
            var validatedToken = GetPrincipalFromToken(token);

            if (validatedToken == null)
            {
                return null;
            }

            var refreshTokenIsExpired = await _distributedCache.GetStringAsync(GetKey(refreshToken)) == null;

            if (refreshTokenIsExpired)
            {
                return null;
            }

            var user = await _context.Users.FirstOrDefaultAsync(e =>
                e.Id.ToString() == validatedToken.Claims.Single(x => x.Type == "Id").Value &&
                e.IsDeleted == false);

            if (user == null)
            {
                return null;
            }
            
            var newToken = AuthenticationTokenService.GenerateTokenAsync(user);
            return newToken;
        }

        public async Task<string> GenerateRefreshToken(Guid userId, string token)
        {
            var refreshToken = GenerateRefreshToken(token, userId);
            var refreshRedisKey = GetKey(refreshToken);
            var expirationTimeInMinutes = _configuration.GetSection("PotifolioSettings").GetSection("RefreshTokenExpirationInMinutes").Value;

            await _distributedCache.SetStringAsync(refreshRedisKey, " ", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Int32.Parse(expirationTimeInMinutes))
            });

            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("PotifolioSettings").GetSection("Token_key").Value);
            var frontendUrl = _configuration.GetSection("PotifolioSettings").GetSection("FrontendUrl").Value;
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "RafaelChicovisPortifolioService",
                ValidateAudience = true,
                ValidAudience = frontendUrl
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                {
                    return null;
                }

                return principal;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validateToken)
        {
            return (validateToken is JwtSecurityToken jwtSecurityToken) &&
                   jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase); 
        }
        
        private static string GetKey(string refreshToken)
            => $"refreshTokens:{refreshToken}:activated";
        
        private static string GenerateRefreshToken(string token, Guid userId)
            => $"(userId:{userId}:token:{token.ToString()}:refreshKey:{Guid.NewGuid()})";
            
    }
}