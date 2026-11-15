using E_commerce.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Data
{
    // primary ctor
    public class StoreDBContext(DbContextOptions<StoreDBContext>options) :DbContext(options)
    {
        #region DbSets
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductType> ProductTypes => Set<ProductType>();
        public DbSet<ProductBrand> ProductBrands => Set<ProductBrand>();
        #endregion


        #region Configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreDBContext).Assembly);
        }
        #endregion
    }
}
