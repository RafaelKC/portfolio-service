using System.Threading.Tasks;

namespace RafaelChicovisPortifolio.Authentications.Services
{
    public interface IDeactiveTokenService
    {
        public Task<bool> IsActiveAsync(string token);
        public Task<bool> IsCurrentActiveAsync();
        public Task DeactivateAsync(string token);
        public Task DeactivateCurrentAsync(string token);
    }
}