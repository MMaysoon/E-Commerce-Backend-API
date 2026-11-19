using E_commerce.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_commerce.Infrastructure.Configurations
{
    public class DeliveryMethodConfigurations:IEntityTypeConfiguration<DeliveryMethod> 
    {
        public void Configure (EntityTypeBuilder<DeliveryMethod> builder)
        {
            builder.ToTable("DeliveryMethod");

            builder.Property(d => d.Cost).HasColumnType("decimal(10,2)");
            builder.Property(d => d.ShortName).HasColumnType("VarChar").HasMaxLength(50);
            builder.Property(d => d.Description).HasColumnType("varchar").HasMaxLength(10);
            builder.Property(d => d.DeliveryTime).HasColumnType("Varchar").HasMaxLength(50);
        }
    }
}
