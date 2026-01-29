using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MonarchLearn.Application.Interfaces.Services;
using MonarchLearn.Application.Interfaces.UnitOfWork;
using MonarchLearn.Application.Services;
using MonarchLearn.Domain.Entities.Users;
using MonarchLearn.Infrastructure.FileStorage;
using MonarchLearn.Infrastructure.Persistence.Context;
using MonarchLearn.Infrastructure.Persistence.Interceptors;
using MonarchLearn.Infrastructure.Persistence.Repositories;
using MonarchLearn.Infrastructure.BackgroundJobs;
using System.Text;

namespace MonarchLearn.Infrastructure
{
    public static class DependencyInjection
    {
        public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<EntitySaveChangesInterceptor>();

            services.AddDbContext<MonarchLearnDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<EntitySaveChangesInterceptor>();

                options.UseSqlServer(configuration.GetConnectionString("Default"))
                       .AddInterceptors(interceptor);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IScheduledTaskService, ScheduledTaskService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<INotificationService, NotificationService>();

            // --- IDENTITY & OTP CONFIGURATION ---
            services.AddIdentity<AppUser, AppRole>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.User.RequireUniqueEmail = true;

                // OTP (6 rəqəmli kod) üçün provayderlər
                opt.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                opt.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
            })
            .AddEntityFrameworkStores<MonarchLearnDbContext>()
            .AddDefaultTokenProviders();

            services.AddHangfire(config => config
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("Default")));

            services.AddHangfireServer();

            var jwtSettings = configuration.GetSection("JwtSettings");
            services.AddAuthentication(opt => {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),
                    ClockSkew = TimeSpan.Zero
                };
            });
        }
    }
}