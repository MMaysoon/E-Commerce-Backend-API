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
    public class OrderItemConfigurations :IEntityTypeConfiguration<OrderItem>
    {
        public void Configure (EntityTypeBuilder<OrderItem>builder)
        {
            builder.ToTable("OrderItem");
            builder.Property(o => o.Price).HasColumnType("decimal(10,2)");

            // ProductItemOrdered Product
            builder.OwnsOne(o => o.Product);
        }
    }
}
