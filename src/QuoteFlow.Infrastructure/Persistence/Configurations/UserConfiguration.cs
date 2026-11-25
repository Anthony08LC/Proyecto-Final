using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteFlow.Domain.Entities;
using QuoteFlow.Domain.ValueObjects;

namespace QuoteFlow.Infrastructure.Persistence.Configurations;

/// <summary>Tabla <c>users</c> (sección 9.3).</summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint("ck_users_full_name_not_empty", "length(trim(full_name)) > 0");
            table.HasCheckConstraint("ck_users_role", "role IN ('Administrator', 'Sales')");
            table.HasCheckConstraint("ck_users_email_lowercase", "email = lower(email)");
            table.HasCheckConstraint(
                "ck_users_password_hash",
                "password_hash IS NOT NULL OR is_system");
        });

        builder.ConfigureAggregateRoot();

        builder.Property(u => u.FullName)
            .HasMaxLength(User.FullNameMaxLength)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(ValueObjectConverters.Email)
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(User.PasswordHashMaxLength);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.SecurityStamp).IsRequired();

        // El correo es único aunque el usuario esté inactivo (RN-30); se guarda en minúsculas.
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("uq_users_email");

        // Un único usuario de sistema.
        builder.HasIndex(u => u.IsSystem)
            .IsUnique()
            .HasFilter("is_system")
            .HasDatabaseName("uq_users_single_system_user");
    }
}
