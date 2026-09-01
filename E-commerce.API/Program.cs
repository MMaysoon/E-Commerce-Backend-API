using E_commerce.API.Extensions;
using E_commerce.Infrastructure;

namespace E_commerce.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            #region Apply Services 
           builder.Services.AddInfrastructureServices(builder.Configuration);
            #endregion
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            //builder.Services.AddOpenApi();

           

            var app = builder.Build();

            #region Data Seeding
            await app.SeedDataBaseAsync();
            #endregion

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
               
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
