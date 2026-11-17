using E_commerce.Domain.Contracts;
using E_commerce.Infrastructure.Data;
using E_commerce.Infrastructure.Identity.Data;
using E_commerce.Infrastructure.Identity.Entities;
using E_commerce.Infrastructure.Repositories;
using E_commerce.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            //1.IDentityDbContext
            services.AddDbContext<StoreIdentityDbContext>(options =>
            {
                var connectionString =
                    configuration.GetConnectionString("IdentityConnection")
                    ?? throw new InvalidOperationException(
                        "IdentityConnection is not configured.");

                options.UseSqlServer(connectionString);
            });

            services
                .AddIdentity<ApplicationUser, IdentityRole>()   // Registers Identity services for User and Role management
                .AddEntityFrameworkStores<StoreIdentityDbContext>()   // Configures EF Core as the storage provider using StoreIdentityDbContext
                .AddDefaultTokenProviders();   // Registers token providers for password reset, email confirmation, etc.

            //2.Seed Identity Roles
            services.AddKeyedScoped<IDataSeeder, IdentityDataSeeder>("Identity");

            
            #endregion

            return services;
        }
    }
}
