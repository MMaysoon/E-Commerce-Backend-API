using E_commerce.Application.IServices;
using E_commerce.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            //Automapper
            services.AddAutoMapper(cfg=> { },typeof(ApplicationServiceRegistration).Assembly);

            //Dependency Injection
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IBasketService, BasketService>();

            return services;
        }
    }
}
