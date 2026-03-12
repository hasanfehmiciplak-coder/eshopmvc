using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Orders.Models;
using EShopMVC.Modules.Payments.Models;
using EShopMVC.Shared.Domain;
using EShopMVC.Shared.EventBus;
using EShopMVC.Shared.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection.Emit;
using EShopMVC.Shared.Outbox;
using System.Text.Json;
using EShopMVC.Modules.Fraud.Models;

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

        public DbSet<FraudAlert> FraudAlerts { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        public DbSet<CustomerAddress> Addresses { get; set; }
        public DbSet<OrderLog> OrderLogs { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<PaymentLog> PaymentLogs { get; set; }

        public DbSet<RefundLog> RefundLogs { get; set; }

        public DbSet<PartialRefund> PartialRefunds { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<BannedIp> BannedIps { get; set; }
        public DbSet<FraudFlag> FraudFlags { get; set; }

        public DbSet<Refund> Refunds { get; set; }

        public DbSet<FraudCase> FraudCases { get; set; }

        public DbSet<OrderTimeline> OrderTimelines { get; set; }

        public DbSet<UserFraudScore> UserFraudScores { get; set; }

        public DbSet<FraudRule> FraudRules { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Ignore<DomainEvent>();

            // Identity tablolarındaki kolon uzunlukları
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

            // Order → User (cascade KAPALI)
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔥 PartialRefund → Order (cascade KAPALI)
            builder.Entity<PartialRefund>()
                .HasOne(pr => pr.Order)
                .WithMany(o => o.PartialRefunds)
                .HasForeignKey(pr => pr.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // PartialRefund → OrderItem (cascade AÇIK)
            builder.Entity<PartialRefund>()
                .HasOne(pr => pr.OrderItem)
                .WithMany()
                .HasForeignKey(pr => pr.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefundLog>()
                .HasOne(r => r.PartialRefund)
                .WithMany(p => p.RefundLogs)
                .HasForeignKey(r => r.PartialRefundId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<FraudFlag>()
                .HasOne(f => f.Order)
                .WithMany(o => o.FraudFlags)
                .HasForeignKey(f => f.OrderId);

            builder.Entity<PaymentLog>()
                .HasOne(p => p.Order)
                .WithMany(o => o.PaymentLogs)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<RefundLog>()
                .HasOne<Order>()                 // 👈 navigation belirtmiyoruz
                .WithMany()                      // 👈 Order tarafında collection yok
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderTimeline>()
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<FraudFlag>()
                .HasOne(f => f.Order)
                .WithMany(o => o.FraudFlags)
                .HasForeignKey(f => f.OrderId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var domainEntities = ChangeTracker
                .Entries<BaseEntity>()
                .Where(x => x.Entity.DomainEvents.Any())
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            var eventBus = this.GetService<IEventBus>();

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
            foreach (var entity in domainEntities)
            {
                entity.Entity.ClearDomainEvents();
            }

            return result;
        }
    }
}