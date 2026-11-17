using E_commerce.Domain.Contracts;
using E_commerce.Infrastructure.Identity.Data;
using E_commerce.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Seeding
{
    internal class IdentityDataSeeder : IDataSeeder
    {
        private readonly StoreIdentityDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<IdentityDataSeeder> _logger;
        private readonly IConfiguration _configuration;

        public IdentityDataSeeder( StoreIdentityDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,ILogger<IdentityDataSeeder> logger, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SeedAsync( CancellationToken cancellationToken = default)
        {
            try
            {
                var pendingMigrations =  await _dbContext.Database.GetPendingMigrationsAsync( cancellationToken);

                if (pendingMigrations.Any())
                {
                    await _dbContext.Database.MigrateAsync(cancellationToken);
                }

                // Seed Roles
                string[] roles = { "Admin", "SuperAdmin" };

                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        var result = await _roleManager.CreateAsync(new IdentityRole(role));

                        if (!result.Succeeded)
                        {
                            _logger.LogWarning(
                                "Failed to create role {Role}: {Errors}",
                                role,
                                string.Join("; ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }

                // Seed Admin User
                var admin = await _userManager.FindByNameAsync("Maysoon");

                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        DisplayName = "Maysoon Ahmed",
                        Email = "maysoonahmedahmed@gmail.com",
                        UserName = "Maysoon",
                        PhoneNumber = "01551953448"
                    };

                    // if seed using user secerts
                    //var password = _configuration["SeedAdmin:Password"];

                    //if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException( "Seed admin password is not configured.");

                    //var result = await _userManager.CreateAsync(admin, password);

                    var result = await _userManager.CreateAsync(admin, "P@ssw0rd");

                    if (!result.Succeeded)
                    {
                        _logger.LogWarning(
                            "Could not create admin: {Errors}",
                            string.Join("; ", result.Errors.Select(e => e.Description)));

                        return;
                    }
                }

                // Assign Admin Role
                if (await _roleManager.RoleExistsAsync("Admin") &&
                    !await _userManager.IsInRoleAsync(admin, "Admin"))
                {
                    var result = await _userManager.AddToRoleAsync(admin, "Admin");

                    if (!result.Succeeded)
                    {
                        _logger.LogWarning(
                            "Could not assign Admin role: {Errors}",
                            string.Join("; ", result.Errors.Select(e => e.Description)));
                    }
                }

                _logger.LogInformation( "Identity data seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Identity data seeding failed.");
                throw;
            }
        }
    }
}
