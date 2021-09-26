using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RafaelChicovisPortifolio.Contexts;
using RafaelChicovisPortifolio.Models.Administrations.Dtos;
using RafaelChicovisPortifolio.Models.Administrations.Entities;
using RafaelChicovisPortifolio.Models.Administrations.Enums;

namespace RafaelChicovisPortifolio.Authentications.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly PortifolioContext _context;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IDeactiveTokenService _deactiveTokenService;
        private readonly IAuthenticationTokenService _authenticationTokenService;

        public AuthenticationService(PortifolioContext context, IRefreshTokenService refreshTokenService, IDeactiveTokenService deactiveTokenService, IAuthenticationTokenService authenticationTokenService)
        {
            _context = context;
            _refreshTokenService = refreshTokenService;
            _deactiveTokenService = deactiveTokenService;
            _authenticationTokenService = authenticationTokenService;
        }

        public async Task<AuthenticationResponseDto> Login(AuthenticationDto authenticationInput)
        {
            if (authenticationInput.Type == AuthenticationType.Password)
            {
                if (string.IsNullOrEmpty(authenticationInput.Password) || string.IsNullOrEmpty(authenticationInput.Key))
                {
                    return null;
                }

                authenticationInput.Password = EncryptPassword(authenticationInput.Password);

                var user = await _context.Users.FirstOrDefaultAsync(e =>
                    e.Password == authenticationInput.Password && e.Key == authenticationInput.Key &&
                    e.IsDeleted == false);

                if (user == null)
                {
                    return null;
                }

                var token = _authenticationTokenService.GenerateTokenAsync(user);
                var refreshToken = await _refreshTokenService.GenerateRefreshToken(user.Id, token);

                return new AuthenticationResponseDto
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                    UserName = user.UserName
                };

            }

            if (authenticationInput.Type == AuthenticationType.RefreshToken)
            {
                if (string.IsNullOrEmpty(authenticationInput.Key) || string.IsNullOrEmpty(authenticationInput.Token) ||
                    string.IsNullOrEmpty(authenticationInput.RefreshToken))
                {
                    return null;
                }
                
                var user = await _context.Users.FirstOrDefaultAsync(e => e.Key == authenticationInput.Key && e.IsDeleted == false);
                if (user == null)
                {
                    return null;
                }
                
                var token = await _refreshTokenService.AuthByRefreshToken(authenticationInput.RefreshToken, authenticationInput.Token);
                if (token == null)
                {
                    return null;
                }
                var refreshToken = await _refreshTokenService.GenerateRefreshToken(user.Id, token);
                if (refreshToken == null)
                {
                    return null;
                }
                
                return new AuthenticationResponseDto
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                    UserName = user.UserName
                };
            }
            
            return null;
        }

        public async Task<AuthenticationResponseDto> CreateUser(User newUser)
        {
            if (string.IsNullOrEmpty(newUser.Key) || string.IsNullOrEmpty(newUser.Password) ||
                string.IsNullOrEmpty(newUser.UserName))
            {
                return null;
            }
            
            newUser.Password = EncryptPassword(newUser.Password);
            newUser.CreationTime = DateTime.UtcNow;
            newUser.CreatorId = newUser.Id;
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
            
            var token = _authenticationTokenService.GenerateTokenAsync(newUser);
            var refreshToken = await _refreshTokenService.GenerateRefreshToken(newUser.Id, token);

            return new AuthenticationResponseDto
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                UserId = newUser.Id,
                UserName = newUser.UserName
            };
        }

        public async Task Logout(AuthenticationDto authenticationInput)
        {
            if (!string.IsNullOrEmpty(authenticationInput.Token))
            {
                await _deactiveTokenService.DeactivateAsync(authenticationInput.Token);
                if (!string.IsNullOrEmpty(authenticationInput.RefreshToken))
                {
                    await _refreshTokenService.DeactiveAsync(authenticationInput.RefreshToken);
                }
            }
        }

        private string EncryptPassword(string password)
        {
            var sha = new SHA512CryptoServiceProvider();
            var passwordBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            
            var stringBuilder = new StringBuilder();
            foreach (var bte in passwordBytes)
            {
                stringBuilder.Append(bte.ToString("X2"));
            }
            
            return stringBuilder.ToString();
        }
    }
}