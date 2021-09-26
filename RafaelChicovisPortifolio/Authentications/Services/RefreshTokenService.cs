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

namespace RafaelChicovisPortifolio.Authentications.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;
        private readonly PortifolioContext _context;
        private readonly IDeactiveTokenService _deactiveTokenService;
        private readonly IAuthenticationTokenService _authenticationService;
        
        public RefreshTokenService(
            IConfiguration configuration,
            IDistributedCache distributedCache,
            PortifolioContext context,
            IDeactiveTokenService deactiveTokenService, IAuthenticationTokenService authenticationService)
        {
            _configuration = configuration;
            _distributedCache = distributedCache;
            _context = context;
            _deactiveTokenService = deactiveTokenService;
            _authenticationService = authenticationService;
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

            var tokenActivated = await _deactiveTokenService.IsActiveAsync(token);
            if (!tokenActivated)
            {
                return null;
            }

            Guid userId;
            try
            {
                userId = Guid.Parse(validatedToken.Claims.Single(x => x.Type == "Id").Value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            
            var user = await _context.Users.FirstOrDefaultAsync(e =>
                e.Id == userId && e.IsDeleted == false);

            if (user == null)
            {
                return null;
            }
            
            var newToken = _authenticationService.GenerateTokenAsync(user);
            await DeactiveAsync(refreshToken);
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

        public async Task DeactiveAsync(string refreshToken)
        {
            await _distributedCache.RemoveAsync(GetKey(refreshToken));
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
                return principal;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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