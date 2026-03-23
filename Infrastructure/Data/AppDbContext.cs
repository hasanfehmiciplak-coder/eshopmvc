using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Catalog.Models;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Logs;
using EShopMVC.Modules.Orders.Domain.Refunds;
using EShopMVC.Modules.Orders.Refund;
using EShopMVC.Modules.Payments.Models;
using EShopMVC.Shared.Domain;
using EShopMVC.Shared.EventBus;
using EShopMVC.Shared.Events;
using EShopMVC.Shared.Outbox;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection.Emit;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using OrderItem = EShopMVC.Modules.Orders.Domain.Entities.OrderItem;
using Refund = EShopMVC.Modules.Orders.Domain.Entities.Refund;

//using Refund = EShopMVC.Modules.Orders.Domain.Entities.Refund;

namespace EShopMVC.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Firm> Firms { get; set; }

        public DbSet<OrderTimeline> OrderTimelines { get; set; }

        public DbSet<StockReservation> StockReservations { get; set; }

        public DbSet<Inventory> Inventories { get; set; }

        public DbSet<FraudAlert> FraudAlerts { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        public DbSet<CustomerAddress> Addresses { get; set; }
        public DbSet<OrderLog> OrderLogs { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<PaymentLog> PaymentLogs { get; set; }

        public DbSet<Modules.Orders.Domain.Entities.Refund> Refunds { get; set; }

        public DbSet<RefundTimeline> RefundTimelines { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<BannedIp> BannedIps { get; set; }
        public DbSet<FraudFlag> FraudFlags { get; set; }

        public DbSet<FraudCase> FraudCases { get; set; }

        public DbSet<UserFraudScore> UserFraudScores { get; set; }

        public DbSet<FraudRule> FraudRules { get; set; }

        public DbSet<ProductVariant> ProductVariants { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public int UserId { get; private set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            builder.Ignore<DomainEvent>();

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.Property(m => m.LoginProvider).HasMaxLength(128);
                entity.Property(m => m.ProviderKey).HasMaxLength(128);
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.Property(m => m.LoginProvider).HasMaxLength(128);
                entity.Property(m => m.Name).HasMaxLength(128);
            });

            // Refund → Order
            builder.Entity<Refund>()
                .HasOne(r => r.Order)
                .WithMany(o => o.PartialRefunds)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Refund>()
                .HasOne(r => r.ParentRefund)
                .WithMany(r => r.ChildRefunds)
                .HasForeignKey(r => r.RefundId)
                .OnDelete(DeleteBehavior.Restrict);

            // Refund → OrderItem
            builder.Entity<Refund>()
                .HasOne(r => r.OrderItem)
                .WithMany()
                .HasForeignKey(r => r.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Refund → ParentRefund
            builder.Entity<Refund>()
                .HasOne(r => r.ParentRefund)
                .WithMany()
                .HasForeignKey(r => r.RefundId)
                .OnDelete(DeleteBehavior.Restrict);

            // PaymentLog → Order
            builder.Entity<PaymentLog>()
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // RefundLog → Order
            builder.Entity<RefundLog>()
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // OrderItem → Order
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)   // Order içindeki OrderItem listesi
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // FraudFlag → Order
            builder.Entity<FraudFlag>()
                .HasOne(f => f.Order)
                .WithMany()
                .HasForeignKey(f => f.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<RefundLog>()
                .HasOne(r => r.Refund)
                .WithMany()
                .HasForeignKey(r => r.RefundId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexler
            builder.Entity<Order>()
                .HasIndex(x => x.CreatedAt);

            builder.Entity<Refund>()
                .HasIndex(x => x.Status);

            builder.Entity<Refund>()
                .HasOne(r => r.Order)
                .WithMany(o => o.PartialRefunds)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Refund>()
                .HasOne(r => r.OrderItem)
                .WithMany()
                .HasForeignKey(r => r.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Refund>()
                .HasOne(r => r.ParentRefund)
                .WithMany()
                .HasForeignKey(r => r.RefundId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            // Domain event içeren entityleri bul
            var domainEntities = ChangeTracker
                .Entries<BaseEntity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
                .ToList();

            // Domain eventleri topla
            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            // Outbox mesajlarını oluştur
            foreach (var domainEvent in domainEvents)
            {
                var outboxMessage = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = domainEvent.GetType().Name,
                    Payload = JsonSerializer.Serialize(domainEvent),
                    OccurredOn = domainEvent.OccurredOn
                };

                OutboxMessages.Add(outboxMessage);
            }

            // Domain eventleri temizle
            foreach (var entity in domainEntities)
            {
                entity.Entity.ClearDomainEvents();
            }

            // Tek SaveChanges ile hepsini kaydet
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}