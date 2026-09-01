using E_commerce.Domain.Contracts;

namespace E_commerce.API.Extensions
{
    public static class WebApplicationExtension
    {
        public static async Task<WebApplication> SeedDataBaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredKeyedService<IDataSeeder>("Catalog");
            await seeder.SeedAsync();
            return app;
        }
    }
}

