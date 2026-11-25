using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Infrastructure.Persistence.Records;

namespace QuoteFlow.Infrastructure.Persistence.Configurations;

/// <summary>Tabla <c>audit_events</c> (sección 9.3): solo inserción y lectura.</summary>
internal sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events", table =>
            table.HasCheckConstraint(
                "ck_audit_events_entity_type",
                "entity_type IN ('User', 'Customer', 'Category', 'Product', 'Quote', 'SystemSettings', 'Session')"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityAlwaysColumn();

        builder.Property(e => e.OccurredAt)
            .HasDefaultValueSql("now()")
            .IsRequired();

        builder.Property(e => e.UserName).HasMaxLength(AuditEvent.UserNameMaxLength);
        builder.Property(e => e.EventType).HasMaxLength(AuditEvent.EventTypeMaxLength).IsRequired();
        builder.Property(e => e.EntityType).HasMaxLength(AuditEvent.EntityTypeMaxLength).IsRequired();
        builder.Property(e => e.Detail).HasMaxLength(AuditEvent.DetailMaxLength).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices (sección 9.5).
        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.OccurredAt });
        builder.HasIndex(e => e.OccurredAt).IsDescending();
        builder.HasIndex(e => new { e.UserId, e.OccurredAt }).IsDescending(false, true);
    }
}
