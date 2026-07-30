# Entity Template

## File: `ClinicHub.Domain/Entities/<EntityName>.cs`

```csharp
using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities;

public class <EntityName> : BaseEntity<Guid>
{
    // Private backing fields for collections
    private readonly List<ChildEntity> _children = [];

    // Parameterless constructor for EF Core
    private <EntityName>() { }

    public <EntityName>(string name, Guid? clinicId = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        ClinicId = clinicId;
    }

    // Properties with private setters
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Guid? ClinicId { get; private set; }

    // Collections exposed as IReadOnlyCollection
    public IReadOnlyCollection<ChildEntity> Children => _children.AsReadOnly();

    // Domain methods for state changes
    public void UpdateName(string newName)
    {
        Name = newName;
    }

    public void AddChild(ChildEntity child)
    {
        _children.Add(child);
    }

    public void RemoveChild(ChildEntity child)
    {
        _children.Remove(child);
    }
}
```

## Key Points

- Always inherit from `BaseEntity<Guid>` (or `BaseEntity` if no key property needed)
- Provide a private parameterless constructor for EF Core
- Use private setters to enforce domain invariants
- Collections: `private readonly List<T>` backing field, `IReadOnlyCollection<T>` public property
- Domain methods for mutation (avoid public setters where possible)
- Implement `IClinicScopedEntity` if entity is scoped to a clinic (`Guid? ClinicId`)