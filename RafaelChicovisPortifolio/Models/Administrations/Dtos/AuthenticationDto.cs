#nullable enable
using System;
using RafaelChicovisPortifolio.Models.Administrations.Enums;

namespace RafaelChicovisPortifolio.Models.Administrations.Dtos
{
    public class AuthenticationDto
    {
        public AuthenticationType Type { get; set; }
        public string Key { get; set; }
        public string? Password { get; set; }
        public string? RefreshToken { get; set; }
        public string? Token { get; set; }
    }
}