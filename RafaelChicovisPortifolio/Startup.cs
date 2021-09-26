using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using RafaelChicovisPortifolio.Authentications.Middlewares;
using RafaelChicovisPortifolio.Authentications.Services;
using RafaelChicovisPortifolio.Contexts;

namespace RafaelChicovisPortifolio
{
    public class Startup
    {
        public IConfiguration Configuration { get; }   
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            var key = Encoding.ASCII.GetBytes(Configuration.GetSection("PotifolioSettings").GetSection("Token_key").Value);
            var frontendUrl = Configuration.GetSection("PotifolioSettings").GetSection("FrontendUrl").Value;
            
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration =
                    Configuration.GetSection("PotifolioSettings").GetSection("redisConnection").Value;
            });
            services.AddDbContext<PortifolioContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("NpgsqlConnection"));
            });
            services.AddCors();
            services.AddControllers();
            services.AddTransient<TokenManagerMiddleware>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IRefreshTokenService, RefreshTokenService>();
            services.AddTransient<IDeactiveTokenService, DeactiveTokenService>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IAuthenticationTokenService, AuthenticationTokenService>();
            services.AddAuthentication(e =>
                {
                    e.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    e.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(e =>
                {
                    e.RequireHttpsMetadata = false;
                    e.SaveToken = true;
                    e.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = "RafaelChicovisPortifolioService",
                        ValidateAudience = true,
                        ValidAudience = frontendUrl,
                        ValidateLifetime = true
                    };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors(e => e
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();
            // app.UseMiddleware<TokenManagerMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}