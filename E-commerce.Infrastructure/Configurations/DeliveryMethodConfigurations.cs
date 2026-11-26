
using E_commerce.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.Infrastructure.Configurations
{
    public class DeliveryMethodConfigurations : IEntityTypeConfiguration<DeliveryMethod>
    {
        public void Configure(EntityTypeBuilder<DeliveryMethod> builder)
        {
            builder.ToTable("DeliveryMethod");

            builder.Property(d => d.Cost)
                .HasColumnType("decimal(10,2)");

            builder.Property(d => d.ShortName)
                .HasColumnType("varchar")
                .HasMaxLength(50);

            builder.Property(d => d.Description)
                .HasColumnType("varchar")
                .HasMaxLength(200);

            builder.Property(d => d.DeliveryTime)
                .HasColumnType("varchar")
                .HasMaxLength(50);

            builder.HasData(
                new DeliveryMethod
                {
                    Id = 1,
                    ShortName = "Standard",
                    Description = "Standard home delivery",
                    DeliveryTime = "3-5 Business Days",
                    Cost = 30m
                },
                new DeliveryMethod
                {
                    Id = 2,
                    ShortName = "Express",
                    Description = "Express home delivery",
                    DeliveryTime = "1-2 Business Days",
                    Cost = 50m
                },
                new DeliveryMethod
                {
                    Id = 3,
                    ShortName = "Next Day",
                    Description = "Next day home delivery",
                    DeliveryTime = "Next Business Day",
                    Cost = 80m
                }
            );
        }
    }
}
