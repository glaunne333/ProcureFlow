using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureFlow.Application.Auth;
using ProcureFlow.Infrastructure.Persistence;
using ProcureFlow.Infrastructure.Security;

namespace ProcureFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(options =>
        {
            var section = configuration.GetSection(JwtOptions.SectionName);
            options.Key = section["Key"] ?? string.Empty;
            options.Issuer = section["Issuer"] ?? "portfolio-demo";
            options.Audience = section["Audience"] ?? "portfolio-demo";

            if (int.TryParse(section["ExpirationMinutes"], out var expirationMinutes))
            {
                options.ExpirationMinutes = expirationMinutes;
            }
        });
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<ProcureFlowDbContext>(options =>
                options.UseNpgsql(connectionString));
            services.AddScoped<DemoDataSeeder>();
        }

        return services;
    }
}
