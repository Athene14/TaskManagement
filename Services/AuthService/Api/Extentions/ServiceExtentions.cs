using AuthService.Domain.Abstractions;
using AuthService.Infra.Data.Repositories;
using AuthService.Infra.Security;
using TaskManagementServices.Shared.AuthService;
using Database.Dapper;

namespace AuthService.Api.Extentions
{
    internal static class ServiceExtensions
    {
        public static void ConfigureDatabase(this IServiceCollection services, IConfiguration config)
        {
            var serviceConfiguration = new AuthServiceConfiguration(config);
            services.AddPgSqlDatabase(serviceConfiguration.GetServiceSectionName());
            services.AddScoped<IUserRepository, UserRepository>();
        }

        public static void ConfigureAuthServices(this IServiceCollection services)
        {
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IAuthService, AuthService.App.Services.AuthService>();
        }
    }
}
