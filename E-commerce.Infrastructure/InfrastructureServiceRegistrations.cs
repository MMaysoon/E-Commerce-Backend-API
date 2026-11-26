using E_commerce.Application.Contracts;
using E_commerce.Application.settings;
using E_commerce.Domain.Contracts;
using E_commerce.Infrastructure.Data;
using E_commerce.Infrastructure.Identity.Data;
using E_commerce.Infrastructure.Identity.Entities;
using E_commerce.Infrastructure.Repositories;
using E_commerce.Infrastructure.Seeding;
using E_commerce.Infrastructure.Services;
using E_commerce.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure
{
    public static class InfrastructureServiceRegistrations
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // DBContext
           services.AddDbContext<StoreDBContext>(options =>
           {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("The 'DefaultConnection' connection string is not configured.");
                options.UseSqlServer(connectionString);
           });

            //DataSeeder
            services.AddKeyedScoped<IDataSeeder, CatalogDataSeeder>("Catalog");

            //Dependency Injection 
            services.AddScoped<IUnitOfWork, UnitOfWork>();


            #region Redis 
            services.AddSingleton<IConnectionMultiplexer>(config =>
            {
                return ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!);
            });

            services.AddScoped<IBasketRepository, BasketRepository>();  //Redis as storage -> apply in basket 
            services.AddSingleton<ICacheRepository, CacheRepository>();  //Redis as cache

            #endregion

            #region Identity
            //IDentityDbContext
            services.AddDbContext<StoreIdentityDbContext>(options =>
            {
                var connectionString =
                    configuration.GetConnectionString("IdentityConnection")
                    ?? throw new InvalidOperationException(
                        "IdentityConnection is not configured.");

                options.UseSqlServer(connectionString);
            });

            services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                })  // Registers Identity services for User and Role management
                .AddEntityFrameworkStores<StoreIdentityDbContext>()   // Configures EF Core as the storage provider using StoreIdentityDbContext
                .AddDefaultTokenProviders();   // Registers token providers for password reset, email confirmation, etc.

            //Seed Identity Roles
            services.AddKeyedScoped<IDataSeeder, IdentityDataSeeder>("Identity");


            //DI Token Service , Identiyt Service
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ITokenService, TokenService>();


            // JWT Configuration
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException("JwtSettings is not configured.");

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.SaveToken = true;
                opt.RequireHttpsMetadata = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                };
            });
            #endregion


            #region Email Service
            services.Configure<EmailSettings>( configuration.GetSection("EmailSettings"));


            services.Configure<EmailConfirmationSettings>(configuration.GetSection("EmailConfirmation"));

            services.AddScoped<IEmailService, EmailService>();
            #endregion


            #region Payment Gateway
            services.AddSingleton<IPaymentGateWay,StripePaymentGateway>();
            // Singleton because StripeConfiguration.ApiKey is a static set once at startup.
            #endregion
            return services;
        }
    }
}
