using E_commerce.Domain.Contracts;
using E_commerce.Infrastructure.Data;
using E_commerce.Infrastructure.Repositories;
using E_commerce.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
           });

            //DataSeeder
            services.AddKeyedScoped<IDataSeeder, CatalogDataSeeder>("Catalog");

            //Dependency Injection 
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
