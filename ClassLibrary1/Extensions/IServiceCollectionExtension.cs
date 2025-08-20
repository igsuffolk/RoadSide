using ClassLibrary1.DataContext;
using ClassLibrary1.Interfaces;
using ClassLibrary1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClassLibrary1.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void AddApiServices(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddDbContext<MyIdentityDbContext>(options =>
            {
                options.UseMySQL(configuration.GetConnectionString("MySql"));
                options.EnableSensitiveDataLogging();
                //options.EnableDetailedErrors();
            });

            service.AddDbContext<ReportDbContext>(options =>
            {
                options.UseMySQL(configuration.GetConnectionString("MySql"));
                options.EnableSensitiveDataLogging();
                //options.EnableDetailedErrors();
            });

            service.AddIdentity<IdentityUser,IdentityRole>(options =>
            {
                options.Lockout.AllowedForNewUsers = false;
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
            }
            )
             .AddEntityFrameworkStores<MyIdentityDbContext>()
             .AddDefaultTokenProviders();

            service.AddScoped<IIdentityService, IdentityService>();
            service.AddScoped<IEmailService, EmailService>();
            service.AddScoped<IHomeService, HomeService>();
            service.AddScoped<IAuthService, AuthService>();
        }

    }
    }
