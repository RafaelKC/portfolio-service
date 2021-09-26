using System.Threading.Tasks;
using RafaelChicovisPortifolio.Models.Administrations.Dtos;
using RafaelChicovisPortifolio.Models.Administrations.Entities;

namespace RafaelChicovisPortifolio.Authentications.Services
{
    public interface IAuthenticationService
    {
        public Task<AuthenticationResponseDto> Login(AuthenticationDto authenticationInput);
        public Task<AuthenticationResponseDto> CreateUser(User authenticationInput);
        public Task Logout(AuthenticationDto authenticationInput);
    }
}