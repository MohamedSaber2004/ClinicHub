# EF Configuration Template

## File: `ClinicHub.Persistence/Configuration/<EntityName>Configuration.cs`

```csharp
using ClinicHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicHub.Persistence.Configuration;

public class <EntityName>Configuration : IEntityTypeConfiguration<<EntityName>>
{
    public void Configure(EntityTypeBuilder<<EntityName>> builder)
    {
        builder.ToTable("<TableName>");

        builder.HasKey(x => x.Id);

        // Required: Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Required: Concurrency token
        builder.Property(x => x.Version).IsRowVersion();

        // Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Relationships — use PropertyAccessMode.Field for IReadOnlyCollection backing fields
        builder.HasMany(x => x.Children)
            .WithOne(x => x.Parent)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Clinic scoping
        builder.HasQueryFilter(x => x.ClinicId == null);
    }
}
```

## Key Points

- Required: `HasQueryFilter(x => !x.IsDeleted)` — global soft-delete filter
- Required: `IsRowVersion()` on the `Version` property
- Use `OnDelete(DeleteBehavior.Restrict)` — never cascade deletes
- Use `PropertyAccessMode.Field` for `IReadOnlyCollection` backing fields
- Add clinic-scoped `HasQueryFilter` for `IClinicScopedEntity` entities