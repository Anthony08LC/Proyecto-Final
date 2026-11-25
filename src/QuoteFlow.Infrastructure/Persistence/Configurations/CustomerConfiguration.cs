using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuoteFlow.Domain.Entities;

namespace QuoteFlow.Infrastructure.Persistence.Configurations;

/// <summary>Tabla <c>customers</c> (sección 9.3). El correo no es único: solo se advierte (RF-CLI-01).</summary>
internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers", table =>
        {
            table.HasCheckConstraint("ck_customers_name_not_empty", "length(trim(name)) > 0");
            table.HasCheckConstraint("ck_customers_email_lowercase", "email IS NULL OR email = lower(email)");
            table.HasCheckConstraint("ck_customers_phone_format", @"phone IS NULL OR phone ~ '^\+506[0-9]{8}$'");
        });

        builder.ConfigureAggregateRoot();

        builder.Property(c => c.Name)
            .HasMaxLength(Customer.NameMaxLength)
            .IsRequired();

        builder.Property(c => c.Company).HasMaxLength(Customer.CompanyMaxLength);
        builder.Property(c => c.Email).IsEmail();
        builder.Property(c => c.Phone).IsPhone();
        builder.Property(c => c.Notes).HasMaxLength(Customer.NotesMaxLength);
    }
}
