using EShopMVC.Modules.Fraud.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EShopMVC.Modules.Fraud.Configurations
{
    public class FraudFlagConfiguration : IEntityTypeConfiguration<FraudFlag>
    {
        public void Configure(EntityTypeBuilder<FraudFlag> builder)
        {
            builder.HasIndex(x => x.IsResolved);

            builder.HasIndex(x => x.RuleCode);

            builder.HasIndex(x => new { x.IsResolved, x.Severity });
        }
    }
}