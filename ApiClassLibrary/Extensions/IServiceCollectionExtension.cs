using ApiClassLibrary.DataContext;
using ApiClassLibrary.Interfaces;
using ApiClassLibrary.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiClassLibrary.Extensions
{
    /// <summary>
    /// Extension methods for configuring API-related services into the application's
    /// dependency injection container.
    /// </summary>
    public static class IServiceCollectionExtension
    {
        /// <summary>
        /// Registers database contexts, Identity, and application services used by the API.
        /// Call this from <see cref="Program"/> or the startup composition root to centralize
        /// service registrations.
        /// </summary>
        /// <param name="service">The service collection to add registrations to.</param>
        /// <param name="configuration">Application configuration used to read connection strings and other settings.</param>
        public static void AddApiServices(this IServiceCollection service, IConfiguration configuration)
        {
            // Register the identity database context that stores users, roles and Identity tables.
            service.AddDbContext<ApiIdentityDbContext>(options =>
            {
                // Use MySQL connection string from configuration
                options.UseMySQL(configuration.GetConnectionString("MySql"));

                // Enable sensitive data logging for easier debugging (remove or disable in production).
                options.EnableSensitiveDataLogging();

                // If needed, enable detailed errors to get more EF Core error detail.
                // options.EnableDetailedErrors();
            });

            // Register an application-specific report context (separate DbContext for reports/data).
            service.AddDbContext<ReportDbContext>(options =>
            {
                options.UseMySQL(configuration.GetConnectionString("MySql"));
                options.EnableSensitiveDataLogging();
                // options.EnableDetailedErrors();
            });

            // Configure ASP.NET Core Identity with options tailored to the application requirements.
            service.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // Disable lockout for new users by default.
                options.Lockout.AllowedForNewUsers = false;

                // Require confirmed email for sign-in to improve security.
                options.SignIn.RequireConfirmedAccount = true;

                // Enforce unique emails per user.
                options.User.RequireUniqueEmail = true;

                // Password rules: require digit and minimum length.
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
            })
            // Persist Identity data into the ApiIdentityDbContext.
            .AddEntityFrameworkStores<ApiIdentityDbContext>()
            // Add default token providers for password reset, email confirmation, etc.
            .AddDefaultTokenProviders();

            // Register application services with appropriate lifetimes.
            // Scoped lifetime is suitable for services that use DbContext or per-request state.
            service.AddScoped<IIdentityService, IdentityService>();
            service.AddScoped<IEmailService, EmailService>();
            service.AddScoped<IHomeService, HomeService>();
            service.AddScoped<IAuthService, AuthService>();
        }
    }
}
