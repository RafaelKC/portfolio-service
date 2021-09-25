using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RafaelChicovisPortifolio.Authentications.Services;

namespace RafaelChicovisPortifolio.Authentications.Middlewares
{
    public class TokenManagerMiddleware : IMiddleware
    {
        private IDeactiveTokenService _deactiveTokenService;

        public TokenManagerMiddleware(IDeactiveTokenService deactiveTokenService)
        {
            _deactiveTokenService = deactiveTokenService;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (await _deactiveTokenService.IsCurrentActiveAsync())
            {
                await next(context);
                return;
            }
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}