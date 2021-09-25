using System;

namespace RafaelChicovisPortifolio.Models.Administrations.Dtos
{
    public class AuthenticationResponseDto
    {
        public string UserName { get; set;}
        public string Token { get; set;}
        public string RefreshToken { get; set;}
        public bool Success { get; set;}
        public Guid UserId { get; set;}
    }
}