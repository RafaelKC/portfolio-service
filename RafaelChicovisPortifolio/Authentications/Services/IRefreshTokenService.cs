using System;
using System.Threading.Tasks;
using RafaelChicovisPortifolio.Models.Administrations.Dtos;

namespace RafaelChicovisPortifolio.Authentications.Services
{
    public interface IRefreshTokenService
    {
        public Task<string> AuthByRefreshToken(string refreshToken, string token);
        public Task<string> GenerateRefreshToken(Guid userId, string token);
    }
}