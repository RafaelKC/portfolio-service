using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RafaelChicovisPortifolio.Authentications.Services;
using RafaelChicovisPortifolio.Models.Administrations.Dtos;
using RafaelChicovisPortifolio.Models.Administrations.Entities;

namespace RafaelChicovisPortifolio.Authentications.Controllers
{
    [Route("athentication")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthenticationResponseDto>> Login(
            [FromBody] AuthenticationDto authenticationInput)
        {
            if (authenticationInput == null)
            {
                return BadRequest();
            }
            var auth = await _authenticationService.Login(authenticationInput);
            if (auth == null)
            {
                return Unauthorized();
            }

            return auth;
        }

        [HttpPost("create")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthenticationResponseDto>> CreateUser([FromBody] User user)
        {
            var newUser = await _authenticationService.CreateUser(user);
            if (newUser == null)
            {
                return Problem();
            }

            return newUser; 
        }

        [HttpPost("logout")]
        public async Task<ActionResult> Logout(
            [FromBody] AuthenticationDto authenticationInput)
        {
            await _authenticationService.Logout(authenticationInput);
            return Ok();
        }
    }
}