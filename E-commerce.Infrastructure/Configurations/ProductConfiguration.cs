using E_commerce.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasOne(x => x.ProductBrand)
                 .WithMany()
                 .HasForeignKey(x => x.ProductBrandId);

            builder.HasOne(x => x.ProductType)
                    .WithMany()
                    .HasForeignKey(x => x.ProductTypeId);

            builder.Property(x=>x.Price)
                .HasColumnType("decimal(18,2)");

            builder.Property(x=>x.Name)
                   .IsRequired()
                   .HasMaxLength(100);


            builder.Property(x=>x.Description)
                   .HasMaxLength(100);

            builder.Property(x=>x.PictureUrl)
                   .HasMaxLength(200);

        }
    }
}
