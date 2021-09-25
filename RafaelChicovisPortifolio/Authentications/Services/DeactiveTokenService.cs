using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace RafaelChicovisPortifolio.Authentications.Services
{
    public class DeactiveTokenService : IDeactiveTokenService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public DeactiveTokenService(
            IDistributedCache distributedCache,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _distributedCache = distributedCache;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }
        
        public async Task<bool> IsActiveAsync(string token)
        {
            return await _distributedCache.GetStringAsync(GetKey(token)) == null;
        }

        public Task<bool> IsCurrentActiveAsync()
        {
            return IsActiveAsync(GetCurrent());
        }

        public async Task DeactivateAsync(string token)
        {
            var expirationTimeInMinutes = _configuration.GetSection("PotifolioSettings").GetSection("authenticationTimeoutInMinutes").Value;
            await _distributedCache.SetStringAsync(GetKey(token), " ", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Int32.Parse(expirationTimeInMinutes))
            });
        }

        public async Task DeactivateCurrentAsync(string token)
        {
            await DeactivateAsync(GetCurrent());
        }

        private string GetCurrent()
        {
            var autorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["authorization"];
            return autorizationHeader ==
                StringValues.Empty ? string.Empty : autorizationHeader.Single().Split(" ").Last();
        }
        
        private static string GetKey(string token)
            => $"tokens:{token}:deactivated";
    }
}