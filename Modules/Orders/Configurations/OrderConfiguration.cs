using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EShopMVC.Modules.Orders.Domain;
using EShopMVC.Modules.Orders.Domain.Entities;

namespace EShopMVC.Modules.Orders.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.TotalPrice)
                   .HasColumnType("decimal(18,2)");

            builder.Property(o => o.OrderDate)
                   .IsRequired();

            builder.Property(o => o.UserId)
                   .IsRequired();

            builder.HasIndex(x => x.OrderDate);
        }
    }
}